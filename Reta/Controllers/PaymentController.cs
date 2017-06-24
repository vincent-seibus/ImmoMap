using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Reta.Controllers.Helpers;
using Microsoft.AspNet.Identity;
using log4net;

namespace Reta.Controllers
{
    [Authorize(Roles="Supervisor")]
    public class PaymentController : Controller
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(PaymentController));
        // POST : payment 
        [HttpPost]
        public string PostPaymentByCard(string value)
        {
            //get the data in the object 
            PaymentDetails paymentDetails = JsonConvert.DeserializeObject<PaymentDetails>(value);
            //Initialize the services
            PaymentService service = new PaymentService();
            //Check the payment data provided and return error if invalid

            // Pay via stripe with the card details provided
            if (!service.payByCard(paymentDetails, User.Identity.GetUserId()))
            {
                logger.Error("payment stopped as Stripe payment failed");
                return JsonConvert.SerializeObject(new { error = "true", errorMessage = "paymentFailed" });
            }

          
            return JsonConvert.SerializeObject(new { error = "false" });
        }

    }
}