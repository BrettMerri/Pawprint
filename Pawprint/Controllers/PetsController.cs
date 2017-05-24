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
        public ActionResult Profile(int PetID)
        {
            PawprintEntities DB = new PawprintEntities();
            Pet PetProfile = DB.Pets.SingleOrDefault(x => x.PetID == PetID);

            if (PetProfile == null)
            {
                return RedirectToAction("Index", "Home");
            }

            List<Post> PostList = DB.Posts.Where(x => x.PetID == PetProfile.PetID).ToList();

            PostList.Reverse();

            ViewBag.PostList = PostList;

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

            //Create unique identifier
            string UniqueID = Guid.NewGuid().ToString().Replace("-", "");

            //This file path gets saved to the database
            NewPost.FilePath = $"{NewPost.PetID}/{UniqueID}/{uploadFile.FileName}";

            //This file path will be used to save the file
            string FilePath = $"~/img/posts/{NewPost.PetID}/{UniqueID}";

            Upload(uploadFile, FilePath);


            PawprintEntities PE = new PawprintEntities();
                PE.Posts.Add(NewPost);
                PE.SaveChanges();

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public ActionResult Upload(HttpPostedFileBase file, string filePath)
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
                ViewBag.Message = "You have not specified a file.";
            }
            return View("Index");
        }
    }
}