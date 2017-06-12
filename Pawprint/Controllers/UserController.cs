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

            List<Pet> PetList = DB.Pets.Where(x => x.OwnerID == UserProfile.ID).ToList(); //Finds all pets that the user owns

            List<Pet> PetsFollowed = (from pets in DB.Pets
                                join follow in DB.FollowLists on pets.PetID equals follow.PetID
                                where follow.UserID == UserProfile.ID && pets.OwnerID != UserProfile.ID
                                select pets).Take(8).ToList(); //Finds all pets that the user follows

            //PetsFollowed.RemoveAll(x => PetList.Contains(x)); //Removes all pets from the list that the user owns

            ViewBag.PetList = PetList;
            ViewBag.PetsFollowed = PetsFollowed;

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


        public JsonResult Comment(int PostID, string CommentInput)
        {


            string CurrentUserID = User.Identity.GetUserId();
            ApplicationDbContext UserDB = new ApplicationDbContext();

            PawprintEntities PE = new PawprintEntities();

            AspNetUser CurrentUser = PE.AspNetUsers.Find(CurrentUserID);

            Post PostToCommentOn = PE.Posts.Find(PostID);

            if (PostToCommentOn == null)
            {
                return Json(true, JsonRequestBehavior.AllowGet);
            }

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
            catch (Exception)
            {
                return Json(true, JsonRequestBehavior.AllowGet);
            }

            JsonComment NewJsonComment = new JsonComment();
            NewJsonComment.DisplayName = CurrentUser.DisplayName;
            NewJsonComment.Text = NewComment.Text;
            NewJsonComment.PostID = NewComment.PostID;
            NewJsonComment.CommentID = NewComment.CommentID;

            if (CurrentUser.FilePath == null)
            {
                NewJsonComment.FilePath = "default_avatar.png";
            }
            else
            {
                NewJsonComment.FilePath = CurrentUser.FilePath;
            }

            return Json(NewJsonComment, JsonRequestBehavior.AllowGet);
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

        public string DeleteComment(int CommentID)
        {
            PawprintEntities PE = new PawprintEntities();
            Comment CommentToDelete = PE.Comments.Find(CommentID);

            if (CommentToDelete == null)
            {
                ViewBag.Message = "Unable to find comment to delete!";
                return "Error";
            }

            if (CommentToDelete.UserID != User.Identity.GetUserId())
            {
                return "Error";
            }

            try
            {
                PE.Comments.Remove(CommentToDelete);
                PE.SaveChanges();
            }
            catch (Exception)
            {
                return "Error";
            }

            return "Success";
        }

        public ActionResult YourAnimals()
        {
            PawprintEntities DB = new PawprintEntities();

            string UserID = User.Identity.GetUserId();
            List<Pet> PetList = DB.Pets.Where(x => x.OwnerID == UserID).ToList(); //creates list of pets where ownerID is equal to UserID

            ViewBag.PetList = PetList;  //viewbag so you can output list to YourAnimals view

            return View();
        }

        public ActionResult AddNewPet()
        {
            string CurrentUserID = User.Identity.GetUserId();
            ApplicationDbContext UserDB = new ApplicationDbContext();
            ApplicationUser CurrentUserInfo = UserDB.Users.Find(CurrentUserID);
            ViewBag.DisplayName = CurrentUserInfo.DisplayName;


            return View();  //returns the addnewpet view which has the form for user to fill out
        }

        [HttpPost]
        public ActionResult SaveNewPet(Pet NewPet)
        {
            string CurrentUserID = User.Identity.GetUserId();
            NewPet.OwnerID = CurrentUserID;        //makes sure ownerId = to USerID so only user can save pet for own profile
            NewPet.CreationDate = DateTime.Now;

            PawprintEntities PE = new PawprintEntities();
            
            FollowList FollowYourNewPet = new FollowList();

            try
            {
                PE.Pets.Add(NewPet);  //adds new pet to database using entity

                PE.SaveChanges();     // saves pet changes to database 
            }
            catch (Exception ex)
            {
                ViewBag.Message = "Unable to save new pet. " + ex.Message.ToString();
                return View("Error");
            }

            try
            {
                FollowYourNewPet.PetID = NewPet.PetID;           //when user creates new pet, pet is now automatically being followed by user.
                FollowYourNewPet.UserID = CurrentUserID;
                PE.FollowLists.Add(FollowYourNewPet);
                PE.SaveChanges(); // saves followlist changes to database
            }
            catch (Exception ex)
            {
                ViewBag.Message = "Unable to follow your pet. " + ex.Message.ToString();
                return View("Error");
            }

            ApplicationDbContext UserDB = new ApplicationDbContext();
            ApplicationUser CurrentUserInfo = UserDB.Users.Find(CurrentUserID); //here only to find out which user profile to direct back to, using entity

            return RedirectToAction("Profile", new { DisplayName = CurrentUserInfo.DisplayName });
        }

        public ActionResult DeletePet(int PetID)
        {
            PawprintEntities DB = new PawprintEntities();
            Pet SelectedPet = DB.Pets.SingleOrDefault(x => x.PetID == PetID); //select pet youd want to delete 

            if (SelectedPet.OwnerID != User.Identity.GetUserId()) //validation so only owner can delete their own pets
            {
                ViewBag.Message = "You cannot delete another user's pet!"; 
                return View("Error");
            }

            List<Post> PetPosts = DB.Posts.Where(x => x.PetID == SelectedPet.PetID).ToList();

            try
            {
                //Remove all pets posts before deleting pet or else app will give you an error
                foreach (var post in PetPosts)
                {
                    DB.Posts.Remove(post);
                }

                DB.SaveChanges(); //saves changes to database
            }
            catch (Exception ex)
            {
                ViewBag.Message = "Unable to delete pet's posts. " + ex.Message.ToString();
                return View("Error");
            }

            try
            {
                DB.Pets.Remove(SelectedPet);  //removes pet from database
                DB.SaveChanges(); //saves change to database
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
            Pet ToFind = PE.Pets.Find(PetID); //finds pet you need to update

            if (ToFind.OwnerID != User.Identity.GetUserId()) //validation so only owner can update pet
            {
                ViewBag.Message = "You cannot edit another user's pet";
                return View("Error");
            }

            return View(ToFind); //pulls pet info from model
        }

        [HttpPost]
        public ActionResult SaveUpdates(Pet ToBeUpdated)
        {
            PawprintEntities PE = new PawprintEntities();
            Pet ToFind = PE.Pets.Find(ToBeUpdated.PetID); //finds pet using tobeupdated petID

            if (ToFind.OwnerID != User.Identity.GetUserId())
            {
                ViewBag.Message = "You cannot edit another user's pet";
                return View("Error");
            }

            try
            {
                ToFind.Breed = ToBeUpdated.Breed;  //using entity to update collumn in table
                ToFind.Name = ToBeUpdated.Name;
                ToFind.Color = ToBeUpdated.Color;
                ToFind.BirthDay = ToBeUpdated.BirthDay;
                ToFind.FavoriteFood = ToBeUpdated.FavoriteFood;

                PE.SaveChanges(); //saves changes to database
            }
            catch (Exception ex)
            {
                ViewBag.Message = "Unable to update pet. " + ex.Message.ToString();
                return View("Error");
            }

            return RedirectToAction("Profile", "Pets", new { PetID = ToFind.PetID }); //redirects to the pet profile you were updating
        }

        public ActionResult EditUserProfile()
        {
            string CurrentUserID = User.Identity.GetUserId();

            PawprintEntities PE = new PawprintEntities();
            AspNetUser ToFind = PE.AspNetUsers.Find(CurrentUserID); //gets the user ID for user that wants to update profile

            if (ToFind == null)
            {
                ViewBag.Message = "Unable to edit profile.";
                return View("Error");
            }

            return View(ToFind); //pulls user info from model so form is not empty
        }

        [HttpPost]
        public ActionResult SaveEditUserProfile(AspNetUser ToBeUpdated)
        {
            string CurrentUserID = User.Identity.GetUserId();

            PawprintEntities PE = new PawprintEntities();
            AspNetUser ToFind = PE.AspNetUsers.Find(CurrentUserID); //finds user using identity

            if (ToFind == null) //if ID = null then unable to edit profile
            {
                ViewBag.Message = "Unable to edit profile.";
                return View("Error");
            }

            try
            {
                ToFind.Bio = ToBeUpdated.Bio;       //using entity to update columns on table
                ToFind.Location = ToBeUpdated.Location;
                ToFind.Gender = ToBeUpdated.Gender;
                ToFind.BirthDay = ToBeUpdated.BirthDay;

                PE.SaveChanges();  //saves changes to database
            }
            catch (Exception ex)
            {
                ViewBag.Message = "Unable to update profile. " + ex.Message.ToString();
                return View("Error");
            }

            ApplicationDbContext UserDB = new ApplicationDbContext();
            ApplicationUser CurrentUserInfo = UserDB.Users.Find(CurrentUserID);

            return RedirectToAction("Profile", new { DisplayName = CurrentUserInfo.DisplayName }); //redirects to user profile you were updating
        }

        public ActionResult UploadUserAvatar()
        {
            string CurrentUserID = User.Identity.GetUserId();
            ApplicationDbContext UserDB = new ApplicationDbContext();
            ApplicationUser CurrentUserInfo = UserDB.Users.Find(CurrentUserID);
            ViewBag.DisplayName = CurrentUserInfo.DisplayName;

            return View();  //returns the view of uploaduseravatar which has the upload link
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
                    dir.Create(); //Creates new directory if one doesn't exist
                }

                string path = Path.Combine(Server.MapPath(filePath), Path.GetFileName(file.FileName));

                file.SaveAs(path); //Saves image to project folder
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