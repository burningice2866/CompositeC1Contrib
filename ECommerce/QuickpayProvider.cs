using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

using Composite.Data;

using CompositeC1Contrib.ECommerce.Data.Types;

namespace CompositeC1Contrib.ECommerce
{
    public class QuickpayProvider : PaymentProvider
    {
        private const string Currency = "DKK";
        private const string Protocol = "4";
        private const string Msgtype = "authorize";
        private const string StatusOk = "000";

        private string _md5Secret;

        protected override string PaymentWindowEndpoint
        {
            get { return "https://secure.quickpay.dk/form/"; }
        }

        public override void Initialize(string name, NameValueCollection config)
        {
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }

            _md5Secret = config["md5Secret"];
            if (String.IsNullOrEmpty(_md5Secret))
            {
                throw new ConfigurationErrorsException("md5Secret");
            }

            config.Remove("md5Secret");

            base.Initialize(name, config);
        }

        public override string GeneratePaymentWindow(IShopOrder order, Uri currentUri)
        {
            /*  Documentation. Request data fields
            Name	Regular expression	Description
            *protocol	/^4$/	Defines the version of the protocol. 4 is the latest
            *msgtype	/^(authorize|subscribe)$/``	Defines wether the transaction should be a standard payment or a subscription payment.
            *merchant	/^[0-9]{8}$/	The QuickPayId
            *language	/^[a-z]{2}$/	The language to use in the HTML pages as 2-letter ISO 639-1 alphabetical code. See http://quickpay.net/features/languages/ for supported languages.
            *ordernumber	/^[a-zA-Z0-9]{4,20}$/	A value by merchant's own choise. Must be unique for each transaction. Usually an incrementing sequence. The value may be reflected in the your bank account list.
            *amount	/^[0-9]{1,9}$/	The transaction amount in its smallest unit. In example, 1 EUR is written 100.
            *currency	/^[A-Z]{3}$/	The transaction currency as the 3-letter ISO 4217 alphabetical code. See http://quickpay.net/features/multi-currency/ for more info
            *continueurl	!^https?://!	QuickPay will redirect to this URL upon a succesful transaction.
            *cancelurl	!^https?://!	QuickPay will redirect to this URL if transaction is cancelled.
            callbackurl	!^https?://!	QuickPay will make a call back to this URL with the result of the transaction. See http://quickpay.net/faq/callbackurl/ for more information.
            autocapture	/^[0-1]{,1}$/	If set to 1, the transaction will be captured automatically. See http://quickpay.net/features/autocapture/ for more information. Note: autocapture is only valid for message type 'authorize'
            autofee	/^[0-1]{,1}$/	If set to 1, the fee charged by the acquirer will be calculated and added to the transaction amount. See http://quickpay.net/features/transaction-fees/ for more information.
            cardtypelock	/^[a-zA-Z,-]{0,}$/	Lock to card type. Multiple card types allowed by comma separation. See http://quickpay.net/features/cardtypelock/ for available values. From V4 cardtypelock=creditcard is default
            description	/^[\w _-.]{,20}$/	A value by the merchant's own choise. Used for identifying a subscription payment. Note: Required for message type 'subscribe'.
            group	/^[0-9]{0,9}$/	Add subscription to this subscription group - (API v4 only)
            testmode	/^[0-1]{,1}$/	Enables inline testing. If set to '1', QuickPay will handle this and only this transaction in test-mode, while QuickPay is in production-mode.
            splitpayment	/^[0-1]{0,1}$/	Enable split payment on transaction - (API v4 only)
            *md5check	/^[a-z0-9]{32}$/	A MD5 checksum to ensure data integrity. See http://quickpay.net/faq/md5check/ for more information.
            */

            var cultureInfo = CultureInfo.CurrentCulture;

            var merchant = MerchantId;
            var language = cultureInfo.TwoLetterISOLanguageName;
            var ordernumber = order.Id;
            var amount = (order.OrderTotal * 100).ToString("0", CultureInfo.InvariantCulture); //NOTE: Primary store should be changed to DKK, if you do not have internatinal agreement with pbs and quickpay. Otherwise you need to do currency conversion here.
            var continueUrl = ParseContinueUrl(order, currentUri);
            var cancelUrl = ParseUrl(CancelUrl, currentUri);

            // optional parameters
            var callbackurl = ParseUrl(CallbackUrl, currentUri);
            var autocapture = String.Empty;
            var autofee = String.Empty;
            var cardtypelock = String.Empty;
            var description = String.Empty;
            var group = String.Empty;
            var testmode = IsTestMode ? "1" : String.Empty;
            var splitpayment = String.Empty;
            // optional end

            var md5Secret = _md5Secret;

            var stringToMd5 = String.Concat(
                Protocol, Msgtype, merchant, language, ordernumber, amount, Currency, continueUrl, cancelUrl,
                callbackurl, autocapture, autofee, cardtypelock, description, group, testmode, splitpayment,
                md5Secret);

            var md5Check = GetMd5(stringToMd5);

            var param = new NameValueCollection
            {
                {"protocol", Protocol},
                {"msgtype", Msgtype},
                {"merchant", merchant},
                {"language", language},
                {"ordernumber", ordernumber},
                {"amount", amount},
                {"currency", Currency},
                {"continueurl", continueUrl},
                {"cancelurl", cancelUrl},
                {"callbackurl", callbackurl},
                {"autocapture", autocapture},
                {"autofee", autofee},
                {"cardtypelock", cardtypelock},
                {"description", description},
                {"group", group},
                {"testmode", testmode},
                {"splitpayment", splitpayment},
                {"md5check", md5Check}
            };

            return GetFormPost(order, param);
        }

        public override async Task<IShopOrder> HandleCallbackAsync(HttpRequestMessage request)
        {
            /*  Documentation.  Response data fields
            msgtype	/^[a-z]$/	Defines which action was performed - Each message type is described in detail later
            ordernumber	/^[a-zA-Z0-9]{4,20}$/	A value specified by merchant in the initial request.
            amount	/^[0-9]{1,10}$/	The amount defined in the request in its smallest unit. In example, 1 EUR is written 100.
            currency	/^[A-Z]{3}$/	The transaction currency as the 3-letter ISO 4217 alphabetical code.
            time	/^[0-9]{12}$/	The time of which the message was handled. Format is YYMMDDHHIISS.
            state	/^[1-9]{1,2}$/	The current state of the transaction. See http://quickpay.net/faq/transaction-states/
            qpstat	/^[0-9]{3}$/	Return code from QuickPay. See http://quickpay.net/faq/status-codes/
            qpstatmsg	/^[\w -.]{1,}$/	A message detailing errors and warnings if any.
            chstat	/^[0-9]{3}$/	Return code from the clearing house. Please refer to the clearing house documentation.
            chstatmsg	/^[\w -.]{1,}$/	A message from the clearing house detailing errors and warnings if any.
            merchant	/^[\w -.]{1,100}$/	The QuickPay merchant name
            merchantemail	/^[\w_-.\@]{6,}$/	The QuickPay merchant email/username
            transaction	/^[0-9]{1,32}$/	The id assigned to the current transaction.
            cardtype	/^[\w-]{1,32}$/	The card type used to authorize the transaction.
            cardnumber	/^[\w\s]{,32}$/	A truncated version of the card number - eg. 'XXXX XXXX XXXX 1234'. Note: This field will be empty for other message types than 'authorize' and 'subscribe'.
            cardexpire	/^[\w\s]{,4}$/	Expire date on the card used in a 'subscribe'. Notation is 'yymm'. Note: This field will be empty for other message types than 'subscribe'.
            splitpayment	/^[0|1]$/	Spitpayment enabled on transaction. See http://quickpay.net/features/split-payment/ for more information. (API v4 only)
            fraudprobability	/^[low|medium|high]?$/	Fraud probability if fraudcheck was performed. (API v4 only)
            fraudremarks	/^.*?$/	Fraud remarks if fraudcheck was performed. (API v4 only)
            fraudreport	/^.*?$/	Fraud report if given. (API v4 only)
            fee	/^[0-9]{,10}$/	Will contain the calculated fee, if autofee was activated in request. See http://quickpay.net/features/transaction-fees/ for more information.
            md5check	/^[a-z0-9]{32}$/	A MD5 checksum to ensure data integrity. See http://quickpay.net/faq/md5check/ for more information.
            *  TESTNUMBERS
            *  I testmode kan man fremprovokere fejlrespons ved, at sende kortoplysninger der indeholder et bogstav, f.eks:

            *  Cart that WILL FAIL
            Korntnr: 4571123412341234, Udløbsdato: 09/12 og cvd: 12a.

            Så bliver kortet afvist, selv om der køres i testmode.

            *  Cart that WILL SUCEED
            En succesrespons kan opnåes ved at bruge f.eks.:

            Kortnr: 4571123412341234, Udløbsdato: 09/12 og cvd: 123.

            *  Possible status codes
            Code 	Description
            000 	Approved.
            001 	Rejected by clearing house. See field 'chstat' and 'chstatmsg' for further explanation.
            002 	Communication error.
            003 	Card expired.
            004 	Transition is not allowed for transaction current state.
            005 	Authorization is expired.
            006 	Error reported by clearing house.
            007 	Error reported by QuickPay.
            008 	Error in request data.
            */

            var form = await request.Content.ReadAsFormDataAsync();

            var ordernumber = GetFormString("ordernumber", form);

            using (var data = new DataConnection())
            {
                var order = data.Get<IShopOrder>().SingleOrDefault(f => f.Id == ordernumber);
                if (order == null)
                {
                    Utils.WriteLog("Error, no order with number " + ordernumber);

                    return null;
                }

                if (order.PaymentStatus == (int)PaymentStatus.Authorized)
                {
                    Utils.WriteLog(order, "debug", "Payment is already authorized");

                    return order;
                }

                var qpstat = GetFormString("qpstat", form);
                if (qpstat != StatusOk)
                {
                    Utils.WriteLog(order, "debug", "Error in status, values is " + qpstat + " but " + StatusOk + " was expected");

                    return order;
                }

                var msgtype = GetFormString("msgtype", form);
                var amount = GetFormString("amount", form);
                var currency = GetFormString("currency", form);
                var time = GetFormString("time", form);
                var state = GetFormString("state", form);
                var qpstatmsg = GetFormString("qpstatmsg", form);
                var chstat = GetFormString("chstat", form);
                var chstatmsg = GetFormString("chstatmsg", form);
                var merchant = GetFormString("merchant", form);
                var merchantemail = GetFormString("merchantemail", form);
                var transactionId = GetFormString("transaction", form);
                var cardtype = GetFormString("cardtype", form);
                var cardnumber = GetFormString("cardnumber", form);
                var cardexpire = GetFormString("cardexpire", form);
                var splitpayment = GetFormString("splitpayment", form);
                var fraudprobability = GetFormString("fraudprobability", form);
                var fraudremarks = GetFormString("fraudremarks", form);
                var fraudreport = GetFormString("fraudreport", form);
                var fee = GetFormString("fee", form);
                var md5Check = GetFormString("md5check", form);

                var serverMd5Check = GetMd5(String.Concat(
                    msgtype, ordernumber, amount, currency, time, state, qpstat, qpstatmsg, chstat, chstatmsg,
                    merchant, merchantemail, transactionId, cardtype, cardnumber, cardexpire, splitpayment,
                    fraudprobability, fraudremarks, fraudreport, fee, _md5Secret
                ));

                if (md5Check != serverMd5Check)
                {
                    Utils.WriteLog(order, "debug", "Error, MD5 Check doesn't match. This may just be an error in the setting or it COULD be a hacker trying to fake a completed order");

                    return order;
                }

                order.AuthorizationXml = OrderDataToXml(form);
                order.AuthorizationTransactionId = transactionId;
                order.PaymentStatus = (int)PaymentStatus.Authorized;

                data.Update(order);

                Utils.WriteLog(order, "authorized");

                return order;
            }
        }

        private static string GetMd5(string inputStr)
        {
            var textBytes = Encoding.Default.GetBytes(inputStr);

            using (var md5 = new MD5CryptoServiceProvider())
            {
                var hash = md5.ComputeHash(textBytes);
                var ret = String.Empty;

                foreach (var a in hash)
                {
                    if (a < 16)
                    {
                        ret += "0" + a.ToString("x");
                    }
                    else
                    {
                        ret += a.ToString("x");
                    }
                }

                return ret;
            }
        }
    }
}
