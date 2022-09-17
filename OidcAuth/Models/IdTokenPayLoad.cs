namespace OidcAuth.Models
{
    public class IdTokenPayLoad
    {
            public string iss { get; set; }
            public string azp { get; set; }
            public string aud { get; set; }
            public string sub { get; set; }
            public string hd { get; set; }
            public string email { get; set; }
            public bool email_verified { get; set; }
            public string at_hash { get; set; }
            public string name { get; set; }
            public string picture { get; set; }
            public string given_name { get; set; }
            public string family_name { get; set; }
            public string locale { get; set; }
            public int iat { get; set; }
            public int exp { get; set; }
    }
}
