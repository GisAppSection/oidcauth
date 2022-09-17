using System.Collections.Generic;

namespace OidcAuth.Models
{
    public class StaffData
    {
        public string kind { get; set; }
        public string id { get; set; }
        public string etag { get; set; }
        public string primaryEmail { get; set; }
        public Name name { get; set; }
        public List<Email> emails { get; set; }
        public List<Address> addresses { get; set; }
        public List<Organization> organizations { get; set; }
        public List<Phone> phones { get; set; }
        public List<Language> languages { get; set; }
        public string thumbnailPhotoUrl { get; set; }
        public string thumbnailPhotoEtag { get; set; }
        public CustomSchemas customSchemas { get; set; }
    }
    public class Address
    {
        public string type { get; set; }
        public bool primary { get; set; }
    }

    public class CustomSchemas
    {
        public LACityEmployeeID LACityEmployeeID { get; set; }
        public LACityCustomAttributes LACityCustomAttributes { get; set; }
        public SAML SAML { get; set; }
    }

    public class Email
    {
        public string address { get; set; }
        public bool primary { get; set; }
    }

    public class LACityCustomAttributes
    {
        public string LACityMobileNumber { get; set; }
        public string LACityWorkNumber { get; set; }
    }

    public class LACityEmployeeID
    {
        public string employeeId { get; set; }
    }

    public class Language
    {
        public string languageCode { get; set; }
        public string preference { get; set; }
    }

    public class Name
    {
        public string givenName { get; set; }
        public string familyName { get; set; }
        public string fullName { get; set; }
    }

    public class Organization
    {
        public string title { get; set; }
        public bool primary { get; set; }
        public string customType { get; set; }
        public string department { get; set; }
        public string location { get; set; }
    }

    public class Phone
    {
        public string value { get; set; }
        public string type { get; set; }
    }

    public class SAML
    {
        public bool securityGroupMember { get; set; }
        public List<SecurityGroupEmail> securityGroupEmails { get; set; }
        public List<SecurityGroup> securityGroups { get; set; }
    }

    public class SecurityGroup
    {
        public string type { get; set; }
        public string value { get; set; }
    }

    public class SecurityGroupEmail
    {
        public string type { get; set; }
        public string value { get; set; }
    }

}
