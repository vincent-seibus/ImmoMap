using log4net;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Newtonsoft.Json;
using Reta.Models;
using Stripe;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Web;

namespace Reta.Controllers.Helpers
{
    public class PaymentService
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(PaymentService));
       
        public bool payByCard(PaymentDetails paymentDetails, string userId)
        {
            var charge = paymentDetails.paymentCardDetails;                     

            // Treat the info to be ready to put into the charge object for stripe api
            int? amountTreated = Convert.ToInt32(Math.Round(Convert.ToDouble(charge.amount, new CultureInfo("en-US")) * 100), new CultureInfo("en-US"));

            // get Stripe Customer Id 
            string stripeCustomerId = getStripeCustomerId(userId);
            var user = getUser(userId); 

            // insatnce of card service
            var tokenService = new StripeTokenService();          
            StripeTokenCreateOptions stripeToken = new StripeTokenCreateOptions()
            {
                Card = new StripeCreditCardOptions()
                {
                    // Capture = true,
                    Cvc = charge.cvc,
                    // Description = "",                   
                    ExpirationMonth = charge.month,
                    ExpirationYear = charge.year,
                    Number = charge.number,
                    Name = charge.cardHolderName
                }
                //,CustomerId = stripeCustomerId
            };

            StripeToken token = tokenService.Create(stripeToken);

            // Instance of charge service
            var chargeService = new StripeChargeService();


            // Put info in the Charge object
            StripeChargeCreateOptions chargeCasted = new StripeChargeCreateOptions()
            {
                Amount = amountTreated,
                //ApplicationFee = applicationFeeTreated,
                Capture = true,
                Currency = "EUR",
                //CustomerId = stripeCustomerId,
                Description = "",
                //Destination = "",
                SourceTokenOrExistingSourceId = token.Id,
                ReceiptEmail = user.Email,
            };

            // Send charge to stripe API
            StripeCharge response = chargeService.Create(chargeCasted);

            if(response.Status == "succeeded")
                return true;

            return false;
        }

        private string getStripeCustomerId(string userId)
        {
            //Instanciate usermanager
            ApplicationDbContext context = new ApplicationDbContext();
            IUserStore<ApplicationUser> store = new UserStore<ApplicationUser>(context);
            ApplicationUserManager UserManager = new ApplicationUserManager(store);
            // get the claim where the StripeCustomerId is stored for the user id passed in parameters
            var user = UserManager.FindByIdAsync(userId).Result;
            var claims = UserManager.GetClaimsAsync(user.Id).Result;
            string stripeCustomerId = claims.Where(a => a.Type == "StripeCustomerId").FirstOrDefault()?.Value;

            return stripeCustomerId;

        }

        private ApplicationUser getUser(string userId)
        {
            //Instanciate usermanager
            ApplicationDbContext context = new ApplicationDbContext();
            IUserStore<ApplicationUser> store = new UserStore<ApplicationUser>(context);
            ApplicationUserManager UserManager = new ApplicationUserManager(store);
            return UserManager.FindById(userId);
        }
    }

    public class PaymentDetails
    {
        public paymentCardDetails paymentCardDetails { get; set; }
        public purchaseBitcoinDetails purchaseDetails { get; set; }
    }

    public class paymentCardDetails
    {
        public string cardHolderName { get; set; }
        public string number { get; set; }
        public string month { get; set; }
        public string year { get; set; }
        public string cvc { get; set; }
        public string amount { get; set; }
    }

    public class purchaseBitcoinDetails
    {
        public string quantity { get; set; }
        public string amount { get; set; }
        public string address { get; set; }
    }
}