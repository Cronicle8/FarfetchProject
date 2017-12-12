using DeliveryService.Models.DataModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace DeliveryService.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Title = "Home Page";
            Session["userProfile"] = new UserVO() { userId = "control", password = "123", role = "ADMIN" };

            return View();
        }
        public ActionResult ReadOnly()
        {
            Session["userProfile"] = new UserVO() { userId = "monitor", password = "Neo4j", role = "READONLY" };

            return View("Index");
        }
        public ActionResult Admin()
        {
            Session["userProfile"] = new UserVO() { userId = "control", password = "123", role = "ADMIN" };

            return View("Index");
        }
        public async Task<ActionResult> GetPaths(string origin, string destination)
        {
            string Baseurl = Request.Url.Scheme + "://" + Request.Url.Authority + Request.ApplicationPath.TrimEnd('/') + "/";
            UserVO user = (UserVO)Session["userProfile"];
            string[] paths = new string[10];

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(Baseurl);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage Res = await client.GetAsync("api/Values/Get?origin=" + origin + "&destination=" + destination + "&userId=" + user.userId + "&password=" + user.password);

                if (Res.IsSuccessStatusCode)
                {
                    var apiResponse = Res.Content.ReadAsStringAsync().Result;
                    paths = JsonConvert.DeserializeObject<string[]>(apiResponse);

                    ViewData["paths"] = paths;
                }
                return View("Index", ViewData);
            }
        }
        public async Task<ActionResult> SetNode(string name)
        {
            string Baseurl = Request.Url.Scheme + "://" + Request.Url.Authority + Request.ApplicationPath.TrimEnd('/') + "/";
            UserVO user = (UserVO)Session["userProfile"];
            string message = string.Empty;

            if (user.role == "ADMIN")
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(Baseurl);
                    client.DefaultRequestHeaders.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    Node node = new Node
                    {
                        name = name,
                        userId = user.userId,
                        password = user.password
                    };

                    HttpResponseMessage Res = await client.PostAsJsonAsync("api/Values/Post", node);

                    if (Res.IsSuccessStatusCode)
                    {
                        var apiResponse = Res.Content.ReadAsStringAsync().Result;
                        //message = JsonConvert.DeserializeObject<string>(apiResponse);

                        ViewData["message"] = "SUCCESS";
                    }
                    else
                    {
                        ViewData["message"] = "ERROR";
                    }
                }
            }
            else
            {
                ViewData["message"] = "Only administrators can create new nodes";
            }

            return View("Index", ViewData);
        }
    }
    public class Node
    {
        public string name { get; set; }
        public string userId { get; set; }
        public string password { get; set; }
    }
}
