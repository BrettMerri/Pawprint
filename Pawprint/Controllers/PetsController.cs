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
            NewPost.FilePath = "img/posts/" + uploadFile.FileName;
            Upload(uploadFile);


            PawprintEntities PE = new PawprintEntities();
                PE.Posts.Add(NewPost);
                PE.SaveChanges();

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public ActionResult Upload(HttpPostedFileBase file)
        {
            if (file != null && file.ContentLength > 0)
                try
                {
                    string path = Path.Combine(Server.MapPath("~/img/posts"),
                                               Path.GetFileName(file.FileName));
                    file.SaveAs(path);
                    ViewBag.Message = "File uploaded successfully";
                }
                catch (Exception ex)
                {
                    ViewBag.Message = "ERROR:" + ex.Message.ToString();
                }
            else
            {
                ViewBag.Message = "You have not specified a file.";
            }
            return View("Index");
        }
    }
}