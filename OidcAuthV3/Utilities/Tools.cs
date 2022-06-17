using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using System.Xml.Serialization;

namespace OidcAuthV3.Utilities
{
    public static class Tools
    {
        public static string WkHtmlToPdfExe { get; set; }
        public static string ReportDir { get; set; }
        public static string PdfUrlLocation { get; set; }
        public static string EncryptionKey { get; set; }

        // Encryption function to use to encrypt querystring values
        // https://www.aspsnippets.com/Articles/Encrypt-and-Decrypt-QueryString-Parameter-Values-in-ASPNet-using-C-and-VBNet.aspx
        // Usage:
        // string EncItemId = HttpUtility.UrlEncode(Encrypt(ItemId.ToString().Trim()));
        //  @{ string EncItemId = HttpUtility.UrlEncode(Tools.Encrypt(@i.ItemId.ToString())); }

        private static Random random = new Random();

        public static string RandomString(int length)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        // encrypt then encode
        public static string eencrypt(string queryString)

        {
            if (string.IsNullOrWhiteSpace(queryString))
            {
                return null;
            }
            else
            {
                try
                {
                    string eeString = HttpUtility.UrlEncode(Tools.Encrypt(queryString));
                    return eeString;
                }
                catch
                {
                    return null;
                }
            }
        }

        // decode then decrypt
        public static string ddecrypt(string eencString)
        {
            if (string.IsNullOrWhiteSpace(eencString))
            {
                return null;
            }
            else
            {
                try
                {
                    string queryString = Tools.Decrypt(HttpUtility.UrlDecode(eencString));
                    return queryString;
                }
                catch
                {
                    return null;
                }
            }
        }

        public static string Encrypt(string clearText)
        {
            try
            {
                byte[] clearBytes = Encoding.Unicode.GetBytes(clearText);
                using (Aes encryptor = Aes.Create())
                {
                    Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                    encryptor.Key = pdb.GetBytes(32);
                    encryptor.IV = pdb.GetBytes(16);
                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                        {
                            cs.Write(clearBytes, 0, clearBytes.Length);
                            cs.Close();
                        }
                        clearText = Convert.ToBase64String(ms.ToArray());
                    }
                }
                return clearText;
            }
            catch
            {
                return null;
            }
        }

        // decrypt querystring values
        // https://www.aspsnippets.com/Articles/Encrypt-and-Decrypt-QueryString-Parameter-Values-in-ASPNet-using-C-and-VBNet.aspx
        // usage:
        // int itemId = Convert.ToInt32(Tools.Decrypt(HttpUtility.UrlDecode(encItemId)));
        public static string Decrypt(string cipherText)
        {
            try
            {
                if (string.IsNullOrEmpty(cipherText))
                {
                    throw new Exception("|An Error was encountered!");
                }
                cipherText = cipherText.Replace(" ", "+");
                byte[] cipherBytes = Convert.FromBase64String(cipherText);
                using (Aes encryptor = Aes.Create())
                {
                    Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                    encryptor.Key = pdb.GetBytes(32);
                    encryptor.IV = pdb.GetBytes(16);
                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                        {
                            cs.Write(cipherBytes, 0, cipherBytes.Length);
                            cs.Close();
                        }
                        cipherText = Encoding.Unicode.GetString(ms.ToArray());
                    }
                }
                return cipherText;
            }
            catch
            {
                return null;
            }
        }

        public static string Left(string str, int nCount)
        {
            str = str.Substring(0, nCount);
            return str;
        }

        public static string Right(string str, int nCount)
        {
            str = str.Substring(str.Length - nCount, nCount);
            return str;
        }

        public static string PrepFileName(string str)
        {
            var newFileName = str;

            var g = Guid.NewGuid();

            if (Right(str, 4).ToLower() == ".pdf")
            {
                newFileName = Left(str, str.Length - 4) + "_" + g + ".pdf";
                return newFileName;
            }

            if (Right(str, 4).ToLower() == ".jpg")
            {
                newFileName = Left(str, str.Length - 4) + "_" + g + ".jpg";
                return newFileName;
            }

            if (Right(str, 5).ToLower() == ".jpeg")
            {
                newFileName = Left(str, str.Length - 5) + "_" + g + ".jpeg";
                return newFileName;
            }

            if (Right(str, 4).ToLower() == ".png")
            {
                newFileName = Left(str, str.Length - 4) + "_" + g + ".png";
                return newFileName;
            }

            if (Right(str, 4).ToLower() == ".tif")
            {
                newFileName = Left(str, str.Length - 4) + "_" + g + ".tif";
                return newFileName;
            }

            if (Right(str, 5).ToLower() == ".tiff")
            {
                newFileName = Left(str, str.Length - 5) + "_" + g + ".tiff";
                return newFileName;
            }

            return newFileName;
        }

        #region Password Tools

        private const int SaltByteSize = 24;
        private const int HashByteSize = 24;
        private const int PBKDF2Iterations = 1000;

        private const int IterationIndex = 0;
        private const int SaltIndex = 1;
        private const int PBKDF2Index = 2;

        // static method within the Tools static class
        public static string GeneratePasswordHash(string password)
        {
            byte[] salt = new byte[SaltByteSize];

            // Generate a random salt
            using (RNGCryptoServiceProvider csprng = new RNGCryptoServiceProvider())
            {
                csprng.GetBytes(salt);
            }

            // Hash the password and encode the parameters
            byte[] hash = GetHashInByteArray(password, salt, PBKDF2Iterations, HashByteSize);
            return PBKDF2Iterations + ":" + Convert.ToBase64String(salt) + ":" + Convert.ToBase64String(hash);
        }

        // static method within the Tools static class
        private static byte[] GetHashInByteArray(string password, byte[] salt, int iterations, int outputBytes)
        {
            using (Rfc2898DeriveBytes pbkdf2 = new Rfc2898DeriveBytes(password, salt))
            {
                pbkdf2.IterationCount = iterations;
                return pbkdf2.GetBytes(outputBytes);
            }
        }

        // static method within the Tools static class
        private static bool SlowEquals(IList<byte> a, IList<byte> b)
        {
            var diff = (uint)a.Count ^ (uint)b.Count;
            for (var i = 0; (i < a.Count) && (i < b.Count); i++)
            {
                diff |= (uint)(a[i] ^ b[i]);
            }
            return diff == 0;
        }

        //internal static LoginInfo GetLoginInfo(TokenInfo tokenInfo)
        //{
        //    ClaimInfo claimInfo = tokenInfo.ClaimInfos.Where(x => x.Name == "http://schemas.microsoft.com/ws/2008/06/identity/claims/userdata").Select(x => x).FirstOrDefault();
        //    LoginInfo loginInfo = JsonConvert.DeserializeObject<LoginInfo>(claimInfo.Value);
        //    return loginInfo;
        //}

        // static method within the Tools static class

        // static method within the Tools static class
        public static bool ValidatePassword(string password, string correctHash)
        {
            // Extract the parameters from the hash
            var delimiter = new[] { ':' };
            var split = correctHash.Split(delimiter);
            var iterations = int.Parse(split[IterationIndex]);
            var salt = Convert.FromBase64String(split[SaltIndex]);
            var hash = Convert.FromBase64String(split[PBKDF2Index]);
            var testHash = GetHashInByteArray(password, salt, iterations, hash.Length);
            return SlowEquals(hash, testHash);
        }

        // static method within the Tools static class
        public static string EncryptString(string stringToEncrypt)
        {
            var bytesToBeEncrypted = Encoding.UTF8.GetBytes(stringToEncrypt);
            string passwordInfo = DateTime.Now.Ticks.ToString();
            var passwordBytes = Encoding.UTF8.GetBytes(passwordInfo);

            // Hash the password with SHA256
            passwordBytes = SHA256.Create().ComputeHash(passwordBytes);

            var bytesEncrypted = Encrypt(bytesToBeEncrypted, passwordBytes);

            var string1 = Convert.ToBase64String(bytesEncrypted);
            var string2 = Convert.ToBase64String(Encoding.UTF8.GetBytes(passwordInfo));
            return string1 + "-/+/" + string2;
        }

        // static method within the Tools static class
        public static string DecryptString(string stringToDecrypt)
        {
            string sep = "-/+/";
            int indexOfSep = stringToDecrypt.IndexOf(sep);
            string string1 = stringToDecrypt.Substring(0, indexOfSep);
            string string2 = stringToDecrypt.Substring(indexOfSep + sep.Length);
            var bytesToBeDecrypted = Convert.FromBase64String(string1);
            string passwordInfo = Encoding.UTF8.GetString(Convert.FromBase64String(string2));
            var passwordBytes = Encoding.UTF8.GetBytes(passwordInfo);
            passwordBytes = SHA256.Create().ComputeHash(passwordBytes);
            var bytesDecrypted = Decrypt(bytesToBeDecrypted, passwordBytes);
            return Encoding.UTF8.GetString(bytesDecrypted);
        }

        // static method within the Tools static class
        private static byte[] Encrypt(byte[] bytesToBeEncrypted, byte[] passwordBytes)
        {
            byte[] encryptedBytes = null;

            // Set your salt here, change it to meet your flavor:
            // The salt bytes must be at least 8 bytes.
            var saltBytes = new byte[] { 9, 8, 7, 6, 5, 4, 3, 2, 1, 0 };

            using (MemoryStream ms = new MemoryStream())
            {
                using (RijndaelManaged AES = new RijndaelManaged())
                {
                    var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 1000);

                    AES.KeySize = 256;
                    AES.BlockSize = 128;
                    AES.Key = key.GetBytes(AES.KeySize / 8);
                    AES.IV = key.GetBytes(AES.BlockSize / 8);

                    AES.Mode = CipherMode.CBC;

                    using (var cs = new CryptoStream(ms, AES.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(bytesToBeEncrypted, 0, bytesToBeEncrypted.Length);
                        cs.Close();
                    }

                    encryptedBytes = ms.ToArray();
                }
            }

            return encryptedBytes;
        }

        // static method within the Tools static class
        private static byte[] Decrypt(byte[] bytesToBeDecrypted, byte[] passwordBytes)
        {
            byte[] decryptedBytes = null;

            // Set your salt here, change it to meet your flavor:
            // The salt bytes must be at least 8 bytes.
            var saltBytes = new byte[] { 9, 8, 7, 6, 5, 4, 3, 2, 1, 0 };

            using (MemoryStream ms = new MemoryStream())
            {
                using (RijndaelManaged AES = new RijndaelManaged())
                {
                    var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 1000);

                    AES.KeySize = 256;
                    AES.BlockSize = 128;
                    AES.Key = key.GetBytes(AES.KeySize / 8);
                    AES.IV = key.GetBytes(AES.BlockSize / 8);
                    AES.Mode = CipherMode.CBC;

                    using (var cs = new CryptoStream(ms, AES.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(bytesToBeDecrypted, 0, bytesToBeDecrypted.Length);
                        cs.Close();
                    }

                    decryptedBytes = ms.ToArray();
                }
            }

            return decryptedBytes;
        }

        #endregion Password Tools

        // static method within the Tools static class
        //internal static string GetCallingView(HttpRequest request)
        //{
        //    string togoViewName = ((Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http.HttpRequestHeaders)request.Headers).HeaderReferer.ToString();
        //    return togoViewName;
        //}

        // static method within the Tools static class
        internal static string GetActionName(string callingViewName)
        {
            int lastIndexOfSlash = callingViewName.LastIndexOf("/");
            string actionName = callingViewName.Substring(lastIndexOfSlash + 1);
            return actionName;
        }

        // static method within the Tools static class
        //internal static User GetUserInfo(TokenInfo tokenInfo)
        //{
        //    ClaimInfo claimInfo = tokenInfo.ClaimInfos.Where(x => x.Name == "http://schemas.microsoft.com/ws/2008/06/identity/claims/userdata").Select(x => x).FirstOrDefault();
        //    User user = JsonConvert.DeserializeObject<User>(claimInfo.Value);
        //    return user;
        //}

        // static method within the Tools static class
        internal static string GetControllerName(string callingViewName)
        {
            //string actionName = GetActionName(callingViewName);
            //int secondLastIndexOfSlash = callingViewName.LastIndexOf("/", actionName.Length + 1);
            //string controllerName = callingViewName.Substring(secondLastIndexOfSlash + 1, callingViewName.Length - (actionName.Length + 1) - (secondLastIndexOfSlash + 1));
            //return controllerName;
            string[] callingViewArr = callingViewName.Split(new char[] { '/' });
            string controllerName = callingViewArr[callingViewArr.Length - 2];
            return controllerName;
        }

        // static method within the Tools static class
        public static IList<SelectListItem> GetSelectListItems(Dictionary<string, string> inDictionary)
        {
            List<SelectListItem> itemsToGo = new List<SelectListItem>();
            foreach (KeyValuePair<string, string> eachKVPair in inDictionary)
            {
                itemsToGo.Add(new SelectListItem(eachKVPair.Value, eachKVPair.Key));
            }
            return itemsToGo;
        }

        // static method within the Tools static class
        //public static string GetAppRolesDisplayString(List<AppRoles> appRoles)
        //{
        //    string allRolesToShow = string.Empty;
        //    foreach (AppRoles eachRole in appRoles)
        //    {
        //        if (string.IsNullOrEmpty(allRolesToShow) == false)
        //        {
        //            allRolesToShow = allRolesToShow + ", ";
        //        }
        //        allRolesToShow = allRolesToShow + eachRole.ToString();
        //    }
        //    return allRolesToShow;
        //}

        // static method within the Tools static class

        // static method within the Tools static class

        public static bool IsValidEmail(string email)
        {
            bool isEmail = Regex.IsMatch(email, @"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z", RegexOptions.IgnoreCase);

            return isEmail;
        }

        //public static bool IsValidEmail(string email)
        //{
        //    try
        //    {
        //        var addr = new System.Net.Mail.MailAddress(email);
        //        return addr.Address == email;
        //    }
        //    catch
        //    {
        //        return false;
        //    }
        //}

        public static void TransferValues(object postedSourceObject, object dbDestinationObject, IList<string> fieldsFromView)
        {
            //foreach (var newProp in newObj.GetType().GetProperties()) {
            //    var propname = newProp.Name;
            //    if (fieldNamesToIgnore.Contains(propname) == false) {
            //        var oldProp = oldObj.GetType().GetProperty(propname);
            //        var oldValue = oldProp.GetValue(oldObj);
            //        newProp.SetValue(newObj, oldValue);
            //    }
            //}
            try
            {
                foreach (string eachViewFieldName in fieldsFromView)
                {
                    if (eachViewFieldName == "__RequestVerificationToken") continue;
                    var newValue = GetFieldValue(postedSourceObject, eachViewFieldName);
                    SetFieldValue(eachViewFieldName, dbDestinationObject, newValue);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error in transfer values : " + ex.ToString());
            }
        }

        public static void SetFieldValue(string nestedFieldName, object target, object value)
        {
            string[] fieldNames = nestedFieldName.Split('.');
            for (int i = 0; i < fieldNames.Length - 1; i++)
            {
                PropertyInfo propertyToGet = target.GetType().GetProperty(fieldNames[i]);
                target = propertyToGet.GetValue(target, null);
            }
            PropertyInfo propertyToSet = target.GetType().GetProperty(fieldNames.Last());
            if (propertyToSet != null)
            {
                propertyToSet.SetValue(target, value, null);
            }
        }

        public static object GetFieldValue(object obj, string propertyName)
        {
            var propNames = propertyName.Split('.');

            for (var i = 0; i < propNames.Length; i++)
            {
                if (obj != null)
                {
                    var propInfo = obj.GetType().GetProperty(propNames[i]);
                    if (propInfo != null)
                        obj = propInfo.GetValue(obj);
                    else
                        obj = null;
                }
            }

            return obj;
        }

        public static Dictionary<string, object> GetDictFromJObject(JObject inJObject)
        {
            if (inJObject == null) return new Dictionary<string, object>();
            return ((JObject)inJObject).ToObject<Dictionary<string, object>>();
        }

        public static string GetCsv<T>(List<T> csvDataObjects)
        {
            PropertyInfo[] propertyInfos = typeof(T).GetProperties();
            var sb = new StringBuilder();
            sb.AppendLine(GetCsvHeadersRow(propertyInfos));
            csvDataObjects.ForEach(x => sb.AppendLine(GetCsvValuesRow(x, propertyInfos)));
            string finalCSVExportString = sb.ToString();
            return finalCSVExportString;
        }

        private static string GetCsvValuesRow<T>(T csvDataObject, PropertyInfo[] propertyInfos)
        {
            IEnumerable<string> allValuesCSVString = propertyInfos.Select(x => new
            {
                Value = x.GetValue(csvDataObject, null)
            }).Select(x => GetPropertyValueAsString(x.Value));
            string csvRowValuesString = String.Join(",", allValuesCSVString);
            return csvRowValuesString;
        }

        private static string GetPropertyValueAsString(object propertyValue)
        {
            string propertyValueString;

            if (propertyValue == null)
                propertyValueString = "";
            else if (propertyValue is DateTime)
                propertyValueString = ((DateTime)propertyValue).ToString("dd MMM yyyy");
            else if (propertyValue is int)
                propertyValueString = propertyValue.ToString();
            else if (propertyValue is float)
                propertyValueString = ((float)propertyValue).ToString("#.####"); // format as you need it
            else if (propertyValue is double)
                propertyValueString = ((double)propertyValue).ToString("#.####"); // format as you need it
            else // treat as a string
                propertyValueString = @"""" + propertyValue.ToString().Replace(@"""", @"""""") + @""""; // quotes with 2 quotes

            return propertyValueString;
        }

        private static string GetCsvHeadersRow(PropertyInfo[] propertyInfos)
        {
            IEnumerable<string> allHeaderNames = propertyInfos.Select(x => x.Name);
            string csvHeaderNamesString = string.Join(",", allHeaderNames);
            return csvHeaderNamesString;
        }

        public static double GetBusinessDays(DateTime startD, DateTime endD)
        {
            // Ref : https://stackoverflow.com/questions/1617049/calculate-the-number-of-business-days-between-two-dates
            double calcBusinessDays = 1 + ((endD - startD).TotalDays * 5 - (startD.DayOfWeek - endD.DayOfWeek) * 2) / 7;
            if (endD.DayOfWeek == DayOfWeek.Saturday) calcBusinessDays--;
            if (startD.DayOfWeek == DayOfWeek.Sunday) calcBusinessDays--;

            // For same date, force the business day to zero
            if (endD.Year == startD.Year && endD.Month == startD.Month && endD.Day == startD.Day)
            {
                calcBusinessDays = 0;
            }

            calcBusinessDays = Math.Round(calcBusinessDays, 0);
            return calcBusinessDays;
        }

        // the first day is always not counted.
        public static double GetBusinessDaysBoe(DateTime startD, DateTime endD)
        {
            // strip the time part from the datetime
            startD = startD.Date.Date;
            endD = endD.Date.Date;

            double calcBusinessDays =
               ((endD - startD).TotalDays * 5 -
                (startD.DayOfWeek - endD.DayOfWeek) * 2) / 7;

            if (endD.DayOfWeek == DayOfWeek.Saturday) calcBusinessDays--;
            if (startD.DayOfWeek == DayOfWeek.Sunday) calcBusinessDays--;

            calcBusinessDays = Math.Floor(calcBusinessDays);
            return calcBusinessDays;
        }

        public static IList<string> ConvertStringsToEmailIds(string emailIDsWithComma)
        {
            IList<string> validEmails = new List<string>();
            if (string.IsNullOrEmpty(emailIDsWithComma) || string.IsNullOrWhiteSpace(emailIDsWithComma))
            {
                return validEmails;
            }
            IList<string> allEmailString = emailIDsWithComma.Split(new char[] { ',' }).ToList();
            foreach (string eachEmailString in allEmailString)
            {
                if (IsValidEmail(eachEmailString.Trim()))
                {
                    validEmails.Add(eachEmailString.Trim());
                }
                else
                {
                    throw new Exception("Invalid Email : " + eachEmailString);
                }
            }
            return validEmails;
        }

        public static string GetEnumDescription<T>(T e) where T : IConvertible
        {
            if (e is Enum)
            {
                Type type = e.GetType();
                Array values = Enum.GetValues(type);

                foreach (int val in values)
                {
                    if (val == e.ToInt32(CultureInfo.InvariantCulture))
                    {
                        var memInfo = type.GetMember(type.GetEnumName(val));
                        var descriptionAttribute = memInfo[0]
                            .GetCustomAttributes(typeof(DescriptionAttribute), false)
                            .FirstOrDefault() as DescriptionAttribute;

                        if (descriptionAttribute != null)
                        {
                            return descriptionAttribute.Description;
                        }
                    }
                }
            }

            return e.ToString(); // could also return string.Empty
        }

        public static IList<SelectListItem> GetSelectionListItemsFromEnum(Type type)
        {
            Array allTypes = Enum.GetValues(type);
            IList<string> allTypesString = new List<string>();
            foreach (var eachType in allTypes)
            {
                allTypesString.Add(eachType.ToString());
            }
            Dictionary<string, string> dropDownListInfo = allTypesString.ToDictionary(mc => mc.ToString(), mc => mc.ToString());
            return GetSelectListItems(dropDownListInfo);
        }

        public static IList<int> GetIntegersListFromCommaSepString(string csString)
        {
            string[] catListArr = csString.Split(new char[] { ',' });
            IList<int> catList = new List<int>();
            foreach (string eachString in catListArr)
            {
                catList.Add(Convert.ToInt32(eachString));
            }
            return catList;
        }

        public static FileInfo GetUniqueFileName(string path)
        {
            string dir = Path.GetDirectoryName(path);
            string fileName = Path.GetFileNameWithoutExtension(path);
            string fileExt = Path.GetExtension(path);

            for (int i = 1; ; ++i)
            {
                if (!System.IO.File.Exists(path))
                    return new FileInfo(path);

                path = Path.Combine(dir, fileName + "_(" + i + ")" + fileExt);
            }
        }

        // convert this to a static method that return an image
        public static string GenerateBarCode(string barcode)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (Bitmap bitMap = new Bitmap(barcode.Length * 20, 85))  // width , height of barcode image // was 40,80
                {
                    using (Graphics graphics = Graphics.FromImage(bitMap))
                    {
                        Font oFont = new Font("IDAutomationHC39L", 12);  // familyName, emSize 16
                        PointF point = new PointF(2f, 2f);
                        SolidBrush whiteBrush = new SolidBrush(Color.White); // make White later
                        graphics.FillRectangle(whiteBrush, 0, 0, bitMap.Width, bitMap.Height);
                        SolidBrush blackBrush = new SolidBrush(Color.Black);  // make black later
                        graphics.DrawString("*" + barcode + "*", oFont, blackBrush, point);
                    }

                    bitMap.Save(memoryStream, ImageFormat.Jpeg);

                    // could this be a jpeg
                    return "data:image/png;base64," + Convert.ToBase64String(memoryStream.ToArray());
                    //return "data:image/png;base64," + Convert.ToBase64String(b);
                    // return BarcodeImage;
                }
            }
        }

        // static method to render view to html
        //public static class ControllerExtensions
        //{
        public static async Task<string> RenderViewAsync<TModel>(Controller controller, string viewName, TModel model, bool partial = false)
        {
            if (string.IsNullOrEmpty(viewName))
            {
                viewName = controller.ControllerContext.ActionDescriptor.ActionName;
            }

            controller.ViewData.Model = model;

            using (var writer = new StringWriter())
            {
                IViewEngine viewEngine = controller.HttpContext.RequestServices.GetService(typeof(ICompositeViewEngine)) as ICompositeViewEngine;
                ViewEngineResult viewResult = viewEngine.FindView(controller.ControllerContext, viewName, !partial);

                if (viewResult.Success == false)
                {
                    return $"A view with the name {viewName} could not be found";
                }

                ViewContext viewContext = new ViewContext(
                    controller.ControllerContext,
                    viewResult.View,
                    controller.ViewData,
                    controller.TempData,
                    writer,
                    new HtmlHelperOptions()
                );

                await viewResult.View.RenderAsync(viewContext);

                return writer.GetStringBuilder().ToString();
            }
        }

        //}

        // https://stackoverflow.com/questions/10218181/xmlserializer-serialize-stripping-the-xml-tag
        public static string XmlSerialize<T>(T obj)
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.OmitXmlDeclaration = true;
            var writer = new StringWriter();
            XmlWriter xmlWriter = XmlWriter.Create(writer, settings);

            XmlSerializerNamespaces names = new XmlSerializerNamespaces();
            names.Add("", "");

            XmlSerializer serializer = new XmlSerializer(typeof(T));

            serializer.Serialize(xmlWriter, obj, names);
            var xml = writer.ToString();
            return xml;
        }

        // https://stackoverflow.com/questions/2434534/serialize-an-object-to-string
        public static T XmlDeserialize<T>(this string toDeserialize)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
            using (StringReader textReader = new StringReader(toDeserialize))
            {
                return (T)xmlSerializer.Deserialize(textReader);
            }
        }

        public static IActionResult GeneratePdf(string htmlContent, string baseFileName, string pdfOptions, bool saveFile)
        {
            try
            {
                if (htmlContent == null) throw new Exception("HTML Content cannot be null.");
                if (string.IsNullOrEmpty(baseFileName)) throw new Exception("Base File Name cannot be null.");

                string inputHtmlBaseFileNameWithExtension = baseFileName + ".html";
                string outputPDFBaseFileNameWithExtension = baseFileName + ".pdf";

                //// convert to static fields
                //string WkHtmlToPdfExe = "C:\\Program Files\\wkhtmltopdf\\wkhtmltopdf.exe";
                //// Configuration["AppConfig:WkHtmlToPdfExe"];
                //string ReportDir = "D:\\Docs\\temppdfs\\ucs";
                //// Configuration["AppConfig:ReportDir"];
                //string PdfUrlLocation = @"http://Localhost:44325/docs/tempPdfs/ucs";
                //// Configuration["AppConfig:PdfUrlLocation"];

                // Setup the name and location of the executable file.
                FileInfo exeFileInfo = new FileInfo(WkHtmlToPdfExe);
                if (exeFileInfo.Exists == false) throw new Exception("File " + WkHtmlToPdfExe + " missing.");

                // Write the htmlContent to a file with an html extension
                string htmlFileName = ReportDir + "\\" + inputHtmlBaseFileNameWithExtension;
                StreamWriter sw = new StreamWriter(htmlFileName);
                sw.WriteLine(htmlContent);
                sw.Close();
                FileInfo htmlFileInfo = new FileInfo(htmlFileName);
                if (htmlFileInfo.Exists == false) throw new Exception("File " + htmlFileName + " missing.");

                string outputPDFFileName = ReportDir + "\\" + outputPDFBaseFileNameWithExtension;
                string outputPDFUrl = PdfUrlLocation + "/" + outputPDFBaseFileNameWithExtension;

                // setup the process parameters
                var process = new Process()
                {
                    StartInfo = new ProcessStartInfo

                    {
                        FileName = WkHtmlToPdfExe,

                        Arguments = pdfOptions + " " + htmlFileName + " " + outputPDFFileName,

                        RedirectStandardOutput = true,

                        UseShellExecute = false,

                        CreateNoWindow = true,
                    }
                };

                // run the process
                process.Start();
                string result = process.StandardOutput.ReadToEnd();
                process.WaitForExit();  // you should have a pdf file at this point

                // Read process.StandardError if error happens
                if (saveFile == false)
                {
                    byte[] byteArray = System.IO.File.ReadAllBytes(outputPDFFileName);
                    MemoryStream pdfStream = new MemoryStream();
                    pdfStream.Write(byteArray, 0, byteArray.Length);
                    pdfStream.Position = 0;
                    //FileStreamResult fsr = new FileStreamResult(pdfStream, "application/pdf");
                    //fsr.FileDownloadName = outputPDFBaseFileNameWithExtension;
                    //return fsr;
                    return new FileStreamResult(pdfStream, "application/pdf");
                }
                else
                {
                    //byte[] byteArray = System.IO.File.ReadAllBytes(outputPDFFileName);
                    //MemoryStream pdfStream = new MemoryStream();
                    //pdfStream.Write(byteArray, 0, byteArray.Length);
                    //pdfStream.Position = 0;
                    //FileStreamResult fsr = new FileStreamResult(pdfStream, "application/pdf");
                    //fsr.FileDownloadName = outputPDFBaseFileNameWithExtension;
                    return new JsonResult(outputPDFUrl);
                }
            }
            catch (Exception ex)
            {
                return new JsonResult("Error during PDF create operation. Details : " + ex.ToString());
            }
        }
    }  // End of the Tools Class

    public static class UrlExtensions
    {
        public static string PathAndQuery(this HttpRequest request) =>
        request.QueryString.HasValue
        ? $"{request.Path}{request.QueryString}"
        : request.Path.ToString();
    }

    //public static class Sessions
    //{
    //    private static IHttpContextAccessor _httpContextAccessor;

    //    public static void Configure(IHttpContextAccessor httpContextAccessor)
    //    {
    //        _httpContextAccessor = httpContextAccessor;
    //    }

    //    public static HttpContext Current => _httpContextAccessor.HttpContext;

    //    //Set Session Variable
    //    public static void SetSessionVar(string key, string value)
    //    {
    //        HttpContext.Session.SetString(key, value);
    //    }

    //    //Get Session Variable

    //    public static string GetSessionVar(string key)
    //    {
    //        return HttpContext.Session.GetString(key);
    //    }

    //}

    // https://www.pluralsight.com/guides/property-copying-between-two-objects-using-reflection
    // usage:  PropertyCopier<SourceClass, TargetClass>.Copy(sourceObject, targetObject);

    public static class PropertyCopier<TSource, TTarget>
        where TSource : class
        where TTarget : class
    {
        public static void Copy(TSource source, TTarget target)
        {
            var parentProperties = source.GetType().GetProperties();
            var childProperties = target.GetType().GetProperties();

            foreach (var parentProperty in parentProperties)
            {
                foreach (var childProperty in childProperties)
                {
                    if (parentProperty.Name == childProperty.Name && parentProperty.PropertyType == childProperty.PropertyType)
                    {
                        childProperty.SetValue(target, parentProperty.GetValue(source));
                        break;
                    }
                }
            }
        }
    }

    //public static class ControllerExtensions
    //{
    //    public static async Task<string> RenderViewAsync<TModel>(this Controller controller, string viewName, TModel model, bool partial = false)
    //    {
    //        if (string.IsNullOrEmpty(viewName))
    //        {
    //            viewName = controller.ControllerContext.ActionDescriptor.ActionName;
    //        }

    //        controller.ViewData.Model = model;

    //        using (var writer = new StringWriter())
    //        {
    //            IViewEngine viewEngine = controller.HttpContext.RequestServices.GetService(typeof(ICompositeViewEngine)) as ICompositeViewEngine;
    //            ViewEngineResult viewResult = viewEngine.FindView(controller.ControllerContext, viewName, !partial);

    //            if (viewResult.Success == false)
    //            {
    //                return $"A view with the name {viewName} could not be found";
    //            }

    //            ViewContext viewContext = new ViewContext(
    //                controller.ControllerContext,
    //                viewResult.View,
    //                controller.ViewData,
    //                controller.TempData,
    //                writer,
    //                new HtmlHelperOptions()
    //            );

    //            await viewResult.View.RenderAsync(viewContext);

    //            return writer.GetStringBuilder().ToString();
    //        }
    //    }
    //}
}