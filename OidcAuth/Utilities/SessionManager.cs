using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OidcAuth.Utilities
{
    public static class SessionExtensions
    {
        public static void SetJson(this ISession session, string key, object value)
        {
            session.SetString(key, JsonConvert.SerializeObject(value));
        }
        public static T GetJson<T>(this ISession session, string key)
        {
            var sessionData = session.GetString(key);
            return sessionData == null
            ? default(T) : JsonConvert.DeserializeObject<T>(sessionData);
        }
    }
}

// Usage:
// to add an object such as the Cart to the session use this,
// HttpContext.Session.SetJson("Cart", cart);

// to retrieve the Cart object again, use this,
// Cart cart = HttpContext.Session.GetJson<Cart>("Cart");
