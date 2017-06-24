using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;


namespace Reta.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        [AllowAnonymous]
        public RedirectResult Index()
        {
            var version = ConfigurationManager.AppSettings["AppVersion"];
           return Redirect("~/angular/index.html?version=" + version);
        }            
    
    }
}