using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MvcMovie.Models;

namespace MvcMovie.Controllers
{
    public class PaintController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Draw()
        {
            return View();
        }

    }
}