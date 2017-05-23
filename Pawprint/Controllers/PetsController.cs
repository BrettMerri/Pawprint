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

        [HttpPost]
        public ActionResult SaveNewPost(Post NewPost, HttpPostedFileBase uploadFile)
        {
            NewPost.Date = DateTime.Now;

            if (uploadFile != null && uploadFile.ContentLength > 0)
            {
                string FileName = uploadFile.FileName;
                string PostID = Guid.NewGuid().ToString().Replace("-", "");
                string PetID = NewPost.PetID.ToString();

                string path = Path.Combine(Server.MapPath("~/App_Data/"), FileName);

                Directory.CreateDirectory(path);

                NewPost.FilePath = path;

                uploadFile.SaveAs(path);

                PawprintEntities PE = new PawprintEntities();
                PE.Posts.Add(NewPost);
                PE.SaveChanges();
            }
            return RedirectToAction("Index", "Home");
        }
    }
}