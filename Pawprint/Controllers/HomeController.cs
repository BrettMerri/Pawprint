using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Pawprint.Models;

namespace Pawprint.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            PawprintEntities PE = new PawprintEntities();

            List<Post> PostList = PE.Posts.ToList();

            PostList.Reverse();

            ViewBag.PostList = PostList;

            return View();
        }

        public ActionResult Developers()
        {
            return View();
        }
    }
}