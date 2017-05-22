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
        public ActionResult Profile(int ID)
        {
            PawprintEntities DB = new PawprintEntities();
            Pet PetProfile = DB.Pets.SingleOrDefault(x => x.PetID == ID);

            if (PetProfile == null)
            {
                return RedirectToAction("Index", "Home");
            }

            return View(PetProfile);
        }
    }
}