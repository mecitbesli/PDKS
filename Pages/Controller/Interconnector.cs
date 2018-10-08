using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.AspNetCore.Http;

namespace PDKS.Pages
{
    ///<summary>
    ///Enums of all the possible pages user can access
    ///</summary>
    public enum EPage
    {
        login, worked, excuse, admin, authTable, authorize, hoursChange, hoursTable, requestRespond, requestTable, excuseStates
    }
    public class Interconnector : PageModel
    {
        ///<summary>
        ///Changes the given httpContext user's password on db
        ///with parameters inside the httpContext.Request.Form
        ///</summary>
        public static string PasswordChange(PDKS.Context context, HttpContext httpContext)
        {
            string prev = httpContext.Request.Form["prevPwInput"];
            string new1 = httpContext.Request.Form["newPwInput"];
            string new2 = httpContext.Request.Form["newPwInput2"];
            if (new1 != new2)
            {
                return "respPWMatchErr";
            }

            User user = new User();
            try
            {
                user = (User)context.UserSet.Single(b => (b.Username == httpContext.User.FindFirst("UserName").Value));
            }
            catch (Exception e)
            {
                System.Console.WriteLine(e.Message);
                return "serverErr";
            }
            if (user.Password != Crypt.Encrypt(prev))
            {
                return "wrongPWErr";
            }
            if (new1.Length < 6)
            {
                return "shortPWErr";
            }
            user.Password = Crypt.Encrypt(new1);
            context.SaveChanges();
            return "pwSuccess";
        }
        ///<summary>
        ///Creates user cookie using the model object fields.
        /// AuthStr must be formatted like "101" each digits 
        ///meaning:yes for Customize, no for Requests, yes for Authority.
        /// lang can be "EN" for English and "TR" for Turkish
        ///</summary>
        public static async void CreateCookie(User user, string AuthStr, HttpContext httpContext, string lang = "EN")
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Id.ToString()),
                new Claim("FullName", user.Name),
                new Claim("UserName", user.Username),
                new Claim("Role", user.Role.ToString()),
                new Claim("AuthorisationString", AuthStr),
                new Claim("Language", lang),
            };
            var claimsIdentity = new ClaimsIdentity(
                claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new Microsoft.AspNetCore.Authentication.AuthenticationProperties { };
            authProperties.IsPersistent = true;
            authProperties.ExpiresUtc = DateTimeOffset.UtcNow.AddDays(30);
            await httpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);
        }
        ///<summary>
        ///Creates user cookie using the previous cookie data
        ///use this to change language.
        /// lang can be "EN" for English and "TR" for Turkish
        ///</summary>
        public static void CreateCookie(ClaimsPrincipal user, HttpContext httpContext, string lang = "EN")
        {
            string id = user.Identity.Name;
            string name = user.FindFirst("FullName").Value;
            string username = user.FindFirst("UserName").Value;
            string role = user.FindFirst("Role").Value;
            string authStr = user.FindFirst("AuthorisationString").Value;
            User userTmp = new User
            {
                Id = Int32.Parse(id),
                Name = name,
                Username = username,
                Role = Int32.Parse(role)
            };
            CreateCookie(userTmp, authStr, httpContext, lang);
        }
        ///<summary>
        ///Decides and returns a redirection path according authorization,
        ///current page, role of the user obtained by user cookie and passed
        ///to this
        ///</summary>
        public static string Authenticator(ClaimsPrincipal user, EPage currPage)
        {
            string authStr = "111";
            if (currPage != EPage.login) try
                {
                    authStr = user.FindFirst("AuthorisationString").Value;
                }
                catch (Exception) { }
            try
            {
                if (!user.Identity.IsAuthenticated)
                {
                    if (currPage != EPage.login) return "/login";
                    else return "";
                }
            }
            catch (Exception)
            {
                if (currPage != EPage.login) return "/login";
                else return "";
            }
            string role = user.FindFirst("Role").Value;
            if (role == "1")
            {
                if (currPage == EPage.login)
                    return "/admin";
                else if (currPage == EPage.authTable && authStr[2] == '0')
                    return "/admin";
                else if (currPage == EPage.authorize && authStr[2] == '0')
                    return "/admin";
                else if (currPage == EPage.hoursChange && authStr[0] == '0')
                    return "/admin";
                else if (currPage == EPage.requestRespond && authStr[1] == '0')
                    return "/admin";
                return "";
            }
            else if (role == "2")
            {
                if (currPage == EPage.login && currPage != EPage.worked && currPage != EPage.excuse)
                    return "/worked";
                return "";
            }
            else
            {
                if (currPage != EPage.login) return "/login";
                else return "";
            }
        }
    }
}
