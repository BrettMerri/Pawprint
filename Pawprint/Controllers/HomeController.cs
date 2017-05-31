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

            AspNetUser CurrentUser = PE.AspNetUsers.Find(CurrentUserID);
            ViewBag.CurrentUser = CurrentUser;

            //Get distinct int list of PetID's that the user follows
            List<int> FollowedPets = PE.FollowLists.Where(x => x.UserID == CurrentUserID)
                .Select(x=> x.PetID).Distinct().ToList();

            //Get all posts where Posts.PetID == any PetID that the user follows
            List<Post> PostList = PE.Posts.Where(x => FollowedPets.Any(y => x.PetID == y))
                .OrderByDescending(x => x.Date).ToList();

            ViewBag.PostList = PostList;

            List<Pet> NewestPets = PE.Pets.OrderByDescending(x => x.CreationDate).Take(4).ToList();

            ViewBag.NewestPets = NewestPets;

            ViewBag.LikedPostIds = LikedPosts(PostList);

            return View();
        }

        public List<int> LikedPosts(List<Post> PostList)
        {
            List<int> YouLike = new List<int>();

            PawprintEntities PE = new PawprintEntities();
            string CurrentUserID = User.Identity.GetUserId();

            foreach (Post item in PostList)
            {
                Like DoYouLike = PE.Likes.SingleOrDefault(x => x.UserID == CurrentUserID && x.PostID == item.PostID);
                if (DoYouLike != null)
                {
                    YouLike.Add(item.PostID);
                }
            }
            return YouLike;
        }

        public ActionResult Explore()
        {
            string CurrentUserID = User.Identity.GetUserId();

            PawprintEntities PE = new PawprintEntities();

            AspNetUser CurrentUser = PE.AspNetUsers.Find(CurrentUserID);
            ViewBag.CurrentUser = CurrentUser;

            //Returns list of all posts
            List<Post> PostList = PE.Posts.OrderByDescending(x => x.Date).ToList();

            ViewBag.PostList = PostList;

            List<Pet> NewestPets = PE.Pets.OrderByDescending(x => x.CreationDate).Take(4).ToList();

            ViewBag.NewestPets = NewestPets;

            ViewBag.LikedPostIds = LikedPosts(PostList);

            return View("Index");
        }

        public ActionResult Search(string SearchInput)
        {
            if (string.IsNullOrWhiteSpace(SearchInput))
            {
                return RedirectToAction("Explore");
            }

            PawprintEntities PE = new PawprintEntities();

            List<Post> SearchResults = PE.Posts.Where(x => x.Pet.Name.Contains(SearchInput) ||
                                                           x.Pet.Breed.Contains(SearchInput) ||
                                                           x.Pet.Color.Contains(SearchInput) ||
                                                           x.Pet.AspNetUser.DisplayName.Contains(SearchInput) ||
                                                           x.Caption.Contains(SearchInput)).ToList();

            if (User.Identity.IsAuthenticated)
            {
                string CurrentUserID = User.Identity.GetUserId();
                AspNetUser CurrentUser = PE.AspNetUsers.Find(CurrentUserID);
                ViewBag.CurrentUser = CurrentUser;
            }


            List<Pet> NewestPets = PE.Pets.OrderByDescending(x => x.CreationDate).Take(4).ToList();

            ViewBag.NewestPets = NewestPets;
            ViewBag.PostList = SearchResults;
            ViewBag.SearchInput = SearchInput;

            if (User.Identity.IsAuthenticated)
            {
                ViewBag.LikedPostIds = LikedPosts(SearchResults);
            }

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