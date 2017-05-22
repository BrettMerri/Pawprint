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
    }
}