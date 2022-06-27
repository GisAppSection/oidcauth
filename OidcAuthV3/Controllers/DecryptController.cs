using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OidcAuthV3.Models;
using OidcAuthV3.Utilities;

namespace OidcAuthV3.Controllers
{
    public class DecryptController : Controller
    {
        public IActionResult Index()
        {
            return Content(Tools.DecryptString("TI6z8WjG4QfVzq+YXKWg8TwMCzC11ANuFYkD2XBFq59e+l8khVHIaBAVstjgSfQPcmrMW+MRLPQU/ZnXOFJIQ0qGDv74WJcdr4Hquq2FXB8=-/+/NjM3OTA1NzU0NzY5Mjk5Nzk0"));

        }
    }
}