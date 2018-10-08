using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;

namespace PDKS.Pages
{
    public class LangResource
    {
        ///<summary>
        ///IO Reads then returns language resource as a
        ///JObject depending on the page and language 
        ///</summary>
        public static JObject GetLanguageJson(string page, string lang)
        {
            string path = System.IO.Directory.GetCurrentDirectory();
            StreamReader resourcefile = System.IO.File.OpenText("./resources/" + page + "." + lang + ".json");
            var reContent = resourcefile.ReadToEnd();
            resourcefile.Close();
            return JObject.Parse(reContent);
        }
    }
}
