using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json.Linq;

namespace PDKS.Pages
{
    public class ExcuseModel : PageModel
    {
        public JObject lang = new JObject();
        public List<String> days;
        public int clickedIndex;
        private int Id;
        public string State;
        private readonly PDKS.Context _context;

        ///<summary>
        ///Fetches the context and configuration to the current model
        ///</summary>
        public ExcuseModel(PDKS.Context context)
        {
            _context = context;
        }
        ///<summary>
        ///Default get handler: 
        ///Authenticates the user, gets the language resource,
        ///passes the selected day to client,
        ///fills the "days" list and finally provides the page
        ///</summary>
        public void OnGet(string day = "")
        {
            string redirectionPath = Interconnector.Authenticator(HttpContext.User, EPage.excuse);
            if (redirectionPath != "")
            {
                Response.Redirect(redirectionPath, false);
            }
            Int32.TryParse(HttpContext.User.Identity.Name, out Id);
            lang = LangResource.GetLanguageJson("excuse", User.FindFirst("Language").Value);
            var query =
                from u in _context.UserWorkedSet
                where u.UserId == Id
                && u.ReqApproved == 2
                && u.WorkedTime < 8
                select u;
            days = new List<string>();
            foreach (UserWorked u in query)
            {
                days.Add(u.date.ToString("yyyy.MM.dd") + " / " + u.date.DayOfWeek);
                try
                {
                    if (DateTime.Equals(u.date, DateTime.Parse(day))) clickedIndex = days.Count - 1;
                }
                catch (Exception)
                {
                    System.Console.WriteLine("No day input");
                }
            }
        }
        ///<summary>
        ///Updates the state of an absent day with the users excuse.
        ///Requires: excuseRadios, requestRadios, requestRadios, button
        ///, button through request
        ///</summary>
        public void OnPostSubmit()
        {
            string excuseRadios = HttpContext.Request.Form["excuseRadios"];
            string requestRadios = HttpContext.Request.Form["requestRadios"];
            string selection = HttpContext.Request.Form["selection"];
            string button = HttpContext.Request.Form["button"];
            string customExcuse;
            DateTime selectedDate = new DateTime();
            if (button.Contains("Back"))
            {
                Response.Redirect("/worked", false);
                return;
            }
            if (!DateTime.TryParse(selection.Substring(0, 10), out selectedDate))
            {
                OnGet();
                State = "Invalid date.";
                return;
            }
            Int32.TryParse(HttpContext.User.Identity.Name, out Id);
            customExcuse = HttpContext.Request.Form["customExcuse"];
            var query =
                from u in _context.UserWorkedSet
                where u.UserId == Id
                && u.date == selectedDate
                select u;
            foreach (UserWorked u in query)
            {
                if (excuseRadios.Contains("option1")) u.Excuse = "I had been working at another company for Sampaş";
                else if (excuseRadios.Contains("option2")) u.Excuse = "I was sick";
                else if (excuseRadios.Contains("option3")) u.Excuse = "I had/have personal problems";
                else u.Excuse = customExcuse;
                if (requestRadios.Contains("option1")) u.Request = 0;
                else u.Request = 1;
                u.ReqApproved = 3;
            }
            try
            {
                _context.SaveChanges();
                OnGet();
                State = "Submitted Successfully.";
                return;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
