using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using OidcAuth.Models;
using OidcAuth.Utilities;
using OidcAuth.DataAccess;

namespace OidcAuth.Controllers
{

    public class ErrorController : Controller
    {

        private readonly IEmailService _emailService;
        private readonly IDataFunctions _dataFunctions;
        private readonly IConfiguration _configuration;

        public ErrorController(IConfiguration configuration, IEmailService emailService, IDataFunctions dataFunctions)
        {
            _dataFunctions = dataFunctions;
            _emailService = emailService;
            _configuration = configuration;
        }


        // [Route("Error")]
        [AllowAnonymous]
        public async Task<IActionResult> ErrorAction()
        {
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
            string emailTo = "Essam.Amarragy@Lacity.org";  
            // add all email address of users who should be aware of the error.  Comma separated.
                string emailCc = "";  // Essamce@gmail.com
                string emailBcc = "";
                string emailSubjectLine = "!!! OidcAuth Error (" + envName + ") !!!";
                string emailMessage = "Path:" + exceptionDetails.Path + "<hr />" + "Message:<pre>" + exceptionDetails.Error.Message + "</pre><hr />" + "TargetSite: " + exceptionDetails.Error.TargetSite + "<hr />" + "StackTrace:<pre>" + exceptionDetails.Error.StackTrace + "</pre><hr />" + "Source:<pre>" + exceptionDetails.Error.Source + "</pre><hr />" + "InnerException<pre>" + exceptionDetails.Error.InnerException + "</pre><hr />" + "Data:<pre>" + exceptionDetails.Error.Data + "</pre>" + "HelpLink:<pre>" + exceptionDetails.Error.HelpLink + "</pre><hr />";

            // Write error to database
            _dataFunctions.WriteException(emailSubjectLine, emailMessage);

                await _emailService.SendEmailAsync(emailTo, emailCc, emailBcc, emailSubjectLine, emailMessage);
                return View("ExceptionError");
        }

    }  
}