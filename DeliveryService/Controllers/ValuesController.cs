using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Neo4jClient;
using Newtonsoft.Json;
using Neo4j.Driver.V1;
using DeliveryService.Services;
using DeliveryService.Models.DataModels;

namespace DeliveryService.Controllers
{
    //[Authorize]
    public class ValuesController : ApiController
    {
        private ValuesRepository valuesService = new ValuesRepository();

        public string[] Get(string origin, string destination, string userId, string password)
        {
            List<Paths> paths = valuesService.GetPaths(origin,destination, userId, password);

            Paths quickestPath = paths.OrderBy(x => x.totalTime).First();
            Paths longestPath = paths.OrderByDescending(x => x.totalTime).First();

            string[] result = new string[paths.Count];

            for (var i = 0; i < result.Length; i++)
            {
                if (i == 0)
                {
                    result[i] = string.Format("Shortest way: {0} (Total cost: {1}, Total Time: {2})",
                                string.Join("", quickestPath.points),
                                quickestPath.totalCost,
                                quickestPath.totalTime);                    
                }
                else if (i == result.Length - 1)
                {
                    result[i] = string.Format("Longest way: {0} (Total cost: {1}, Total Time: {2})",
                                string.Join("", longestPath.points),
                                longestPath.totalCost,
                                longestPath.totalTime);
                }
                else
                {
                    result[i] = string.Format("Alternate way: {0} (Total cost: {1}, Total Time: {2})",
                                string.Join("", paths.OrderByDescending(x => x.totalTime).ElementAt(i).points),
                                paths.OrderByDescending(x => x.totalTime).ElementAt(i).totalCost,
                                paths.OrderByDescending(x => x.totalTime).ElementAt(i).totalTime);
                }
            }

            string Temp = result[1];
            result[1] = result[result.Length-1];
            result[result.Length - 1] = Temp;

            return result;
        }
        public HttpResponseMessage Post(Node node)
        {
            valuesService.CreateNode(node);

            var response = Request.CreateResponse<Node>(HttpStatusCode.Created, node);

            return response;
        }
    }    
}
