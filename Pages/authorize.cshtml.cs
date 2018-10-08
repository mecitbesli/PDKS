using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json.Linq;

namespace PDKS.Pages
{
    public class AuthorizeModel : PageModel
    {
        public string State;
        public JObject lang = new JObject();
        public List<AdminAuthorization> auths { get; set; }
        public string initSelect = "";
        private readonly PDKS.Context _context;

        ///<summary>
        ///Fetches the context to the current model
        ///</summary>
        public AuthorizeModel(PDKS.Context context)
        {
            _context = context;
        }
        ///<summary>
        ///Default get handler: 
        ///Authenticates the user and provides the page
        ///</summary>
        public void OnGet()
        {
            try
            {
                if (!HttpContext.User.Identity.IsAuthenticated)
                {
                    Response.Redirect("/login", false);
                }
            }
            catch (Exception)
            {
                Response.Redirect("/login", false);
            }
            OnGetFromTable();
        }
        ///<summary>
        ///Handles get requests from "authTable".
        ///authenticates the user, gets the language resource
        ///and finally provides the page. Optionally gets an Id for client to preselect
        ///</summary>
        public void OnGetFromTable(string id = "1")
        {
            initSelect = id;
            var obj = _context.AdminAuthorizationSet.ToList();
            auths = obj as List<AdminAuthorization>;
            lang = LangResource.GetLanguageJson("authorize", User.FindFirst("Language").Value);
        }
        ///<summary>
        ///Depending on the selections gives/takes authorizations
        ///from users. Requires: authCheckboxes1, authCheckboxes2,
        /// button, selection parameters through request
        ///</summary>
        public void OnPostAuth()
        {
            string Check1 = HttpContext.Request.Form["authCheckboxes1"];
            string Check2 = HttpContext.Request.Form["authCheckboxes2"];
            string Button = HttpContext.Request.Form["button"];
            if (Button.Contains("Back"))
            {
                Response.Redirect("/authTable", false);
                return;
            }
            int Id = 0;
            if (!Int32.TryParse(HttpContext.Request.Form["selection"], out Id))
            {
                OnGetFromTable();
                State = "Request Failed";
                return;
            }
            AdminAuthorization qA;
            try
            {
                qA = (AdminAuthorization)(from a in _context.AdminAuthorizationSet
                                          where a.Id == Id
                                          select a).First();
            }
            catch (Exception)
            {
                _context.AdminAuthorizationSet.Add(new AdminAuthorization { UserId = Id, Customize = false, Requests = false, Authority = false });
                _context.SaveChanges();
                qA = (AdminAuthorization)(from a in _context.AdminAuthorizationSet
                                          where a.Id == Id
                                          select a).First();
            }
            User qU =
                (User)(from a in _context.UserSet
                       where a.Id == qA.UserId
                       select a).First();
            if (Button == "Authorize")
            {
                if (Check1 == "on")
                {
                    qA.Customize = true;
                    qU.Role = 1;
                }
                if (Check2 == "on")
                {
                    qA.Requests = true;
                    qU.Role = 1;
                }
            }
            else if (Button == "Revoke")
            {
                if (Check1 == "on")
                {
                    qA.Customize = false;
                    if (!qA.Requests) qU.Role = 2;
                }
                if (Check2 == "on")
                {
                    qA.Requests = false;
                    if (!qA.Customize) qU.Role = 2;
                }
            }
            try
            {
                _context.SaveChanges();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            OnGetFromTable();
            State = "Success";
        }
    }
}
