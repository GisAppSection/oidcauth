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
            //private readonly IWebHostEnvironment _env;
            //private readonly IConfiguration _configuration;
            //private readonly ILogger<HomeController> _logger;

            //public HomeController(IConfiguration configuration, IWebHostEnvironment env, ILogger<HomeController> logger)
            //{
            //    //_configuration = configuration;
            //    //_env = env;
            //    //string envName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            //    //_logger = logger;
            //}

            public IActionResult Index()
            {
                return View();
            }

            //public IActionResult Privacy()
            //{
            //    return View();
            //}

            //[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
            //public IActionResult Error()
            //{
            //    return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            //}

        }
    }
