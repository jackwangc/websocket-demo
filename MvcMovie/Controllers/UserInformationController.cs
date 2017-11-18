
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MvcMovie.Models;

namespace MvcMovie.Controllers
{
    public class UserInformationController : Controller
    {
        private MvcMovieContext db;
        public UserInformationController (MvcMovieContext context)
        {
            db = context;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult show()
        {
            return View(db.UserInformations.ToList());
        }
        public IActionResult create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create(UserInformation user)
        {
            if (ModelState.IsValid)
            {
                db.UserInformations.Add(user);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(user);
        }

    }
}