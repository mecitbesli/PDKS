using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json.Linq;

namespace PDKS.Pages
{
    public class ExcuseStatesModel : PageModel
    {
        public JObject lang = new JObject();
        public List<Data> reqs { get; set; }
        private readonly PDKS.Context _context;

        ///<summary>
        ///Fetches the context to the current model
        ///</summary>
        public ExcuseStatesModel(PDKS.Context context)
        {
            _context = context;
        }
        ///<summary>
        ///data structure for keeping the current user excuses temporarily
        ///</summary>
        public class Data
        {
            public Data(ClaimsPrincipal User, int id, string excuse, int request, int response, string date)
            {
                JObject lang = LangResource.GetLanguageJson("excuseStates", User.FindFirst("Language").Value);
                Id = id;
                Excuse = excuse;
                Date = date;
                if (request == 0) Request = "Change Status";
                else if (request == 1) Request = "Use Day off";
                if (response == 0) Response = "No";
                else if (response == 1) Response = "Yes";
                else if (response == 2) Response = "Waiting User";
                else if (response == 3) Response = "Waiting Admin";
            }
            public int Id;
            public string Excuse;
            public string Request;
            public string Response;
            public string Date;
        }
        ///<summary>
        ///Default get handler: 
        ///Authenticates the user, gets the language resource,
        ///fills the "reqs" list and finally provides the page
        ///</summary>
        public void OnGet()
        {
            string redirectionPath = Interconnector.Authenticator(HttpContext.User, EPage.excuseStates);
            if (redirectionPath != "")
            {
                Response.Redirect(redirectionPath, false);
                return;
            }
            string name = User.FindFirst("FullName").Value;
            lang = LangResource.GetLanguageJson("excuseStates", User.FindFirst("Language").Value);
            var obj = (from w in _context.UserWorkedSet
                       where w.UserId == Int32.Parse(User.FindFirst(ClaimTypes.Name).Value) && w.WorkedTime < 8 && w.ReqApproved != 2
                       select new
                       {
                           Id = w.Id
                           ,
                           Excuse = w.Excuse
                           ,
                           Request = w.Request
                           ,
                           Response = w.ReqApproved
                           ,
                           Date = w.date
                       }).ToList();
            reqs = new List<Data>();
            for (int i = 0; i < obj.Count; i++)
            {
                reqs.Add(new Data(User,
                    obj[i].Id,
                    obj[i].Excuse,
                    obj[i].Request,
                    obj[i].Response,
                    obj[i].Date.ToString().Substring(0, obj[i].Date.ToString().Length - 9)
                ));
            }
            ViewData["Table"] = reqs;
        }
        ///<summary>
        ///redirects to "worked" page
        ///</summary>
        public void OnPost()
        {
            Response.Redirect("/worked", false);
        }
    }
}
