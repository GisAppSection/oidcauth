using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OidcAuth.Utilities;

namespace OidcAuth.Controllers
{
    public class DecryptController : Controller
    {
        public IActionResult Index()
        {
            return Content(Tools.DecryptString("qJ/qEFqbape8E41ds0jKTO/JUtjc2XSZRJpr7JXnWkGTXipbKS6LSH38mXrIxbaGQBPl9c+T3/SDC6xLMVofCJJl+gE6boKAmbV1ioIkvZg=-/+/NjM3OTg1ODY2MjUzNDQwNDIw"));

        }
    }
}