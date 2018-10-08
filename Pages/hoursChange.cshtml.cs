using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json.Linq;

namespace PDKS.Pages
{
    public class HoursChangeModel : PageModel
    {
        public JObject lang = new JObject();
        public string State;
        public Dictionary<int, string> users;
        private readonly PDKS.Context _context;

        ///<summary>
        ///Fetches the context and configuration to the current model
        ///</summary>
        public HoursChangeModel(PDKS.Context context)
        {
            _context = context;
        }
        ///<summary>
        ///Default get handler: 
        ///Authenticates the user, gets the language resource
        ///, fills the "users" list and finally provides the page
        ///</summary>
        public void OnGet()
        {
            string redirectionPath = Interconnector.Authenticator(HttpContext.User, EPage.hoursChange);
            if (redirectionPath != "")
            {
                Response.Redirect(redirectionPath, false);
                return;
            }
            lang = LangResource.GetLanguageJson("hoursChange", User.FindFirst("Language").Value);
            var query =
                from u in _context.UserSet
                where u.Id != Int32.Parse(HttpContext.User.Identity.Name)
                select u;
            users = new Dictionary<int, string>();
            foreach (User u in query)
            {
                users[u.Id] = u.Name;
            }
        }
        ///<summary>
        ///Changes the shifts of a user
        ///redirects to "hoursTable", "admin" pages
        ///Requires: button, idSelect, shiftSelect
        /// through request
        ///</summary>
        public void OnPost()
        {
            string button = HttpContext.Request.Form["button"];
            if (button.Contains("Back"))
            {
                Response.Redirect("/admin", false);
                return;
            }
            else if (button.Contains("Check"))
            {
                Response.Redirect("/hoursTable", false);
                return;
            }
            else
            {
                string idSelect = HttpContext.Request.Form["idSelect"];
                string shiftSelect = HttpContext.Request.Form["shiftSelect"];
                try
                {
                    var query =
                        from u in _context.UserSet
                        where u.Id == Int32.Parse(idSelect)
                        select u;
                    foreach (User u in query)
                    {
                        u.StandartWorkHoursId = Int32.Parse(shiftSelect);
                        OnGet();
                        State = "Success";
                        return;
                    }
                    _context.SaveChanges();
                }
                catch (Exception e)
                {
                    State = "Failed";
                    OnGet();
                    Console.WriteLine(e.Message);
                }
            }
        }
    }
}
