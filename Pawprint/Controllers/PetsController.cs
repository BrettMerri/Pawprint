using Pawprint.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Pawprint.Controllers
{
    [Authorize]
    public class PetsController : Controller
    {
        [AllowAnonymous]
        public new ActionResult Profile(int? PetID)
        {
            PawprintEntities DB = new PawprintEntities();
            Pet PetProfile = DB.Pets.SingleOrDefault(x => x.PetID == PetID);

            //If there are no pets found with that PetID
            if (PetProfile == null)
            {
                ViewBag.Message = "Invalid PetID";
                return View("Error");
            }

            //If user is logged in
            if (Request.IsAuthenticated)
            {
                string CurrentUserID = User.Identity.GetUserId();

                AspNetUser CurrentUser = DB.AspNetUsers.Find(CurrentUserID);
                ViewBag.CurrentUser = CurrentUser;
                
                //If user is following this pet
                bool IsUserAlreadyFollowingThisPet = DB.FollowLists.Any(x => x.PetID == PetProfile.PetID &&
                                                                    x.UserID == CurrentUserID);

                //If user owns this pet
                if (CurrentUserID == PetProfile.OwnerID)
                {
                    ViewBag.EditProfile = true;
                }
                else
                {
                    ViewBag.EditProfile = false;

                    //If user is following this pet
                    if (IsUserAlreadyFollowingThisPet)
                    {
                        ViewBag.AlreadyFollowing = true;
                    }
                    else
                    {
                        ViewBag.AlreadyFollowing = false;
                    }
                }
            }
            else
            {
                //If user is not logged in
                ViewBag.EditProfile = false;
                ViewBag.AlreadyFollowing = false;
            }

            //Get all of this pet's posts
            List<Post> PostList = DB.Posts.Where(x => x.PetID == PetProfile.PetID)
                                          .OrderByDescending(x => x.Date)
                                          .ToList();

            if (User.Identity.IsAuthenticated)
            {
                ViewBag.LikedPostIds = LikedPosts(PostList);
            }

            ViewBag.PostList = PostList;

            //TempData["Message"] exists when the user follows/unfollows the pet
            if (TempData["Message"] != null)
            {
                ViewBag.Message = TempData["Message"];
            }

            return View(PetProfile);
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

        public string Follow(int PetID)
        {
            PawprintEntities DB = new PawprintEntities();
            Pet FollowPet = DB.Pets.SingleOrDefault(x => x.PetID == PetID);

            string CurrentUserID = User.Identity.GetUserId();

            //If there are no pets found with that PetID
            if (FollowPet == null)
            {
                return "No pets found with that PetID!";
            }

            //If user is following this pet
            bool IsUserAlreadyFollowingThisPet = DB.FollowLists.Any(x => x.PetID == FollowPet.PetID &&
                                                                    x.UserID == CurrentUserID);

            //Checks if the user owns the pet OR if the user is already following this pet
            if (CurrentUserID == FollowPet.OwnerID || IsUserAlreadyFollowingThisPet)
            {
                return "You are already following this pet!";
            }

            FollowList NewFollow = new FollowList();
            NewFollow.UserID = CurrentUserID;
            NewFollow.PetID = FollowPet.PetID;

            DB.FollowLists.Add(NewFollow);

            try
            {
                DB.SaveChanges();
            }
            catch (Exception)
            {
                return "Something odd happened while trying to follow this pet";
            }
            return "Success";
        }

        public string Unfollow(int PetID)
        {
            PawprintEntities DB = new PawprintEntities();
            Pet FollowPet = DB.Pets.SingleOrDefault(x => x.PetID == PetID);

            string CurrentUserID = User.Identity.GetUserId();

            //If there are no pets found with that PetID
            if (FollowPet == null)
            {
                return "No pets found with that PetID!";
            }

            //If user is following this pet
            bool IsUserAlreadyFollowingThisPet = DB.FollowLists.Any(x => x.PetID == FollowPet.PetID &&
                                                                    x.UserID == CurrentUserID);

            //Checks if the user owns the pet
            if (CurrentUserID == FollowPet.OwnerID)
            {
                return "You cannot unfollow your own pet!";
            }

            //Checks if user does not currently follow this pet
            if (!IsUserAlreadyFollowingThisPet)
            {
                return "You are already not following this pet!";
            }

            try
            {
                FollowList Unfollow = DB.FollowLists.Find(CurrentUserID, FollowPet.PetID);
                DB.FollowLists.Remove(Unfollow);
                DB.SaveChanges();
            }
            catch (Exception)
            {
                return "Something odd happened while trying to unfollow this pet!";
            }

            return "Success";
        }


        public ActionResult AddNewPost(int PetID)
        {
            PawprintEntities DB = new PawprintEntities();
            Pet AddPet = DB.Pets.SingleOrDefault(x => x.PetID == PetID);

            // When User ID and Pet Owner ID Do Not Match
            if (User.Identity.GetUserId() != AddPet.OwnerID)
            {
                ViewBag.Message = "Can Not Post Under a Pet That is Not Your Own";
                return View("Error");
            }

            ViewBag.PetID = PetID;
            return View();
        }

        [HttpPost]
        public ActionResult SaveNewPost(Post NewPost, HttpPostedFileBase uploadFile)
        {
            PawprintEntities DB = new PawprintEntities();
            Pet AddPet = DB.Pets.SingleOrDefault(x => x.PetID == NewPost.PetID);

            // When User ID and Pet Owner ID Do Not Match
            if (User.Identity.GetUserId() != AddPet.OwnerID)
            {
                ViewBag.Message = "Can Not Post Under a Pet That is Not Your Own";
                return View("Error");
            }

            //Validates that the file uploaded is an image
            if (!HttpPostedFileBaseExtensions.IsImage(uploadFile))
            {
                ViewBag.Message = "File uploaded is not an image";
                return View("Error");
            }

            // Time Stamp for Post
            NewPost.Date = DateTime.Now;

            // Initiate new post with a like count of 0
            NewPost.LikeCounts = 0;

            //Create Unique Identifier
            //This will be used as a folder name that the image will be saved to
            //Prevents images with the same name being saved in the same location
            string UniqueID = Guid.NewGuid().ToString().Replace("-", "");

            //This File Path Gets Saved To The Database
            //[PetID]/[UniqueID]/[Filename]
            NewPost.FilePath = $"{NewPost.PetID}/{UniqueID}/{uploadFile.FileName}";

            //This File Path Will Be Used To Save The File
            //~/img/users/[PetID]/[UniqueID]
            string FilePath = $"~/img/posts/{NewPost.PetID}/{UniqueID}";

            //Saves post to database
            DB.Posts.Add(NewPost);

            try
            {
                DB.SaveChanges();
            }
            catch (Exception ex)
            {
                ViewBag.Message = "Unable to save post " + ex.Message.ToString();
                return View("Error");
            }

            //Uploads file to project folder
            Upload(uploadFile, FilePath);

            TempData["Message"] = "Post uploaded successfully!";

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public ActionResult Upload(HttpPostedFileBase file, string filePath)
        {
                try
                {
                    DirectoryInfo dir = new DirectoryInfo(HttpContext.Server.MapPath(filePath));  
                    if (!dir.Exists)
                    {
                        dir.Create();            //creates new directory for uploaded image
                    }

                    string path = Path.Combine(Server.MapPath(filePath), Path.GetFileName(file.FileName));
                    file.SaveAs(path);    //saves complete filepath
                }
                catch (Exception ex)
                {
                    ViewBag.Message = "ERROR:" + ex.Message.ToString();
                    return View("Error");
                }

            return View("Index");
        }

        public ActionResult UploadPetAvatar(int PetID)
        {
            PawprintEntities DB = new PawprintEntities();
            Pet AddAvatar = DB.Pets.SingleOrDefault(x => x.PetID == PetID); //gets the petID of the pet you want to upload avatar

            if (User.Identity.GetUserId() != AddAvatar.OwnerID)  //validation so you cant upload avatar for pet you dont own
            {
                ViewBag.Message = "Can Not Change Avatar for a Pet That is Not Your Own";
                return View("Error");
            }

            ViewBag.PetID = PetID;
            
            return View();  //returns the uploadpetavatar view which has the upload form 
        }

        [HttpPost]
        public ActionResult SavePetAvatar(Pet AddAvatar, HttpPostedFileBase uploadFile)
        {
            PawprintEntities DB = new PawprintEntities();
            Pet AddPet = DB.Pets.Find(AddAvatar.PetID);

            if (User.Identity.GetUserId() != AddPet.OwnerID) //makes sure userID is equal to pet ID so no one else can change avatar
            {
                ViewBag.Message = "Can Not Change Avatar for a Pet That is Not Your Own";
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
            //[PetID]/[UniqueID]/[Filename]
            AddPet.FilePath = $"{AddAvatar.PetID}/{UniqueID}/{uploadFile.FileName}";

            //This File Path Will Be Used To Save The File
            //~/img/users/[PetID]/[UniqueID]
            string FilePath = $"~/img/pets/{AddAvatar.PetID}/{UniqueID}";

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

            Upload(uploadFile, FilePath);

            TempData["Message"] = "Avatar uploaded successfully!";

            return RedirectToAction("Profile", new { PetID = AddPet.PetID });
        }



        public ActionResult DeletePost(int PostID)
        {
            PawprintEntities DB = new PawprintEntities();
            Post SelectedPost = DB.Posts.SingleOrDefault(x => x.PostID == PostID);  //finds selected post that needs to be removed via lambda expression
            if (SelectedPost == null)
            {
                ViewBag.Message = "Unable to find post";
                return View("Error");

            }


            if (SelectedPost.Pet.OwnerID != User.Identity.GetUserId())  //validation so only owner can delete posts
            {
                ViewBag.Message = "You cannot delete another pets post!";
                return View("Error");
            }

            try
            {
                DB.Posts.Remove(SelectedPost);
                DB.SaveChanges(); // saves the change
            }
            catch (Exception ex)
            {
                ViewBag.Message = "Unable to delete post. " + ex.Message.ToString();
                return View("Error");       //returns error page
            }

            return RedirectToAction("Profile", new { PetID = SelectedPost.PetID });


        }
    }
}