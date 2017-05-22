using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Pawprint.Models;

namespace Pawprint.Controllers
{
    public class UserController : Controller
    {
        // /User/Profile/[username]
        public ActionResult Profile(string Username)
        {
            PawprintEntities DB = new PawprintEntities();
            AspNetUser UserProfile = DB.AspNetUsers.SingleOrDefault(x => x.UserName == Username);
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

            return View();

        }


        // List of Your Animals
        public ActionResult YourAnimals()
        {
            PawprintEntities DB = new PawprintEntities();
            //Pet PetProfile = DB.Pets.SingleOrDefault(x => x.PetID == PetID);

            //if (PetProfile == null)
            //{
            //    return RedirectToAction("Index", "Home");
            //}


            List<Pet> PetList = DB.Pets.Where(x => x.PetID == Name.PetID).ToList();

            ViewBag.PetList = PetList;

            return View(PetProfile);
        }
    }
}


