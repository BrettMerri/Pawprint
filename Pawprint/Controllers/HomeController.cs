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

            //TempData["Message"] exists when the user follows/unfollows the pet
            if (TempData["Message"] != null)
            {
                ViewBag.Message = TempData["Message"];
            }

            return View();
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

            return View("Index");
        }

        public ActionResult Comment(int PostID, string CommentInput)
        {
            Comment NewComment = new Comment();

            string CurrentUserID = User.Identity.GetUserId();
            ApplicationDbContext UserDB = new ApplicationDbContext();
            ApplicationUser CurrentUser = UserDB.Users.Find(CurrentUserID);

            PawprintEntities PE = new PawprintEntities();

            try
            {
                NewComment.UserID = CurrentUserID;
                NewComment.Text = CommentInput;
                NewComment.PostID = PostID;
                PE.Comments.Add(NewComment);
                PE.SaveChanges();
            }
            catch (Exception ex)
            {
                ViewBag.Message = "Something went wrong! " + ex.Message.ToString();
                return View("Error");
            }

            return RedirectToAction("Index");
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

            ViewBag.PostList = SearchResults;
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