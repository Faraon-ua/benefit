using System;
using System.Web;

namespace Benefit.Services
{
    public class CookiesService
    {
        private const int CookiesDaysLifeTime = 365;
        private static CookiesService _instance = null;

        public static CookiesService Instance
        {
            get { return _instance ?? (_instance = new CookiesService()); }
        }

        public T GetCookieValue<T>(string name)
        {
            var cookie = HttpContext.Current.Request.Cookies[name];
            var cookieValue = cookie == null ? null : cookie.Value;
            if (cookieValue == null) return default(T);
            return (T)Convert.ChangeType(cookieValue, typeof(T));
        }

        public void AddCookie(string name, string value)
        {
            var referalCookie = new HttpCookie(name, value);
            referalCookie.Expires = DateTime.UtcNow.AddDays(CookiesDaysLifeTime);
            HttpContext.Current.Response.Cookies.Add(referalCookie);
        }

        public void RemoveCookie(string name)
        {
            var cookie = HttpContext.Current.Request.Cookies[name];
            if (cookie != null)
            {
                cookie.Secure = true;
                cookie.Expires = DateTime.UtcNow.AddDays(-1);
                HttpContext.Current.Response.SetCookie(cookie);
            }
        }
    }
}
