using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json.Linq;

namespace PDKS.Pages
{
    public class RequestsTableModel : PageModel
    {
        private readonly PDKS.Context _context;
        public List<Data> reqs { get; set; }
        public string State;
        public JObject lang = new JObject();

        ///<summary>
        ///Fetches the context and configuration to the current model
        ///</summary>
        public RequestsTableModel(PDKS.Context context)
        {
            _context = context;
        }
        ///<summary>
        ///data structure for keeping the user requests temporarily
        ///</summary>
        public class Data
        {
            public Data(int id, int userId, string name, string excuse, int request, string day)
            {
                Id = id;
                UserId = userId;
                Excuse = excuse;
                Name = name;
                Request = request;
                Day = day;
            }
            public int Id;
            public int UserId;
            public string Name;
            public string Excuse;
            public int Request;
            public string Day;
        }
        ///<summary>
        ///Default get handler: 
        ///Authenticates the user, gets the language resource
        ///, fills the "reqs" list and finally provides the page
        ///</summary>
        public void OnGet()
        {
            string redirectionPath = Interconnector.Authenticator(HttpContext.User, EPage.authTable);
            if (redirectionPath != "")
            {
                Response.Redirect(redirectionPath, false);
            }
            lang = LangResource.GetLanguageJson("requestsTable", User.FindFirst("Language").Value);
            var obj = (from w in _context.UserWorkedSet
                       join u in _context.UserSet
                       on w.UserId equals u.Id
                       where w.ReqApproved == 3
                       select new
                       {
                           UserId = u.Id
                           ,
                           Id = w.Id
                           ,
                           Name = u.Name
                           ,
                           Excuse = w.Excuse
                           ,
                           Request = w.Request
                           ,
                           Day = w.date
                       }).ToList();
            if (obj.Count == 0)
            {
                State = "No users found.";
            }
            reqs = new List<Data>();
            for (int i = 0; i < obj.Count; i++)
            {
                reqs.Add(new Data(obj[i].Id,
                    obj[i].UserId,
                    obj[i].Name,
                    obj[i].Excuse,
                    obj[i].Request,
                    obj[i].Day.ToString().Substring(0, obj[i].Day.ToString().Length - 9)
                ));
            }
            ViewData["Table"] = reqs;
        }
        ///<summary>
        ///redirects to "requestRespond" page
        ///</summary>
        public void OnPost()
        {
            Response.Redirect("/requestRespond", false);
            return;
        }
    }
}
