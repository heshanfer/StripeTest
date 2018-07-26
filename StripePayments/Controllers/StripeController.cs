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

            var CLIENT_ID = "";
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

                // user id -> db

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
            
            // customer.Id -> db

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
        /// 




        public ActionResult ProceedTransaction()
        {

            var advertiserId = Request["customerId"];
            var totalAmount = Convert.ToInt32(Request["amount"]);    // should be in Cents   eg - $10 means 1000
            var influencerAccId = Request["influencerAccId"];
            //var transactionId = Request["transactionId"];

            int influencerAmount = Convert.ToInt32(Math.Round((0.85 * totalAmount), 2, MidpointRounding.ToEven));  // Round up if there any fraction
            int stripeFee = Convert.ToInt32(Math.Round((totalAmount * 0.0175 + 30), 2, MidpointRounding.ToEven));  // Stripe Charge for Austraila  1.75% + 30 Cents
            int platformCharge = totalAmount - influencerAmount - stripeFee;

            StripeCharge charge = CreateTransaction( advertiserId, totalAmount, platformCharge, influencerAccId);

            ViewData["ChargeId"] = charge.Id;
            ViewData["ChargeBalanceTranscationId"] = charge.BalanceTransactionId;

            return View();
        }



        public StripeCharge CreateTransaction(string advertiserId , int totalAmount , int platformCharge , string influencerAccId)
        {
            StripeConfiguration.SetApiKey("");

            // direct charge from advertiser & deposit in influencer account
            // https://stripe.com/docs/connect/direct-charges
            // https://gist.github.com/jaymedavis/d969be13ca6019686a4e72b82e999546


            // Charge the Customer instead of the card
            var chargeOptions = new StripeChargeCreateOptions
            {
                Amount = totalAmount,
                Currency = "usd",
                CustomerId = advertiserId,
                ApplicationFee = platformCharge
            };

            var requestOptions = new StripeRequestOptions();
            requestOptions.StripeConnectAccountId = influencerAccId;

            var chargeService = new StripeChargeService();
            StripeCharge charge = chargeService.Create(chargeOptions,requestOptions);

            return charge;
        }









            /*     public ActionResult ProceedTransaction()
                 {

                     var cusID = Request["customerId"];
                     var influencerAmount = Convert.ToInt32(Request["amount"]);
                     var influencerAccId = Request["influencerAccId"];
                     var transactionId = Request["transactionId"];

                     int chargeAmount = Convert.ToInt32( (1.15 * influencerAmount* 100)*1.029 ) + 30;


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

             */






        }
}
