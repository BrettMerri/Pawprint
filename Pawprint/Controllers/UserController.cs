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

        // Your Animals Page - Shows List of Pets
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
        public ActionResult SaveNewPet(Pet NewPet)
        {
            NewPet.OwnerID = User.Identity.GetUserId();

            PawprintEntities PE = new PawprintEntities();

            // to DO: Validation!!

            PE.Pets.Add(NewPet);
            PE.SaveChanges();

            ApplicationDbContext UserDB = new ApplicationDbContext();
            ApplicationUser CurrentUserInfo = UserDB.Users.Find(User.Identity.GetUserId());

            return RedirectToAction("Profile", new { DisplayName = CurrentUserInfo.DisplayName });
        }

        // Delete a Pet
        public ActionResult DeletePet(int PetID)
        {
            PawprintEntities DB = new PawprintEntities();
            Pet SelectedPet = DB.Pets.SingleOrDefault(x => x.PetID == PetID);

            DB.Pets.Remove(SelectedPet);
            DB.SaveChanges();

            return RedirectToAction("YourAnimals");
        }

        // Update a Pet
        public ActionResult UpdatePet(int PetID)
        {
            PawprintEntities PE = new PawprintEntities();
            Pet ToFind = PE.Pets.Find(PetID);

            return View("UpdatePet");
        }

        // Save Updates for Pet
        public ActionResult SaveUpdates(Pet ToBeUpdated)
        {
            PawprintEntities PE = new PawprintEntities();
            Pet ToFind = PE.Pets.Find(ToBeUpdated.PetID);

            ToFind.Breed = ToBeUpdated.Breed;
            ToFind.Name = ToBeUpdated.Name;
            ToFind.Color = ToBeUpdated.Color;
            ToFind.BirthDay = ToBeUpdated.BirthDay;
            ToFind.FavoriteFood = ToBeUpdated.FavoriteFood;

            PE.SaveChanges();

            return RedirectToAction("YourAnimals");
        }

        public ActionResult EditUserProfile()
        {
            string CurrentUserID = User.Identity.GetUserId();

            PawprintEntities PE = new PawprintEntities();
            AspNetUser ToFind = PE.AspNetUsers.Find(CurrentUserID);

            return View(ToFind);
        }

        public ActionResult SaveEditUserProfile(AspNetUser ToBeUpdated)
        {
            string CurrentUserID = User.Identity.GetUserId();

            PawprintEntities PE = new PawprintEntities();
            AspNetUser ToFind = PE.AspNetUsers.Find(CurrentUserID);

            ToFind.Bio = ToBeUpdated.Bio;
            ToFind.Location = ToBeUpdated.Location;
            ToFind.Gender = ToBeUpdated.Gender;
            ToFind.BirthDay = ToBeUpdated.BirthDay;

            PE.SaveChanges();

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

            //Create Unique Identifier
            string UniqueID = Guid.NewGuid().ToString().Replace("-", "");

            
            AddAvatar.FilePath = $"{AddAvatar.ID}/{UniqueID}/{uploadFile.FileName}";


            string FilePath = $"~/img/users/{AddAvatar.ID}/{UniqueID}";

            UploadAvatar(uploadFile, FilePath);

            DB.SaveChanges();

            ApplicationDbContext UserDB = new ApplicationDbContext();
            ApplicationUser CurrentUserInfo = UserDB.Users.Find(User.Identity.GetUserId());

            return RedirectToAction("Profile", new { DisplayName = CurrentUserInfo.DisplayName });

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




    }
}