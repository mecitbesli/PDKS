using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json.Linq;

namespace PDKS.Pages
{
    public class RequestRespondModel : PageModel
    {
        public JObject lang = new JObject();
        public string State;
        public List<Data> users;
        private readonly PDKS.Context _context;

        ///<summary>
        ///Fetches the context and configuration to the current model
        ///</summary>
        public RequestRespondModel(PDKS.Context context)
        {
            _context = context;
        }
        ///<summary>
        ///data structure for keeping the users temporarily
        ///</summary>
        public class Data
        {
            public Data(int userId, string name, int id)
            {
                UserId = userId;
                Name = name;
                Id = id;
            }
            public int UserId;
            public string Name;
            public int Id;
        }
        ///<summary>
        ///Default get handler: 
        ///Authenticates the user, gets the language resource
        ///, fills the "users" list and finally provides the page
        ///</summary>
        public void OnGet()
        {
            string redirectionPath = Interconnector.Authenticator(HttpContext.User, EPage.requestRespond);
            if (redirectionPath != "")
            {
                Response.Redirect(redirectionPath, false);
                return;
            }
            lang = LangResource.GetLanguageJson("requestRespond", User.FindFirst("Language").Value);
            var obj = (from u in _context.UserSet
                       join w in _context.UserWorkedSet
                       on u.Id equals w.UserId
                       where u.Id != Int32.Parse(HttpContext.User.Identity.Name)
                       && w.ReqApproved == 3
                       select new
                       {
                           UserId = u.Id
                                   ,
                           Name = u.Name
                                   ,
                           Id = w.Id
                       }).ToList();
            users = new List<Data>();
            if (obj.Count == 0)
            {
                State = "No requests found.";
                return;
            }
            for (int i = 0; i < obj.Count; i++)
            {
                users.Add(new Data(obj[i].UserId, obj[i].Name, obj[i].Id));
            }
            ViewData["Table"] = users;
        }
        ///<summary>
        ///Responses the user absency excuses.
        ///redirects to "admin", "requestsTable" pages
        ///Requires: reqSelect, button through request
        ///</summary>
        public void OnPost()
        {
            string reqSelect = HttpContext.Request.Form["reqSelect"];
            string Button = HttpContext.Request.Form["button"];
            if (Button.Contains("Back"))
            {
                Response.Redirect("/admin", false);
            }
            else if (Button.Contains("Check"))
            {
                Response.Redirect("/requestsTable", false);
            }
            else
            {
                UserWorked obj;
                try
                {
                    obj = (UserWorked)_context.UserWorkedSet.Single(b => (b.Id == Int32.Parse(reqSelect)));
                }
                catch (Exception)
                {
                    OnGet();
                    State = "Response process failed.";
                    return;
                }
                if (Button.Contains("Accept")) obj.ReqApproved = 1;
                else if (Button.Contains("Decline")) obj.ReqApproved = 0;
                try
                {
                    _context.SaveChanges();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    OnGet();
                    State = "Response process failed.";
                    return;
                }
                OnGet();
                State = "Responded succesfully.";
                return;
            }
        }
    }
}
