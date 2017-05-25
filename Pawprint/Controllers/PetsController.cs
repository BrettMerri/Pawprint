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
    public class PetsController : Controller
    {
        public ActionResult Profile(int? PetID)
        {
            PawprintEntities DB = new PawprintEntities();
            Pet PetProfile = DB.Pets.SingleOrDefault(x => x.PetID == PetID);

            if (PetProfile == null)
            {
                ViewBag.Message = "Invalid PetID";
                return View("Error");
            }

            if (Request.IsAuthenticated)
            {
                string CurrentUserID = User.Identity.GetUserId();
                bool IsUserAlreadyFollowingThisPet = DB.FollowLists.Any(x => x.PetID == PetProfile.PetID &&
                                                                    x.UserID == CurrentUserID);

                if (CurrentUserID == PetProfile.OwnerID)
                {
                    ViewBag.EditProfile = true;
                }
                else
                {
                    ViewBag.EditProfile = false;

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
                ViewBag.EditProfile = false;
                ViewBag.AlreadyFollowing = false;
            }

            List<Post> PostList = DB.Posts.Where(x => x.PetID == PetProfile.PetID)
                                          .OrderByDescending(x => x.Date)
                                          .ToList();

            ViewBag.PostList = PostList;

            if (TempData["Message"] != null)
            {
                ViewBag.Message = TempData["Message"];
            }

            return View(PetProfile);
        }

        [Authorize]
        public ActionResult Follow(int PetID)
        {
            PawprintEntities DB = new PawprintEntities();
            Pet FollowPet = DB.Pets.SingleOrDefault(x => x.PetID == PetID);

            string CurrentUserID = User.Identity.GetUserId();

            if (FollowPet == null)
            {
                ViewBag.Message = "Invalid PetID!";
                return View("Error");
            }

            bool IsUserAlreadyFollowingThisPet = DB.FollowLists.Any(x => x.PetID == FollowPet.PetID &&
                                                                    x.UserID == CurrentUserID);

            //Checks if the user owns the pet OR if the user is already following this pet
            if (CurrentUserID == FollowPet.OwnerID || IsUserAlreadyFollowingThisPet)
            {
                TempData["Message"] = $"You are already following {FollowPet.Name}!";
                return RedirectToAction("Profile", new { PetID = FollowPet.PetID });
            }

            FollowList NewFollow = new FollowList();
            NewFollow.UserID = CurrentUserID;
            NewFollow.PetID = FollowPet.PetID;

            DB.FollowLists.Add(NewFollow);

            try
            {
                DB.SaveChanges();
            }
            catch (Exception ex)
            {
                ViewBag.Message = "Something odd just happened! " + ex.ToString();
                return View("Error");
            }

            TempData["Message"] = $"You have successfully followed {FollowPet.Name}!";

            return RedirectToAction("Profile", new { PetID = FollowPet.PetID });
        }

        [Authorize]
        public ActionResult Unfollow(int PetID)
        {
            PawprintEntities DB = new PawprintEntities();
            Pet FollowPet = DB.Pets.SingleOrDefault(x => x.PetID == PetID);

            string CurrentUserID = User.Identity.GetUserId();

            if (FollowPet == null)
            {
                ViewBag.Message = "Invalid PetID!";
                return View("Error");
            }

            bool IsUserAlreadyFollowingThisPet = DB.FollowLists.Any(x => x.PetID == FollowPet.PetID &&
                                                                    x.UserID == CurrentUserID);

            //Checks if the user owns the pet
            if (CurrentUserID == FollowPet.OwnerID)
            {
                TempData["Message"] = $"You are cannot unfollow your own pet!";
                return RedirectToAction("Profile", new { PetID = FollowPet.PetID });
            }

            //Checks if user does not currently follow this pet
            if (!IsUserAlreadyFollowingThisPet)
            {
                TempData["Message"] = $"You are already not following {FollowPet.Name}!";
                return RedirectToAction("Profile", new { PetID = FollowPet.PetID });
            }

            FollowList Unfollow = DB.FollowLists.Find(CurrentUserID, FollowPet.PetID);
            DB.FollowLists.Remove(Unfollow);

            try
            {
                DB.SaveChanges();
            }
            catch (Exception ex)
            {
                ViewBag.Message = "Something odd just happened! " + ex.ToString();
                return View("Error");
            }

            TempData["Message"] = $"You have successfully unfollowed {FollowPet.Name}!";

            return RedirectToAction("Profile", new { PetID = FollowPet.PetID });
        }

        // Add a New Post
        [Authorize]
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



        // Save New Post
        [HttpPost]
        [Authorize]
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


            // Time Stamp for Post
            NewPost.Date = DateTime.Now;


            //Create Unique Identifier
            string UniqueID = Guid.NewGuid().ToString().Replace("-", "");


            //This File Path Gets Saved To The Database
            NewPost.FilePath = $"{NewPost.PetID}/{UniqueID}/{uploadFile.FileName}";


            //This File Path Will Be Used To Save The File
            string FilePath = $"~/img/posts/{NewPost.PetID}/{UniqueID}";

            Upload(uploadFile, FilePath);

                DB.Posts.Add(NewPost);
                DB.SaveChanges();

            return RedirectToAction("Index", "Home");
        }



        // Upload Photo
        [HttpPost]
        [Authorize]
        public ActionResult Upload(HttpPostedFileBase file, string filePath)
        {
            if (file != null && file.ContentLength > 0)
                try
                {
                    DirectoryInfo dir = new DirectoryInfo(HttpContext.Server.MapPath(filePath));
                    if (!dir.Exists)
                    {
                        dir.Create();
                    }

                    string path = Path.Combine(Server.MapPath(filePath),
                                               Path.GetFileName(file.FileName));
                    file.SaveAs(path);
                }
                catch (Exception ex)
                {
                    ViewBag.Message = "ERROR:" + ex.Message.ToString();
                }
            else
            {
                ViewBag.Message = "You Have Not Specified a File.";
            }
            return View("Index");
        }

        [Authorize]
        public ActionResult EditPetProfile(int PetID)
        {
            
            return View("YourAnimals", "User" );

        }

        public ActionResult UploadPetAvatar(int PetID)
        {
            PawprintEntities DB = new PawprintEntities();
            Pet AddAvatar = DB.Pets.SingleOrDefault(x => x.PetID == PetID);

            if (User.Identity.GetUserId() != AddAvatar.OwnerID)
            {
                ViewBag.Message = "Can Not Change Avatar for a Pet That is Not Your Own";
                return View("Error");
            }

            ViewBag.PetID = PetID;
            
            return View();
        }

        public ActionResult UploadAvatar(HttpPostedFileBase file, string filePath)
        {
            if (file != null && file.ContentLength > 0)
                try
                {
                    DirectoryInfo dir = new DirectoryInfo(HttpContext.Server.MapPath(filePath));
                    if (!dir.Exists)
                    {
                        dir.Create();
                    }

                    string path = Path.Combine(Server.MapPath(filePath),
                                               Path.GetFileName(file.FileName));
                    file.SaveAs(path);
                }
                catch (Exception ex)
                {
                    ViewBag.Message = "ERROR:" + ex.Message.ToString();
                }
            else
            {
                ViewBag.Message = "You Have Not Specified a File.";
            }
            return View("Index");
        }

        [HttpPost]
        public ActionResult SavePetAvatar(Pet AddAvatar, HttpPostedFileBase uploadFile)
        {
            PawprintEntities DB = new PawprintEntities();
            Pet AddPet = DB.Pets.Find(AddAvatar.PetID);

            if (User.Identity.GetUserId() != AddPet.OwnerID)
            {
                ViewBag.Message = "Can Not Change Avatar for a Pet That is Not Your Own";
                return View("Error");
            }

            //Create Unique Identifier
            string UniqueID = Guid.NewGuid().ToString().Replace("-", "");

            AddPet.FilePath = $"{AddAvatar.PetID}/{UniqueID}/{uploadFile.FileName}";


            string FilePath = $"~/img/pets/{AddAvatar.PetID}/{UniqueID}";

            UploadAvatar(uploadFile, FilePath);

            DB.SaveChanges();

            

            return RedirectToAction("Profile", new { PetID = AddPet.PetID });

        }




    }
}