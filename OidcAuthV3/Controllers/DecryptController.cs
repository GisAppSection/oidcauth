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
            return Content(Tools.DecryptString("0zFqDjDTymjb8XrZou5WE84VSXO84z83ziOdGjflStukJsIaTB036mllyj3Bg3Ixit9kj+DU7GAdu1MkJvjMn9yLVx4TbdW2iwST/npN2I1OT0gGahDt8jD1ANBcHjkL-/+/NjM3OTE0MTY5MTU1Nzc5ODE2"));

        }
    }
}