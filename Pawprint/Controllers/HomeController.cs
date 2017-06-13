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

            //Find current user view Entity Framework and send the AspNetUser object to the view.
            AspNetUser CurrentUser = PE.AspNetUsers.Find(CurrentUserID);
            ViewBag.CurrentUser = CurrentUser;

            //Gets list of posts from pets that you follow
            List<Post> PostList = GetFollowedPostList(PE, CurrentUserID);

            //Send list of posts of pets that you follow to the view
            ViewBag.PostList = PostList;

            //Sends list of four newest pets to the view
            ViewBag.NewestPets = GetNewestPets(PE);

            //Sends int list of PostID's that the user likes
            ViewBag.LikedPostIds = GetLikedPostIDs(PE, PostList, CurrentUserID);

            return View();
        }

        public List<Post> GetFollowedPostList(PawprintEntities PE, string CurrentUserID)
        {
            return (from post in PE.Posts
                    join follow in PE.FollowLists on post.PetID equals follow.PetID
                    where follow.UserID == CurrentUserID
                    select post).Take(10).ToList();
        }

        public List<Pet> GetNewestPets(PawprintEntities PE)
        {
            return PE.Pets.OrderByDescending(x => x.CreationDate).Take(4).ToList();
        }

        // Returns integer list of PostID's that the current user likes from a Post list of posts
        public List<int> GetLikedPostIDs(PawprintEntities PE, List<Post> PostList, string CurrentUserID)
        {
            // Declare new int List
            List<int> YouLike = new List<int>();

            // For each post, check if there is a record in the Like table which contains the UserID AND that post's PostID
            // If there is a match, add the post's PostID to the int List, YouLike
            foreach (Post item in PostList)
            {
                Like DoYouLike = PE.Likes.FirstOrDefault(x => x.PostID == item.PostID && x.UserID == CurrentUserID);

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

            // Sends list of posts to the view regardless of if you are following that pet or not
            List<Post> PostList = PE.Posts.OrderByDescending(x => x.Date).Take(10).ToList();
            ViewBag.PostList = PostList;

            // Sends list of the last four pets created to the view
            ViewBag.NewestPets = GetNewestPets(PE);

            // If user is logged in, send int list of liked Post ID's to the view
            if (User.Identity.IsAuthenticated)
            {
                ViewBag.LikedPostIds = GetLikedPostIDs(PE, PostList, CurrentUserID);
            }

            return View("Index");
        }

        public ActionResult Search(string SearchInput)
        {
            if (string.IsNullOrWhiteSpace(SearchInput))
            {
                return RedirectToAction("Explore");
            }

            PawprintEntities PE = new PawprintEntities();

            //Searches posts by Pet name, Breed, Color, Display name, and caption
            List<Post> SearchResults = PE.Posts.Where(x => x.Pet.Name.Contains(SearchInput) ||
                                                           x.Pet.Breed.Contains(SearchInput) ||
                                                           x.Pet.Color.Contains(SearchInput) ||
                                                           x.Pet.AspNetUser.DisplayName.Contains(SearchInput) ||
                                                           x.Caption.Contains(SearchInput))
                                                           .OrderByDescending(x => x.Date).Take(10).ToList();

            ViewBag.PostList = SearchResults;

            // If user is logged in, send the current user's AspNetUser object from Entity to the view
            if (User.Identity.IsAuthenticated)
            {
                string CurrentUserID = User.Identity.GetUserId();
                AspNetUser CurrentUser = PE.AspNetUsers.Find(CurrentUserID);
                ViewBag.CurrentUser = CurrentUser;

                // If user is logged in, send an int list of liked Post ID's to the view
                ViewBag.LikedPostIds = GetLikedPostIDs(PE, SearchResults, CurrentUserID);
            }

            //Sends list of four newest pets to the view
            ViewBag.NewestPets = GetNewestPets(PE);

            // Sends the search query to the view
            ViewBag.SearchInput = SearchInput;

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