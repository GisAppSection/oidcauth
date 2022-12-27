using System;

namespace OidcAuth.Utilities
{
    public class AppUserData : IAppUserData
    {
        public AppUserData()
        {
            UserDataSessionId = Guid.NewGuid().ToString();
        }
        //public bool IsApptRescheduleOperation { get; set; }
        //public bool IsVcCounterDirectCall { get; set; }
        //public bool IsAgencyServiceOfficeLinkCall { get; set; }
        //public string RescheduleApptId { get; set; }
        public string UserDataSessionId { get; set; }
        public string RedirectCallUrl { get; set; }
        public string GoogleIDMState { get; set; }

        public string AngelenoIDMState { get; set; }

        public void ResetData()
        {
            //IsVcCounterDirectCall = false;
            //IsAgencyServiceOfficeLinkCall = false;
            //IsApptRescheduleOperation = false;
            //RescheduleApptId = string.Empty;
            RedirectCallUrl = string.Empty;
        }

    }
}
