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
    public class UserController : Controller
    {
        public ActionResult Profile(string DisplayName)
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

        [Authorize]
        public ActionResult AddNewPet()
        {
            return View();
        }

        [Authorize]
        public ActionResult YourAnimals()
        {
            PawprintEntities DB = new PawprintEntities();

            string UserID = User.Identity.GetUserId();
            List<Pet> PetList = DB.Pets.Where(x => x.OwnerID == UserID).ToList();

            ViewBag.PetList = PetList;

            return View();
        }

        // Saves The New Pet
        [HttpPost]
        [Authorize]
        public ActionResult SaveNewPet(Pet NewPet)
        {
            NewPet.OwnerID = User.Identity.GetUserId();

            PawprintEntities PE = new PawprintEntities();

            // to DO: Validation!!

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

            ApplicationDbContext UserDB = new ApplicationDbContext();
            ApplicationUser CurrentUserInfo = UserDB.Users.Find(User.Identity.GetUserId());

            return RedirectToAction("Profile", new { DisplayName = CurrentUserInfo.DisplayName });
        }

        // Delete a Pet
        public ActionResult DeletePet(int PetID)
        {
            PawprintEntities DB = new PawprintEntities();
            Pet SelectedPet = DB.Pets.SingleOrDefault(x => x.PetID == PetID);

            if (SelectedPet.OwnerID != User.Identity.GetUserId())
            {
                ViewBag.Message = "You cannot delete another user's pet!";
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

        // Update a Pet
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

        // Save Updates for Pet
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
        [Authorize]
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

            //Create Unique Identifier
            string UniqueID = Guid.NewGuid().ToString().Replace("-", "");

            //This File Path Gets Saved To The Database
            AddAvatar.FilePath = $"{AddAvatar.ID}/{UniqueID}/{uploadFile.FileName}";

            //This File Path Will Be Used To Save The File
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
            ApplicationUser CurrentUserInfo = UserDB.Users.Find(User.Identity.GetUserId());

            return RedirectToAction("Profile", new { DisplayName = CurrentUserInfo.DisplayName });
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
                    return View("Error");
                }
            else
            {
                ViewBag.Message = "You Have Not Specified a File.";
                return View("Error");
            }
            return View("Index");
        }
    }
}