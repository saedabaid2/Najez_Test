using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Almanea.BusinessLogic
{

    public class cls_Defaults
    {
        public static bool IsEnglish = true;

        public static int Filesize = 20;

        public static string DomainUrl = "https://najez.app/";

        public static string UploadPath = AppDomain.CurrentDomain.BaseDirectory + "/Images/";

        public static string InvoiceUploadPath = "C:\\inetpub\\wwwroot\\Najez-Backend-Nodejs\\Najez-Backend-ssiva\\invoices\\";

        public static string ImageUrl = "/Images/";

        public static string ImageUrl2 = "~/Images/";

        public static string NoImage = ImageUrl + "NoImage.jpg";

        public static string CompanyLogo = "Company/";

        public static string ProfilePic = "UserPic/";

        public static string FinalOrder = "FinalOrder/";

        public static string Session_IsLabourDriver = "S_LabourDriver";

        public static string Session_UserId = "S_UserId";

        public static string Session_UserGroupTypeId = "S_UserGroupTypeId";

        public static string Session_UserGroupId = "S_UserGroupId";

        public static string Session_AccountTypeId = "S_AccountTypeId";

        public static string Session_ProfilePic = "S_ProfilePic";

        public static string Session_HasInventory = "S_HasInventory";

        public static string Session_CompanyLogo = "S_CompanyLogo";

        public static string Session_CompanyNameEN = "S_CompanyNameEN";

        public static string Session_CompanyNameAR = "S_CompanyNameAR";

        public static string Session_UserName = "S_UserName";

        public static string Session_IsInternal = "S_Isinternal";

        public static string Session_LaboursNotified = "S_LaboursNotified";

        public static int LaboursNotified = 5;

        public static CultureInfo culture = CultureInfo.CurrentCulture;

        public static CultureInfo DateTimeCulture = new CultureInfo("en-US");

        public static string GenerateUniqueId()
        {
            return DateTime.Now.Hour.ToString() + DateTime.Now.Minute + DateTime.Now.Second + DateTime.Now.Millisecond;
        }

        public static string FindImage(string FileName = null, string uploadFolder = "")
        {
            if (string.IsNullOrEmpty(FileName))
            {
                return NoImage;
            }
            string strImageUrl = UploadPath + uploadFolder + FileName;
            if (File.Exists(strImageUrl))
            {
                return ImageUrl + uploadFolder + FileName;
            }
            return ImageUrl + "NoImage.jpg";
        }

        public static string FindImage(string FileName, string uploadFolder, string Default)
        {
            if (string.IsNullOrEmpty(FileName))
            {
                return NoImage;
            }
            string strImageUrl = UploadPath + uploadFolder + FileName;
            if (File.Exists(strImageUrl))
            {
                return ImageUrl + uploadFolder + FileName;
            }
            return Default;
        }

        public static string StringReplace(string strReplace, Dictionary<string, string> dct)
        {
            return dct.Aggregate(strReplace, (string result, KeyValuePair<string, string> s) => result.Replace(s.Key, s.Value));
        }

        public static string GenerateCode(int length)
        {
            string strGuid = string.Empty;
            string strActivationCodeForMobile = string.Empty;
            try
            {
                StringBuilder builder = new StringBuilder();
                Random Random = new Random();
                string numbers = "1234567890";
                for (int i = 0; i < length; i++)
                {
                    string singleNumberValue = numbers[Random.Next(0, numbers.Length)].ToString();
                    builder.Append(singleNumberValue);
                }
                strActivationCodeForMobile = builder.ToString();
                return strActivationCodeForMobile;
            }
            catch (Exception)
            {
                return strActivationCodeForMobile;
            }
        }

        public static async Task<string> ShrinkURL(string url)
        {
            string finalUrl = "http://tinyurl.com/api-create.php?url=" + url;
            using
                (HttpClient client = new HttpClient())
            {
                return await client.GetStringAsync(finalUrl);
            }
        }

        public static string get12hour(int StartHour, int EndHour)
        {
            string start = "";
            string end = "";
            if (StartHour >= 12)
            {
                if (StartHour != 12)
                {
                    StartHour -= 12;
                }
                start = StartHour + " pm";
            }
            else
            {
                start = StartHour + " am";
            }
            if (EndHour >= 12)
            {
                if (EndHour != 12)
                {
                    EndHour -= 12;
                }
                end = EndHour + " pm";
            }
            else
            {
                end = EndHour + " am";
            }
            return start + "-" + end;
        }

        public static string GetBaseUrl()
        {
            HttpRequest request = HttpContext.Current.Request;
            string appUrl = HttpRuntime.AppDomainAppVirtualPath;
            if (appUrl != "/")
            {
                appUrl = "/" + appUrl;
            }
            return $"{request.Url.Scheme}://{request.Url.Authority}{appUrl}";
        }

        public static string ShrinkURLz(string strURL)
        {
            string URL = "http://tinyurl.com/api-create.php?url=" + strURL;
            HttpWebRequest objWebRequest = (HttpWebRequest)WebRequest.Create(URL);
            objWebRequest.Method = "GET";
            HttpWebResponse objWebResponse = (HttpWebResponse)objWebRequest.GetResponse();
            StreamReader srReader = new StreamReader(objWebResponse.GetResponseStream());
            string strHTML = srReader.ReadToEnd();
            srReader.Close();
            objWebResponse.Close();
            objWebRequest.Abort();
            return strHTML;
        }
    }
}