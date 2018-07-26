using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net.Http;
using Newtonsoft.Json;
using System.Net;
using System.Text;
using Stripe;

namespace StripeTest.Controllers
{
    public class StripeController : Controller
    {
        private static readonly HttpClient client = new HttpClient();

        // GET: Stripe
       /* public ActionResult Index()
        {
            ViewData["data"] = data;
            return View();
        }
*/

        public string StripeConnect(string code)
        {
            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

            var CLIENT_ID = "ca_DAOZKs12XGyanvfkqUZsa3uYf1ROmche";
            var API_KEY = "";
            var TOKEN_URI = "https://connect.stripe.com/oauth/token";
            var AUTHORIZE_URI = "https://connect.stripe.com/oauth/authorize";

            string data = "None";

            if (code != null)
            {

                //
                WebClient wc = new WebClient();

                wc.QueryString.Add("client_secret", API_KEY);
                wc.QueryString.Add("code", code);
                wc.QueryString.Add("grant_type", "authorization_code");
                //.QueryString.Add("client_id", CLIENT_ID);

                var request_data = wc.UploadValues(TOKEN_URI, "POST", wc.QueryString);
                var responseString = UnicodeEncoding.UTF8.GetString(request_data);
                data = responseString;

            }

            return data;

        }


        public ActionResult Connect(string code)
        {
            var responseString = JsonConvert.DeserializeObject(StripeConnect(code));
            // return Content(responseString.ToString(), "application/json");
            // return Content(StripeConnect(code), "application/json

            ViewData["data"] = responseString;
            return View();
        }


        /// <summary>
        ///   Checkout - Get Card Token and create customer
        /// </summary>

        public ActionResult Checkout()
        {
            var token = Request["stripeToken"];
            var email = Request["stripeEmail"];
            StripeCustomer customer = CreateStripeCustomer(email,token);

            ViewData["token"] = token;
            ViewData["customerID"] = customer.Id;

            return View();
        }


        public StripeCustomer CreateStripeCustomer(string customerEmail ,string cardToken)
        {
            StripeConfiguration.SetApiKey("");

            // Create a Customer:
            var customerOptions = new StripeCustomerCreateOptions
            {
                SourceToken = cardToken,
                Email = customerEmail,
            };
            var customerService = new StripeCustomerService();
            StripeCustomer customer = customerService.Create(customerOptions);

            return customer;
        }

        /// <summary>
        ///  Charge money from customer
        /// </summary>

        public ActionResult ProceedTransaction()
        {

            var cusID = Request["customerId"];
            var influencerAmount = Convert.ToInt32(Request["amount"]);
            var influencerAccId = Request["influencerAccId"];
            var transactionId = Request["transactionId"];

            int chargeAmount = Convert.ToInt32( 1.15 * influencerAmount) + 320;


            StripeCharge charge = CreateCharge(transactionId ,cusID, chargeAmount);


            StripeTransfer transfer = CreateTransfer(transactionId ,influencerAccId, influencerAmount);


            ViewData["ChargeId"] = charge.Id;
            ViewData["ChargeBalanceTranscationId"] = charge.BalanceTransactionId;

            ViewData["TransferId"] = transfer.Id;
            ViewData["TransferBalanceTranscation"] = transfer.BalanceTransactionId;
            ViewData["PaymentId"] = transfer.DestinationPaymentId;

            return View();
        }

        public StripeCharge CreateCharge(string transactionId ,string cusID,int amount)
        {
            StripeConfiguration.SetApiKey("");
            
            // Charge the Customer instead of the card:
            var chargeOptions = new StripeChargeCreateOptions
            {
                Amount = amount,
                Currency = "usd",
                CustomerId = cusID,
                TransferGroup = transactionId
            };
            var chargeService = new StripeChargeService();
            StripeCharge charge = chargeService.Create(chargeOptions);

            return charge;
        }


        public StripeTransfer CreateTransfer(string transactionId ,string accID, int amount)
        {
            StripeConfiguration.SetApiKey("");

            var transferOptions = new StripeTransferCreateOptions()
            {
                Amount = amount,
                Currency = "usd",
                Destination = accID,
                TransferGroup = transactionId
            };

            var transferService = new StripeTransferService();
            StripeTransfer stripeTransfer = transferService.Create(transferOptions);

            return stripeTransfer;
        }

    }
}
