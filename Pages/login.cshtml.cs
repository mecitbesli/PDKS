using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json.Linq;

namespace PDKS.Pages
{
    public class LoginModel : PageModel
    {
        public JObject lang = new JObject();
        public string State { get; set; }
        public string Error { get; set; }
        private readonly PDKS.Context _context;
        ///<summary>
        ///Fetches the context to the current model
        ///</summary>
        public LoginModel(PDKS.Context context)
        {
            _context = context;
        }
        ///<summary>
        ///Default get handler: 
        ///Authenticates the user, gets the language resource
        ///and finally provides the page
        ///</summary>
        public void OnGet()
        {
            string redirectionPath = Interconnector.Authenticator(HttpContext.User, EPage.login);
            if (redirectionPath != "")
            {
                Response.Redirect(redirectionPath, false);
            }
            lang = LangResource.GetLanguageJson("login", "EN");


            try
            {
                if (HttpContext.User.FindFirst("Role").Value == "3")
                {
                    State = "Your account is inactive";
                }
            }
            catch (Exception) { }

        }
        ///<summary>
        ///Default post handler:
        ///Checks the validity of the request inputs, 
        ///creates cookie for the current user and
        ///redirects accordingly
        ///</summary>
        public void OnPost(string inputUsername, string inputPassword)
        {
            if (inputUsername is null || inputPassword is null)
            {
                State = "Please enter your info";
                return;
            }
            while (inputUsername.Length < 6)
            {
                inputUsername = "0" + inputUsername;
            }
            if (inputUsername.Length == 6)
                inputUsername = "P" + inputUsername;
            string AuthStr;
            User user;
            AdminAuthorization auth;
            try
            {
                string encPw = Crypt.Encrypt(inputPassword);
                user = (User)_context.UserSet.Single(b => (b.Username == inputUsername) && (b.Password == encPw));
            }
            catch (Exception)
            {
                State = "Login failed.";
                return;
            }
            try
            {
                auth = (AdminAuthorization)_context.AdminAuthorizationSet.Single(b => (b.UserId == user.Id));
            }
            catch (Exception)
            {
                State = "Login failed.";
                return;
            }
            State = "login success";
            AuthStr = (auth.Customize ? 1 : 0) + "" + (auth.Requests ? 1 : 0) + "" + (auth.Authority ? 1 : 0);
            Interconnector.CreateCookie(user, AuthStr, HttpContext, "EN");
            if (user.Role == 1)
            {
                Response.Redirect("/admin", false);
            }
            if (user.Role == 2)
            {
                Response.Redirect("/worked", false);
            }
            else
            {
                State = "Your account is inactive";
            }

        }

    }
}
