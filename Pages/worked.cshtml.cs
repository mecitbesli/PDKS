using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Globalization;
using Microsoft.AspNetCore.Html;
using Newtonsoft.Json.Linq;

namespace PDKS.Pages
{
    public class WorkedModel : PageModel
    {
        public JObject lang = new JObject();
        public Dictionary<int, Week> weeks;
        public bool UserIsAdmin = false;
        public string Name;
        private readonly PDKS.Context _context;

        ///<summary>
        ///Contains functions to be used by client side script
        ///</summary>
        public static class JScriptConvert
        {
            ///<summary>
            ///Converts an object to a json
            ///</summary>
            public static IHtmlContent SerializeObject(object value)
            {
                using (var stringWriter = new StringWriter())
                using (var jsonWriter = new JsonTextWriter(stringWriter))
                {
                    var serializer = new JsonSerializer
                    {
                        ContractResolver = new CamelCasePropertyNamesContractResolver()
                    };
                    jsonWriter.QuoteName = false;
                    serializer.Serialize(jsonWriter, value);

                    return new HtmlString(stringWriter.ToString());
                }
            }
        }
        ///<summary>
        ///States of the user requests for absent days
        ///</summary>
        public enum EDayStates
        {
            Unexcused, Excused, UserPending, AdminPending
        }
        ///<summary>
        ///data structure for keeping info about a week temporarily
        ///</summary>
        public class Week
        {
            public Dictionary<DayOfWeek, int> days;
            public Dictionary<DayOfWeek, EDayStates> dayReqs;
            public Dictionary<DayOfWeek, DateTime> dates;
        }
        ///<summary>
        ///Fetches the context to the current model
        ///</summary>
        public WorkedModel(PDKS.Context context)
        {
            _context = context;
        }
        ///<summary>
        ///Default get handler: 
        ///Authenticates the user, gets the language resource,
        ///fills the "weeks" list and finally provides the page.
        ///weeks are filled by fetching and grouping the days
        ///by week of a year
        ///</summary>
        public void OnGet()
        {
            string redirectionPath = Interconnector.Authenticator(HttpContext.User, EPage.worked);
            if (redirectionPath != "")
            {
                Response.Redirect(redirectionPath, false);
                return;
            }
            if (HttpContext.User.FindFirst("Role").Value == "1")
            {
                UserIsAdmin = true;
            }
            int uId = 0;
            Int32.TryParse(HttpContext.User.Identity.Name, out uId);

            Name = User.FindFirst("FullName").Value;
            lang = LangResource.GetLanguageJson("worked", User.FindFirst("Language").Value);
            List<UserWorked> worked = null;
            try
            {
                worked = _context.UserWorkedSet.Where(b => (b.UserId == uId)).ToList() as List<UserWorked>;
                worked.Sort((x, y) => DateTime.Compare(x.date, y.date));
                var d1 = worked.First().date;
                var d2 = worked.Last().date;
                var currentCulture = CultureInfo.CurrentCulture;
                weeks = new Dictionary<int, Week>();
                for (var dt = d1; dt <= d2; dt = dt.AddDays(1))
                {
                    var weekNo = currentCulture.Calendar.GetWeekOfYear(
                                            dt,
                                            currentCulture.DateTimeFormat.CalendarWeekRule,
                                            currentCulture.DateTimeFormat.FirstDayOfWeek);
                    if (dt.DayOfWeek == DayOfWeek.Saturday || dt.DayOfWeek == DayOfWeek.Sunday)
                        continue;
                    if (!weeks.Keys.Contains(weekNo))
                    {
                        weeks[weekNo] = new Week();
                        weeks[weekNo].dates = new Dictionary<DayOfWeek, DateTime>();
                        weeks[weekNo].days = new Dictionary<DayOfWeek, int>();
                        weeks[weekNo].dayReqs = new Dictionary<DayOfWeek, EDayStates>();
                    }
                    if (null == worked.Find(x => x.date == dt))
                    {
                        var newWorked = new UserWorked { WorkedTime = 0, date = dt, UserId = uId };
                        _context.UserWorkedSet.Add(newWorked);
                        _context.SaveChanges();
                        weeks[weekNo].days[dt.DayOfWeek] = 0;
                        weeks[weekNo].dayReqs[dt.DayOfWeek] = EDayStates.UserPending;
                    }
                    else
                    {
                        weeks[weekNo].days[dt.DayOfWeek] = worked.Find(x => x.date == dt).WorkedTime;
                        weeks[weekNo].dayReqs[dt.DayOfWeek] = (EDayStates)worked.Find(x => x.date == dt).ReqApproved;
                    }
                    weeks[weekNo].dates[dt.DayOfWeek] = dt;
                    Console.WriteLine(weekNo + " : " + dt.DayOfWeek + " : " + dt + " : " + weeks[weekNo].days[dt.DayOfWeek] + " : " + weeks[weekNo].dayReqs[dt.DayOfWeek]);
                }
            }
            catch (System.Exception e)
            {
                System.Console.WriteLine(e.Message);
                worked = new List<UserWorked>();
                worked.Add(new UserWorked());
            }
        }
        ///<summary>
        ///Handles the Logout and Language change requests.
        /// creates a new cookie with desired language or
        /// logs out the user and redirects to login page
        ///</summary>
        public async Task OnPostLogoutAsync()
        {
            string Button = HttpContext.Request.Form["button"];
            if (!Button.Contains("Logout"))
            {
                Interconnector.CreateCookie(User, HttpContext, Button);
                Response.Redirect("/worked", false);
                return;
            }
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);
            Response.Redirect("/login", false);
        }
        ///<summary>
        ///Calls the interconnector to change the
        /// password of the user accordingly
        ///Requires: prevPwInput, newPwInput, newPwInput2 
        /// through request
        ///</summary>
        public ActionResult OnPostPWChange()
        {
            lang = LangResource.GetLanguageJson("worked", User.FindFirst("Language").Value);
            return new JsonResult(lang[Interconnector.PasswordChange(_context, HttpContext)]);
        }
        ///<summary>
        ///redirects to "admin" page
        ///</summary>
        public void OnPostGoBack()
        {
            Response.Redirect("/admin", false);
        }
        ///<summary>
        ///redirects to "excuseStates" page
        ///</summary>
        public void OnPostExcuseStates()
        {
            Response.Redirect("/excuseStates", false);
        }

    }
}
