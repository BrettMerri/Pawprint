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
            //If user is not logged in, redirect them to the Welcome page.
            if (!Request.IsAuthenticated)
            {
                return RedirectToAction("Welcome");
            }

            string CurrentUserID = User.Identity.GetUserId();

            PawprintEntities PE = new PawprintEntities();

            //Get distinct int list of PetID's that the user follows
            List<int> FollowedPets = PE.FollowLists.Where(x => x.UserID == CurrentUserID).Select(x=> x.PetID).Distinct().ToList();

            //Get all posts where Posts.PetID == any PetID that the user follows
            List<Post> PostList = PE.Posts.Where(x => FollowedPets.Any(y => x.PetID == y)).OrderByDescending(x => x.Date).ToList();

            ViewBag.PostList = PostList;

            //TempData["Message"] exists when the user follows/unfollows the pet
            if (TempData["Message"] != null)
            {
                ViewBag.Message = TempData["Message"];
            }

            return View();
        }

        public ActionResult Explore()
        {
            PawprintEntities PE = new PawprintEntities();

            //Returns list of all posts
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