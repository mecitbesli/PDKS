using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Novell.Directory.Ldap;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;

namespace PDKS.Pages
{
    public class AdminModel : PageModel
    {
        public Dictionary<int, string> users;
        public string Name;
        public JObject lang = new JObject();
        List<Rapor> raporlar { get; set; }
        public IConfiguration Configuration { get; }
        private readonly PDKS.Context _context;

        ///<summary>
        ///Fetches the context and configuration to the current model
        ///</summary>
        public AdminModel(PDKS.Context context, IConfiguration configuration)
        {
            Configuration = configuration;
            _context = context;
        }
        ///<summary>
        ///data structure for keeping the user permits temporarily
        ///</summary>
        public class Rapor
        {
            public string Id { get; set; }//PERSONNELNUMBER
            public string IzinTip { get; set; }//İzin Tipi
            public string IzinTarih { get; set; }//İzin Tarihi
            public string BasSaat { get; set; }//BaslangicSaati
            public string BitSaat { get; set; }//BitisSaati
            public Decimal Saat { get; set; }//Saat
            public string Durum { get; set; }//Durum
        }
        ///<summary>
        ///Converts input mins to HH:MM format
        ///</summary>
        public string MinsToTime(int mins)
        {
            return (mins / 3600).ToString("D2")
                + ":"
                + ((mins / 60) % 60).ToString("D2");
        }
        ///<summary>
        ///Default get handler: 
        ///Authenticates the user, gets the language resource
        ///and finally provides the page
        ///</summary>
        public void OnGet()
        {
            string redirectionPath = Interconnector.Authenticator(HttpContext.User, EPage.admin);
            if (redirectionPath != "")
            {
                Response.Redirect(redirectionPath, false);
                return;
            }
            Name = User.FindFirst("FullName").Value;
            lang = LangResource.GetLanguageJson("admin", User.FindFirst("Language").Value);
        }
        public void OnPost() { }
        ///<summary>
        ///Provides the json data for the permits popup table
        ///but gets only a 10 row page.
        ///can feed page number through "getPage" parameter
        ///</summary>
        public ActionResult OnPostPermits()
        {
            lang = LangResource.GetLanguageJson("admin", User.FindFirst("Language").Value);
            int reqPageOffset = 1;
            Int32.TryParse(HttpContext.Request.Form["getPage"], out reqPageOffset);
            reqPageOffset = (reqPageOffset * 10) - 10;
            string IzinDbPath = Configuration.GetSection("Connections:IzinMsSqlDB").Value;
            using (SqlConnection conn = new SqlConnection())
            {
                conn.ConnectionString = IzinDbPath;
                SqlCommand cmd = new SqlCommand();
                SqlDataReader reader;
                cmd.CommandText = "SELECT * FROM SMP_BORAX_IZIN order by \"İzin Tarihi\" OFFSET " + reqPageOffset + " ROWS FETCH NEXT 10 ROWS ONLY;";
                cmd.Connection = conn;
                raporlar = new List<Rapor>();
                conn.Open();
                reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    Rapor rapor = new Rapor
                    {
                        Id = (string)reader.GetValue(0)
                        ,
                        IzinTip = (string)reader.GetValue(1)
                        ,
                        IzinTarih = ((DateTime)reader.GetValue(2)).ToString()
                        ,
                        BasSaat = MinsToTime((Int32)reader.GetValue(3))
                        ,
                        BitSaat = MinsToTime((Int32)reader.GetValue(4))
                        ,
                        Saat = (Decimal)reader.GetValue(5)
                        ,
                        Durum = (string)reader.GetValue(6)
                    };
                    rapor.IzinTarih = rapor.IzinTarih.Substring(0, 10);

                    raporlar.Add(rapor);
                    //System.Console.WriteLine(rapor.Id + "-" + rapor.IzinTip + "-" + rapor.IzinTarih + "-" + rapor.BasSaat + "-" + rapor.BitSaat + "-" + rapor.Saat + "-" + rapor.Durum);
                }
                return new JsonResult(JsonConvert.SerializeObject(raporlar));
            }
        }
        ///<summary>
        ///redirects to "authTable" page
        ///</summary>
        public void OnPostAuth()
        {
            Response.Redirect("/authTable", false);
        }
        ///<summary>
        ///redirects to "worked" page
        ///</summary>
        public void OnPostWorked()
        {
            Response.Redirect("/worked", false);
        }
        ///<summary>
        ///redirects to "hoursChange" page
        ///</summary>
        public void OnPostChange()
        {
            Response.Redirect("/hoursChange", false);
        }
        ///<summary>
        ///redirects to "requestRespond" page
        ///</summary>
        public void OnPostRespond()
        {
            Response.Redirect("/requestRespond", false);
        }
        ///<summary>
        ///Calls the interconnector to change the
        /// password of the user accordingly
        ///Requires: prevPwInput, newPwInput, newPwInput2 
        /// through request
        ///</summary>
        public ActionResult OnPostPWChange()
        {
            lang = LangResource.GetLanguageJson("admin", User.FindFirst("Language").Value);
            return new JsonResult(lang[Interconnector.PasswordChange(_context, HttpContext)]);
        }
        ///<summary>
        ///Handles the remote->local sync buttons.
        ///does a mass sync to purge the unmatching user records locally or
        ///checks if the name of a user exists on remote server or
        ///adds user from remote db to local db with entered name 
        ///Requires: requester; Optionally: name
        ///</summary>
        public ActionResult OnPostSync()
        {
            string resId = "";
            string resName = "";
            User user;
            if (HttpContext.Request.Form["requester"].Contains("mass"))
            {
                using (SqlConnection conn = new SqlConnection())
                {
                    var query =
                        from us in _context.UserSet
                        select us;
                    if (query.Count() == 0)
                        return new JsonResult("No users on db.");
                    SqlCommand cmd = new SqlCommand();
                    cmd.CommandText = "select * from SMP_PERSONEL where PERSONNELNUMBER in(";
                    List<User> usersLocal = new List<User>();
                    foreach (User us in query)
                    {
                        usersLocal.Add(us);
                        cmd.CommandText += "'" + us.Username + "',";
                    }
                    cmd.CommandText = cmd.CommandText.Substring(0, cmd.CommandText.Length - 1);
                    cmd.CommandText += ");";
                    string IzinDbPath = Configuration.GetSection("Connections:IzinMsSqlDB").Value;
                    conn.ConnectionString = IzinDbPath;
                    SqlDataReader reader;
                    System.Console.WriteLine(cmd.CommandText);
                    cmd.Connection = conn;
                    conn.Open();
                    reader = cmd.ExecuteReader();
                    string tmp = "";
                    while (reader.Read())
                    {
                        tmp += (string)reader.GetValue(0);
                    }
                    foreach (User u in usersLocal)
                    {
                        if (tmp.Contains(u.Username))
                        {
                            continue;
                        }
                        _context.Remove(u);
                    }
                }
            }
            else
            {
                Name = HttpContext.Request.Form["name"];
                try
                {
                    using (SqlConnection conn = new SqlConnection())
                    {
                        try
                        {
                            user = (User)_context.UserSet.Single(b => (b.Name == Name));
                            return new JsonResult("User already exists in local db");
                        }
                        catch (Exception e)
                        {
                            System.Console.WriteLine(e.Message);
                        }
                        string IzinDbPath = Configuration.GetSection("Connections:IzinMsSqlDB").Value;
                        conn.ConnectionString = IzinDbPath;
                        SqlCommand cmd = new SqlCommand();
                        SqlDataReader reader;
                        if (("ĞŞÇİÜÖ").Contains(Name.First()))
                            /*UNSOLVED,worked around: 
                            where EMPLNAME like user.Name not working. */
                            cmd.CommandText = "select * from SMP_PERSONEL";
                        else
                            cmd.CommandText = "select * from SMP_PERSONEL where EMPLNAME like '" + Name.First() + "%'; ";
                        System.Console.WriteLine(cmd.CommandText);
                        cmd.Connection = conn;
                        conn.Open();
                        reader = cmd.ExecuteReader();
                        bool resIsEmpty = true;
                        while (reader.Read())
                        {
                            if (!((string)reader.GetValue(1)).Contains(Name)) continue;
                            resIsEmpty = false;
                            resId = (string)reader.GetValue(0);
                            resName = (string)reader.GetValue(1);
                        }
                        if (resIsEmpty)
                            return new JsonResult("No such users at remote db");
                    }
                }
                catch (Exception e)
                {
                    System.Console.WriteLine(e.Message);
                    return new JsonResult("No data on repo.");
                }
            }
            if (HttpContext.Request.Form["requester"].Contains("checker"))
                return new JsonResult(resId + " - " + resName);
            else if (!HttpContext.Request.Form["requester"].Contains("mass"))
            {
                try
                {
                    user = (User)_context.UserSet.Single(b => (b.Username == resId));
                    return new JsonResult("User already exists in local db");
                }
                catch (Exception e)
                {
                    System.Console.WriteLine(e.Message);
                }
                _context.UserSet.Add(new User
                {
                    Name = resName,
                    Username = resId,
                    Role = 2,
                    DaysOff = 15,
                    Password = Crypt.Encrypt("sifre"),
                    StandartWorkHoursId = 1
                });
                _context.SaveChanges();
                user = (User)_context.UserSet.Single(b => (b.Name == resName));
                _context.AdminAuthorizationSet.Add(new AdminAuthorization
                {
                    Customize = false,
                    Requests = false,
                    Authority = false,
                    UserId = user.Id
                });
                _context.SaveChanges();
                return new JsonResult("Added user successfully");
            }
            else { return new JsonResult("DONE"); }
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
                Response.Redirect("/admin", false);
                return;
            }
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);
            Response.Redirect("/login", false);
        }
    }
}
