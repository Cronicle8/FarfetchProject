using DeliveryService.Controllers;
using DeliveryService.Models.DataModels;
using Neo4jClient;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DeliveryService.Services
{
    public class ValuesRepository
    {
        public List<Paths> GetPaths(string origin, string destination, string userId, string password)
        {
            List<Paths> paths = new List<Paths>();

            GraphClient client = new GraphClient(new Uri("http://localhost:7474/db/data"), userId, password)
            {
                JsonContractResolver = new CamelCasePropertyNamesContractResolver()
            };

            client.Connect();

            var firstNodes = client.Cypher
                .Match("(n:Points)-[r]-(m:Points)")
                .Where((Nodes n) => n.name == origin)
                .Return((n, r, m) => new
                //.Return((n, r) => new
                {
                    node = n.As<Nodes>(),
                    nextNode = m.As<Nodes>(),
                    relations = r.CollectAs<Relations>()
                }).Results;

            for (var i = 0; i < firstNodes.Count(); i++)
            {
                List<string> points = new List<string>();

                points.Add(origin);
                points.Add(firstNodes.ElementAt(i).nextNode.name);

                paths.Add(new Paths
                {
                    points = points,
                    totalCost = firstNodes.ElementAt(i).relations.First().cost,
                    totalTime = firstNodes.ElementAt(i).relations.First().time
                });
            }
            
        recheckPaths:
            for (var i = 0; i < paths.Count; i++)
            {
                Paths currentPath = paths.ElementAt(i);
                string currentNode = currentPath.points.Last();

                if (currentPath.points.Contains(destination))
                {
                    currentPath.reachedDestination = true;
                }

                if (!currentPath.reachedDestination)
                {
                    bool addedFirst = false;

                    var childNodes = client.Cypher
                                .Match("(n:Points)-[r]-(m:Points)")
                                .Where((Nodes n) => n.name == currentNode)
                                .Return((n, r, m) => new
                                //.Return((n, r) => new
                                {
                                    node = n.As<Nodes>(),
                                    nextNode = m.As<Nodes>(),
                                    relation = r.As<Relations>()
                                }).Results;

                    for (var o = 0; o < childNodes.Count(); o++)
                    {
                        if (!currentPath.points.Contains(childNodes.ElementAt(o).nextNode.name) && !addedFirst)
                        {
                            currentPath.points.Add(childNodes.ElementAt(o).nextNode.name);
                            currentPath.totalCost += childNodes.ElementAt(o).relation.cost;
                            currentPath.totalTime += childNodes.ElementAt(o).relation.time;
                            addedFirst = true;
                            if (childNodes.ElementAt(o).nextNode.name == destination)
                            {
                                currentPath.reachedDestination = true;
                                break;
                            }
                        }
                        else
                        {
                            List<string> points = currentPath.points.ToList();

                            if (!points.Contains(childNodes.ElementAt(o).nextNode.name))
                            {
                                paths.Add(new Paths
                                {
                                    points = points,
                                    totalCost = firstNodes.ElementAt(o).relations.First().cost + childNodes.ElementAt(o).relation.cost,
                                    totalTime = firstNodes.ElementAt(o).relations.First().time + childNodes.ElementAt(o).relation.time
                                });

                                paths.Last().points.Remove(paths.Last().points.Last());
                                paths.Last().points.Add(childNodes.ElementAt(o).nextNode.name);
                            }
                        }
                    }

                    if (!addedFirst)
                        paths.Remove(currentPath);
                }
            }

            if (paths.Where(x => !x.reachedDestination).Count() > 0)
                goto recheckPaths;

            return paths;
        }
        public string CreateNode(Node node)
        {
            try
            {
                GraphClient client = new GraphClient(new Uri("http://localhost:7474/db/data"), node.userId, node.password)
                {
                    JsonContractResolver = new CamelCasePropertyNamesContractResolver()
                };

                client.Connect();

                Nodes newNode = new Nodes { name = node.name };
                client.Cypher
                    .Create("(Point" + node.name + ":Points {newNode})")
                    .WithParam("newNode", newNode)
                    .ExecuteWithoutResults();

                return "SUCCESS";
            }
            catch (Exception e)
            {
                return "ERROR: " + e.Message;
            }
        }
    }
    
    public class Paths
    {
        public List<string> points { get; set; } = new List<string>();
        public int totalCost { get; set; } = 0;
        public int totalTime { get; set; } = 0;
        public bool reachedDestination { get; set; } = false;
    }
    public class Nodes
    {
        public int id { get; set; }
        public string name { get; set; }
    }
    public class Relations
    {
        public int cost { get; set; }
        public int time { get; set; }
    }
}