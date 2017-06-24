using log4net;
using Stripe;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace Reta
{
    public class MvcApplication : System.Web.HttpApplication
    {
        private static ILog logger = LogManager.GetLogger(typeof(MvcApplication));
        protected void Application_Start()
        {
            log4net.Config.XmlConfigurator.Configure();
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);           

            //Stripe config             
            StripeConfiguration.SetApiKey(ConfigurationManager.AppSettings["Payment:StripeKey"]);

            // For db migration if not initial start up
            if (!Convert.ToBoolean(ConfigurationManager.AppSettings["InitializeDb"] != null ? ConfigurationManager.AppSettings["InitializeDb"] : "false"))
                App_Start.SqlQuery.Initialize();
            else if(Convert.ToBoolean(ConfigurationManager.AppSettings["InitializeDb"] != null ? ConfigurationManager.AppSettings["InitializeDb"] : "false"))
                App_Start.SqlQuery.InitializeMySqlDatabase();
            
            logger.Info("Application start correctly");
            
        }
    }
}
