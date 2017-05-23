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
        public ActionResult Profile(int PetID)
        {
            PawprintEntities DB = new PawprintEntities();
            Pet PetProfile = DB.Pets.SingleOrDefault(x => x.PetID == PetID);

            if (PetProfile == null)
            {
                return RedirectToAction("Index", "Home");
            }

            return View(PetProfile);
        }

        public ActionResult AddNewPost()
        {
            return View();
        }

        public ActionResult SaveNewPost(Post NewPost)
        {
            NewPost.Date = DateTime.Now;
            NewPost.FilePath = "~/img/posts/example.jpg";

            PawprintEntities PE = new PawprintEntities();
            PE.Posts.Add(NewPost);
            PE.SaveChanges();

            return RedirectToAction("Index", "Home");
        }
    }
}