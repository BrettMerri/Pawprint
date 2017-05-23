using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Pawprint.Models;
using Microsoft.AspNet.Identity;
using System.Data.SqlClient;


namespace Pawprint.Controllers
{
    public class UserController : Controller
    {

        // /User/Profile/[username]
        public ActionResult Profile(string DisplayName)
        {
            PawprintEntities DB = new PawprintEntities();
            AspNetUser UserProfile = DB.AspNetUsers.SingleOrDefault(x => x.DisplayName == DisplayName);
            if (UserProfile == null)
            {
                return RedirectToAction("Index", "Home");
            }

            List<Pet> PetList = DB.Pets.Where(x => x.OwnerID == UserProfile.ID).ToList();

            ViewBag.PetList = PetList;
            return View(UserProfile);
        }


        // Add New Pet to User Profile
        public ActionResult AddNewPet()
        {
            return View ();
        }



        // List of Your Animals
        public ActionResult YourAnimals()
        {
            PawprintEntities DB = new PawprintEntities();

            string UserID = User.Identity.GetUserId();
            List<Pet> PetList = DB.Pets.Where(x => x.OwnerID == UserID).ToList();

            ViewBag.PetList = PetList;

            return View();
        }

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


        public ActionResult DeletePet(int PetID)

        {
            PawprintEntities DB = new PawprintEntities();
            Pet SelectedPet = DB.Pets.SingleOrDefault(x => x.PetID == PetID);
            DB.Pets.Remove(SelectedPet);
            DB.SaveChanges();
            return RedirectToAction("YourAnimals");

        }
    }
}



