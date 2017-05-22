using Pawprint.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Pawprint.Controllers
{
    public class PetsController : Controller
    {
        // GET: Pets
        public ActionResult Profile(string name)
        {
            PawprintEntities DB = new PawprintEntities();
            Pet PetProfile = DB.Pets.SingleOrDefault(x => x.Name == name);

            if (PetProfile == null)
            {
                return RedirectToAction("Index", "Home");
            }

            return View(PetProfile);
        }
    }
}