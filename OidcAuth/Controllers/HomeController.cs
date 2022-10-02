using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OidcAuth.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace OidcAuth.Controllers
{
    [AllowAnonymous]
    public class HomeController : Controller
    {
            public IActionResult Index()
            {
                return View();
            }
        }
    }
