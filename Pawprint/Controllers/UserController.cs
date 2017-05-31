using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Pawprint.Models;
using Microsoft.AspNet.Identity;
using System.Data.SqlClient;
using Microsoft.Owin.Security.Google;
using Owin;
using System.IO;

namespace Pawprint.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        [AllowAnonymous]
        public new ActionResult Profile(string DisplayName)
        {
            PawprintEntities DB = new PawprintEntities();
            AspNetUser UserProfile = DB.AspNetUsers.FirstOrDefault(x => x.DisplayName == DisplayName);
            if (UserProfile == null)
            {
                // Display Error Message When There's Invalid Display Name
                ViewBag.Message = "Invalid Display Name";
                return View("Error");
            }

            if (Request.IsAuthenticated)
            {
                ApplicationDbContext UserDB = new ApplicationDbContext();
                ApplicationUser CurrentUserInfo = UserDB.Users.Find(User.Identity.GetUserId());

                if (CurrentUserInfo == null)
                {
                    ViewBag.Message = "Unable to find current user";
                    return View("Error");
                }

                if (CurrentUserInfo.DisplayName == UserProfile.DisplayName)
                {
                    ViewBag.EditProfile = true;
                }
                else
                {
                    ViewBag.EditProfile = false;
                }
            }
            else
            {
                ViewBag.EditProfile = false;
            }

            List<Pet> PetList = DB.Pets.Where(x => x.OwnerID == UserProfile.ID).ToList();

            ViewBag.PetList = PetList;
            return View(UserProfile);
        }

        public string Like(int PostID)
        {
            string CurrentUserID = User.Identity.GetUserId();
            PawprintEntities PE = new PawprintEntities();

            Post PostToLike = PE.Posts.Find(PostID);
            
            if (PostToLike == null)
            {
                return "Unable to find post!";
            }

            if (PE.Likes.Any(x => x.AspNetUser.ID == CurrentUserID && x.PostID == PostID))
            {
                return "You already like this post!";
            }

            Like NewLike = new Like();
            
            try
            {
                PostToLike.LikeCounts++;
                NewLike.PostID = PostID;
                NewLike.UserID = CurrentUserID;
                PE.Likes.Add(NewLike);
                PE.SaveChanges();
            }
            catch (Exception ex)
            {
                return "Something odd just happened! " + ex.Message.ToString();
            }

            return "Success";
        }

        public string Unlike(int PostID)
        {
            string CurrentUserID = User.Identity.GetUserId();
            PawprintEntities PE = new PawprintEntities();

            Post PostToUnlike = PE.Posts.Find(PostID);

            if (PostToUnlike == null)
            {
                return "Unable to find post!";
            }

            if (!PE.Likes.Any(x => x.AspNetUser.ID == CurrentUserID && x.PostID == PostID))
            {
                return "You already don't like this post!";
            }

            Like RemoveLike = PE.Likes.SingleOrDefault(x => x.UserID == CurrentUserID && x.PostID == PostID);
            
            try
            {
                PostToUnlike.LikeCounts--;
                PE.Likes.Remove(RemoveLike);
                PE.SaveChanges();
            }
            catch (Exception ex)
            {
                return "Something odd just happened! " + ex.Message.ToString();
            }

            return "Success";
        }

        public ActionResult Comment(int PostID, string CommentInput)
        {
            string CurrentUserID = User.Identity.GetUserId();
            ApplicationDbContext UserDB = new ApplicationDbContext();
            ApplicationUser CurrentUser = UserDB.Users.Find(CurrentUserID);

            PawprintEntities PE = new PawprintEntities();

            Comment NewComment = new Comment();

            try
            {
                NewComment.CreationDate = DateTime.Now;
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

            return RedirectToAction("Index", "Home");
        }

        public ActionResult DeleteComment(int CommentID)
        {
            PawprintEntities PE = new PawprintEntities();
            Comment CommentToDelete = PE.Comments.Find(CommentID);

            if (CommentToDelete == null)
            {
                ViewBag.Message = "Unable to find comment to delete!";
                return View("Error");
            }

            if (CommentToDelete.UserID != User.Identity.GetUserId())
            {
                ViewBag.Message = "You cannot delete other user's comments!";
                return View("Error");
            }

            try
            {
                PE.Comments.Remove(CommentToDelete);
                PE.SaveChanges();
            }
            catch (Exception ex)
            {
                ViewBag.Message = "Something went wrong! " + ex.Message.ToString();
                return View("Error");
            }

            return RedirectToAction("Index", "Home");
        }

        public ActionResult YourAnimals()
        {
            PawprintEntities DB = new PawprintEntities();

            string UserID = User.Identity.GetUserId();
            List<Pet> PetList = DB.Pets.Where(x => x.OwnerID == UserID).ToList();

            ViewBag.PetList = PetList;

            return View();
        }

        public ActionResult AddNewPet()
        {
            return View();
        }

        [HttpPost]
        public ActionResult SaveNewPet(Pet NewPet)
        {
            string CurrentUserID = User.Identity.GetUserId();
            NewPet.OwnerID = CurrentUserID;
            NewPet.CreationDate = DateTime.Now;

            PawprintEntities PE = new PawprintEntities();

            FollowList FollowYourNewPet = new FollowList();

            try
            {
                PE.Pets.Add(NewPet);

                PE.SaveChanges();
            }
            catch (Exception ex)
            {
                ViewBag.Message = "Unable to save new pet. " + ex.Message.ToString();
                return View("Error");
            }

            try
            {
                FollowYourNewPet.PetID = NewPet.PetID;
                FollowYourNewPet.UserID = CurrentUserID;
                PE.FollowLists.Add(FollowYourNewPet);
                PE.SaveChanges();
            }
            catch (Exception ex)
            {
                ViewBag.Message = "Unable to follow your pet. " + ex.Message.ToString();
                return View("Error");
            }

            ApplicationDbContext UserDB = new ApplicationDbContext();
            ApplicationUser CurrentUserInfo = UserDB.Users.Find(CurrentUserID);

            return RedirectToAction("Profile", new { DisplayName = CurrentUserInfo.DisplayName });
        }

        public ActionResult DeletePet(int PetID)
        {
            PawprintEntities DB = new PawprintEntities();
            Pet SelectedPet = DB.Pets.SingleOrDefault(x => x.PetID == PetID);

            if (SelectedPet.OwnerID != User.Identity.GetUserId())
            {
                ViewBag.Message = "You cannot delete another user's pet!";
                return View("Error");
            }

            List<Post> PetPosts = DB.Posts.Where(x => x.PetID == SelectedPet.PetID).ToList();

            try
            {
                //Remove all pets posts
                foreach (var post in PetPosts)
                {
                    DB.Posts.Remove(post);
                }

                DB.SaveChanges(); 
            }
            catch (Exception ex)
            {
                ViewBag.Message = "Unable to delete pet's posts. " + ex.Message.ToString();
                return View("Error");
            }

            try
            {
                DB.Pets.Remove(SelectedPet);
                DB.SaveChanges();
            }
            catch (Exception ex)
            {
                ViewBag.Message = "Unable to delete pet. " + ex.Message.ToString();
                return View("Error");
            }

            return RedirectToAction("YourAnimals");
        }

        public ActionResult UpdatePet(int PetID)
        {
            PawprintEntities PE = new PawprintEntities();
            Pet ToFind = PE.Pets.Find(PetID);

            if (ToFind.OwnerID != User.Identity.GetUserId())
            {
                ViewBag.Message = "You cannot edit another user's pet";
                return View("Error");
            }

            return View(ToFind);
        }

        [HttpPost]
        public ActionResult SaveUpdates(Pet ToBeUpdated)
        {
            PawprintEntities PE = new PawprintEntities();
            Pet ToFind = PE.Pets.Find(ToBeUpdated.PetID);

            if (ToFind.OwnerID != User.Identity.GetUserId())
            {
                ViewBag.Message = "You cannot edit another user's pet";
                return View("Error");
            }

            try
            {
                ToFind.Breed = ToBeUpdated.Breed;
                ToFind.Name = ToBeUpdated.Name;
                ToFind.Color = ToBeUpdated.Color;
                ToFind.BirthDay = ToBeUpdated.BirthDay;
                ToFind.FavoriteFood = ToBeUpdated.FavoriteFood;

                PE.SaveChanges();
            }
            catch (Exception ex)
            {
                ViewBag.Message = "Unable to update pet. " + ex.Message.ToString();
                return View("Error");
            }

            return RedirectToAction("Profile", "Pets", new { PetID = ToFind.PetID });
        }

        public ActionResult EditUserProfile()
        {
            string CurrentUserID = User.Identity.GetUserId();

            PawprintEntities PE = new PawprintEntities();
            AspNetUser ToFind = PE.AspNetUsers.Find(CurrentUserID);

            if (ToFind == null)
            {
                ViewBag.Message = "Unable to edit profile.";
                return View("Error");
            }

            return View(ToFind);
        }

        [HttpPost]
        public ActionResult SaveEditUserProfile(AspNetUser ToBeUpdated)
        {
            string CurrentUserID = User.Identity.GetUserId();

            PawprintEntities PE = new PawprintEntities();
            AspNetUser ToFind = PE.AspNetUsers.Find(CurrentUserID);

            if (ToFind == null)
            {
                ViewBag.Message = "Unable to edit profile.";
                return View("Error");
            }

            try
            {
                ToFind.Bio = ToBeUpdated.Bio;
                ToFind.Location = ToBeUpdated.Location;
                ToFind.Gender = ToBeUpdated.Gender;
                ToFind.BirthDay = ToBeUpdated.BirthDay;

                PE.SaveChanges();
            }
            catch (Exception ex)
            {
                ViewBag.Message = "Unable to update profile. " + ex.Message.ToString();
                return View("Error");
            }

            ApplicationDbContext UserDB = new ApplicationDbContext();
            ApplicationUser CurrentUserInfo = UserDB.Users.Find(CurrentUserID);

            return RedirectToAction("Profile", new { DisplayName = CurrentUserInfo.DisplayName });
        }

        public ActionResult UploadUserAvatar()
        {
            return View();
        }

        [HttpPost]
        public ActionResult SaveUserAvatar(HttpPostedFileBase uploadFile)
        {
            string CurrentUserID = User.Identity.GetUserId();
            PawprintEntities DB = new PawprintEntities();

            AspNetUser AddAvatar = DB.AspNetUsers.Find(CurrentUserID);

            if (AddAvatar == null)
            {
                ViewBag.Message = "Unable to find user.";
                return View("Error");
            }

            //Validates that the file uploaded is an image
            if (!HttpPostedFileBaseExtensions.IsImage(uploadFile))
            {
                ViewBag.Message = "File uploaded is not an image";
                return View("Error");
            }

            //Create Unique Identifier
            //This will be used as a folder name that the image will be saved to
            //Prevents images with the same name being saved in the same location
            string UniqueID = Guid.NewGuid().ToString().Replace("-", "");

            //This File Path Gets Saved To The Database
            //[UserID]/[UniqueID]/[Filename]
            AddAvatar.FilePath = $"{AddAvatar.ID}/{UniqueID}/{uploadFile.FileName}";

            //This File Path Will Be Used To Save The File
            //~/img/users/[UserID]/[UniqueID]
            string FilePath = $"~/img/users/{AddAvatar.ID}/{UniqueID}";

            try
            {
                //Saves post to database
                DB.SaveChanges();
            }
            catch (Exception ex)
            {
                ViewBag.Message = "Unable to save avatar " + ex.Message.ToString();
                return View("Error");
            }

            //Uploads file to project folder
            Upload(uploadFile, FilePath);

            ApplicationDbContext UserDB = new ApplicationDbContext();
            ApplicationUser CurrentUserInfo = UserDB.Users.Find(CurrentUserID);

            TempData["Message"] = "Avatar uploaded successfully!";

            return RedirectToAction("Profile", new { DisplayName = CurrentUserInfo.DisplayName });
        }

        [HttpPost]
        public ActionResult Upload(HttpPostedFileBase file, string filePath)
        {
            try
            {
                DirectoryInfo dir = new DirectoryInfo(HttpContext.Server.MapPath(filePath));
                if (!dir.Exists)
                {
                    dir.Create();
                }

                string path = Path.Combine(Server.MapPath(filePath), Path.GetFileName(file.FileName));

                file.SaveAs(path);
            }
            catch (Exception ex)
            {
                ViewBag.Message = "An unexpected error occurred while uploading your image. " + ex.Message.ToString();
                return View("Error");
            }

            return View("Index");
        }
    }
}