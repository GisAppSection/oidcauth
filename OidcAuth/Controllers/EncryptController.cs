using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OidcAuth.Models;
using OidcAuth.Utilities;

namespace OidcAuth.Controllers
{
    public class EncryptController : Controller
    {
        public IActionResult Index()
        {
            return Content(Tools.EncryptString("Server=dsp3;Database=oidcauth;User Id=BoeDbWriter; Password=$$Boe1149w;"));
        }
    }
}