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
            return Content(Tools.DecryptString("sj6GYShji9aL1SuPNYv0uc1cM6+dVUWDEkeJQz6fcCIMyRP6STnvk7FPmWDLHbR3uvuNxvyrkNa+mmsFTMVuh7as+sv8ZIy3v+KW9J2RMQw=-/+/NjM3MzU3MTMwODMwOTA1MzY5"));

        }
    }
}