using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json.Linq;

namespace PDKS.Pages
{
    public class HoursTableModel : PageModel
    {
        public JObject lang = new JObject();
        private readonly PDKS.Context _context;
        public List<Data> users { get; set; }
        public string State;

        ///<summary>
        ///Fetches the context and configuration to the current model
        ///</summary>
        public HoursTableModel(PDKS.Context context)
        {
            _context = context;
        }
        ///<summary>
        ///data structure for keeping the user shifts temporarily
        ///</summary>
        public class Data
        {
            public Data(int id, string name, string startHr, string endHr, int breakTime)
            {
                Id = id;
                Name = name;
                StartHr = startHr;
                EndHr = endHr;
                BreakTime = breakTime;
            }
            public int Id;
            public string Name;
            public string StartHr;
            public string EndHr;
            public int BreakTime;
        }
        ///<summary>
        ///Default get handler: 
        ///Authenticates the user, gets the language resource
        ///, fills the "users" list and finally provides the page
        ///</summary>
        public void OnGet()
        {
            string redirectionPath = Interconnector.Authenticator(HttpContext.User, EPage.authTable);
            if (redirectionPath != "")
            {
                Response.Redirect(redirectionPath, false);
            }
            lang = LangResource.GetLanguageJson("hoursTable", User.FindFirst("Language").Value);
            var obj = (from s in _context.StandartWorkhoursSet
                       join u in _context.UserSet
                       on s.Id equals u.StandartWorkHoursId
                       select new
                       {
                           Id = u.Id
                           ,
                           Name = u.Name
                           ,
                           StartHr = s.StartHr
                           ,
                           EndHr = s.EndHr
                           ,
                           BreakTime = s.BreakTime
                       }).ToList();
            if (obj.Count == 0)
            {
                State = "No users found.";
            }
            users = new List<Data>();
            for (int i = 0; i < obj.Count; i++)
            {
                users.Add(new Data(obj[i].Id,
                    obj[i].Name,
                    obj[i].StartHr.ToString().Substring(10, obj[i].StartHr.ToString().Length - 10),
                    obj[i].EndHr.ToString().Substring(10, obj[i].EndHr.ToString().Length - 10),
                    obj[i].BreakTime
                ));
            }
            ViewData["Table"] = users;
        }
        ///<summary>
        ///redirects to "hoursChange" page
        ///</summary>
        public void OnPost()
        {
            Response.Redirect("/hoursChange", false);
            return;
        }
    }
}
