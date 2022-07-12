using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OidcAuthV3.Models;
using OidcAuthV3.Utilities;

namespace OidcAuthV3.Controllers
{
    public class EncryptController : Controller
    {
        public IActionResult Index()
        {
            return Content(Tools.EncryptString("Server=DSP2;Database=rpermitstemp;User Id=BoeDbWriter; Password=$$Boe1149w;"));
        }
    }
}