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

namespace Pawprint.Controllers
{
    public class UserController : Controller
    {

        // /User/Profile/[username]
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
                        // to DO: Validation!!
            PawprintEntities PE = new PawprintEntities();
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

        public ActionResult EditUserProfile(string ID)
        {
            PawprintEntities PE = new PawprintEntities();
            AspNetUser ToFind = PE.AspNetUsers.Find(ID);
            return View("EditUserProfile");
        }

        public ActionResult SaveEditUserProfile(AspNetUser ToBeUpdated)
        {
            PawprintEntities PE = new PawprintEntities();
            AspNetUser ToFind = PE.AspNetUsers.Find(ToBeUpdated.ID);
            ToFind.Bio = ToBeUpdated.Bio;
            ToFind.Location = ToBeUpdated.Location;
            ToFind.Gender = ToBeUpdated.Gender;
            ToFind.BirthDay = ToBeUpdated.BirthDay;

            PE.SaveChanges();
            ApplicationDbContext UserDB = new ApplicationDbContext();
            ApplicationUser CurrentUserInfo = UserDB.Users.Find(User.Identity.GetUserId());
            return RedirectToAction("Profile", new { DisplayName = CurrentUserInfo.DisplayName });

        }
    }
}



