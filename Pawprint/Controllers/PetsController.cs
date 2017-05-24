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

            if (Request.IsAuthenticated)
            {
                if (User.Identity.GetUserId() == PetProfile.OwnerID)
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

            List<Post> PostList = DB.Posts.Where(x => x.PetID == PetProfile.PetID)
                                          .OrderByDescending(x => x.Date)
                                          .ToList();

            ViewBag.PostList = PostList;

            return View(PetProfile);
        }



        // Add a New Post
        public ActionResult AddNewPost(int PetID)
        {
            PawprintEntities DB = new PawprintEntities();
            Pet AddPet = DB.Pets.SingleOrDefault(x => x.PetID == PetID);

            // When User ID and Pet Owner ID Do Not Match
            if (User.Identity.GetUserId() != AddPet.OwnerID)
            {
                ViewBag.Message = "Can Not Post Under a Pet That is Not Your Own";
                return View("Error");
            }

            ViewBag.PetID = PetID;
            return View();
        }



        // Save New Post
        [HttpPost]
        public ActionResult SaveNewPost(Post NewPost, HttpPostedFileBase uploadFile)
        {
            PawprintEntities DB = new PawprintEntities();
            Pet AddPet = DB.Pets.SingleOrDefault(x => x.PetID == NewPost.PetID);

            // When User ID and Pet Owner ID Do Not Match
            if (User.Identity.GetUserId() != AddPet.OwnerID)
            {
                ViewBag.Message = "Can Not Post Under a Pet That is Not Your Own";
                return View("Error");
            }


            // Time Stamp for Post
            NewPost.Date = DateTime.Now;


            //Create Unique Identifier
            string UniqueID = Guid.NewGuid().ToString().Replace("-", "");


            //This File Path Gets Saved To The Database
            NewPost.FilePath = $"{NewPost.PetID}/{UniqueID}/{uploadFile.FileName}";


            //This File Path Will Be Used To Save The File
            string FilePath = $"~/img/posts/{NewPost.PetID}/{UniqueID}";

            Upload(uploadFile, FilePath);

                DB.Posts.Add(NewPost);
                DB.SaveChanges();

            return RedirectToAction("Index", "Home");
        }



        // Upload Photo
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
                ViewBag.Message = "You Have Not Specified a File.";
            }
            return View("Index");
        }
    }
}