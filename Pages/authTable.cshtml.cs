using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json.Linq;

namespace PDKS.Pages
{
    public class AuthTableModel : PageModel
    {
        public JObject lang = new JObject();
        private readonly PDKS.Context _context;
        public List<Data> auths { get; set; }

        ///<summary>
        ///Fetches the context and configuration to the current model
        ///</summary>
        public AuthTableModel(PDKS.Context context)
        {
            _context = context;
        }
        ///<summary>
        ///data structure for keeping the user authorities temporarily
        ///</summary>
        public class Data
        {
            public Data(int id, string name, bool customize, bool requests, bool authority)
            {
                Id = id;
                Name = name;
                Customize = customize;
                Requests = requests;
                Authority = authority;
            }
            public int Id;
            public string Name;
            public bool Customize;
            public bool Requests;
            public bool Authority;
        }
        ///<summary>
        ///Default get handler: 
        ///Authenticates the user, gets the language resource,
        ///fills the "auths" list and finally provides the page
        ///</summary>
        public void OnGet()
        {
            string redirectionPath = Interconnector.Authenticator(HttpContext.User, EPage.authTable);
            if (redirectionPath != "")
            {
                Response.Redirect(redirectionPath, false);
            }
            lang = LangResource.GetLanguageJson("authTable", User.FindFirst("Language").Value);

            var obj = (from a in _context.AdminAuthorizationSet
                       join u in _context.UserSet
                       on a.UserId equals u.Id
                       select new
                       {
                           Id = a.Id
                           ,
                           Name = u.Name
                           ,
                           Customize = a.Customize
                           ,
                           Requests = a.Requests
                           ,
                           Authority = a.Authority
                       }).ToList();
            if (obj.Count == 0)
            {
                Response.Redirect("/admin", false);
            }
            auths = new List<Data>();
            for (int i = 0; i < obj.Count; i++)
            {
                auths.Add(new Data(obj[i].Id, obj[i].Name, obj[i].Customize, obj[i].Requests, obj[i].Authority));
            }
            ViewData["Table"] = auths;
        }
        ///<summary>
        ///redirects to "authorize" page with input parameter added to it
        ///</summary>
        public void OnPostProceed(String Selection)
        {
            Response.Redirect("/authorize?handler=fromTable&Id=" + Selection, false);
        }
        ///<summary>
        ///redirects to "admin" page
        ///</summary>
        public void OnPostBack()
        {
            Response.Redirect("/admin", false);
        }
    }
}
