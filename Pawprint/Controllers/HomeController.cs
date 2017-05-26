using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Pawprint.Models;
using Microsoft.AspNet.Identity;

namespace Pawprint.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            string CurrentUserID = User.Identity.GetUserId();

            if (!Request.IsAuthenticated)
            {
                return RedirectToAction("Welcome");
            }

            PawprintEntities PE = new PawprintEntities();
            List<int> FollowedPets = PE.FollowLists.Where(x => x.UserID == CurrentUserID).Select(x=> x.PetID).ToList();
            List<Post> PostList = PE.Posts.Where(x => FollowedPets.Any(y => x.PetID == y)).OrderByDescending(x => x.Date).ToList();

            ViewBag.PostList = PostList;

            return View();
        }

        public ActionResult Explore()
        {
            PawprintEntities PE = new PawprintEntities();

            List<Post> PostList = PE.Posts.OrderByDescending(x => x.Date).ToList();

            ViewBag.PostList = PostList;

            return View("Index");
        }

        public ActionResult Developers()
        {
            return View();
        }

        public ActionResult Welcome()
        {
            return View();
        }
    }
}