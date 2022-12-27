namespace OidcAuth.Utilities
{
    public interface IAppUserData

    {
        //bool IsApptRescheduleOperation { get; set; }

        //bool IsVcCounterDirectCall { get; set; }

        //bool IsAgencyServiceOfficeLinkCall { get; set; }

        //string RescheduleApptId { get; set; }

        string UserDataSessionId { get; set; }

        string RedirectCallUrl { get; set; }

        string GoogleIDMState { get; set; }

        string AngelenoIDMState { get; set; }


        public void ResetData();

    }
}
