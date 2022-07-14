using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using OidcAuthV3.Models;
using OidcAuthV3.Utilities;
using OidcAuthV3.DataAccess;

namespace OidcAuthV3.Controllers
{
    public class ErrorController : Controller
    {

        private readonly IEmailService _emailService;
        private readonly IDataFunctions _dataFunctions;
        private readonly IConfiguration _configuration;
        //private readonly IDataFunctions _dataFunctions;
        // private readonly IUserDataService _userDataService;
        //IDataFunctions DataFunctions,   // do not inject this so that you can also get database errors handled.
        // , IUserDataService userDataService
        public ErrorController(IConfiguration configuration, IEmailService emailService, IDataFunctions dataFunctions)
        {
            //_dataFunctions = DataFunctions;
            _emailService = emailService;
            _dataFunctions = dataFunctions;
            _configuration = configuration;
            // _userDataService = userDataService;
        }


        // [Route("Error")]
        [AllowAnonymous]
        public async Task<IActionResult> ErrorAction()
        {
            //Exception ex = (Exception) TempData["ex"];
            //string currentUserFullName = _dataFunctions.GetCurrentUserFullNameM();

            var envName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            var exceptionDetails = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
            ViewBag.Path = exceptionDetails.Path;
            ViewBag.Message = exceptionDetails.Error.Message;
            ViewBag.StackTrace = exceptionDetails.Error.StackTrace;
            ViewBag.Source = exceptionDetails.Error.Source;
            ViewBag.InnerException = exceptionDetails.Error.InnerException;
            ViewBag.Data = exceptionDetails.Error.Data;
            ViewBag.TargetSite = exceptionDetails.Error.TargetSite;
            ViewBag.HelpLink = exceptionDetails.Error.HelpLink;
            //+currentUserFullName
            // Activate the email feature when in production
            string emailTo = "Essam.Amarragy@Lacity.org";  // add all email address of users who should be aware of the error.  Comma separated.
                string emailCc = "";  // Essamce@gmail.com
                string emailBcc = "";
                string emailSubjectLine = "!!! OidcAuthV3 Error (" + envName + ") !!!";
                string emailMessage = "Path:" + exceptionDetails.Path + "<br /><hr />" + "TargetSite: " + exceptionDetails.Error.TargetSite + "<br /><hr />" + "Message:<pre>" + exceptionDetails.Error.Message + "</pre><br /><hr />" + "StackTrace:<pre>" + exceptionDetails.Error.StackTrace + "</pre><br /><hr />" + "Source:<pre>" + exceptionDetails.Error.Source + "</pre><br /><hr />" + "InnerException<pre>" + exceptionDetails.Error.InnerException + "</pre><br /><hr />" + "Data:<pre>" + exceptionDetails.Error.Data + "</pre>" + "HelpLink:<pre>" + exceptionDetails.Error.HelpLink + "</pre>";

            // Write error to database
            _dataFunctions.WriteException(emailSubjectLine, emailMessage);

                await _emailService.SendEmailAsync(emailTo, emailCc, emailBcc, emailSubjectLine, emailMessage);

                return View("Error");
        }

        public IActionResult ListExceptions()
        {
            var exceptionLogs = _dataFunctions.ListExceptionLogsM();
            return View(exceptionLogs);
        }

        public IActionResult DeleteExceptionLog(long logId)
        {
            bool success = _dataFunctions.DeleteExceptionLogM(logId);
            if (!success)
            {
                throw new Exception("An error was encountered.  Couldn't delete this exception log with id = " +logId);
            }
            return RedirectToAction("ListExceptions", "Error");
        }

        public IActionResult DeleteExceptionLog30()
        {
            bool success = _dataFunctions.DeleteExceptionLog30M();

            if (!success)
            {
                throw new Exception("An error was encountered.  Couldn't delete older exception logs.");
            }

            return RedirectToAction("ListExceptions", "Error");
        }


    }  
}