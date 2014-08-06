using System;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

using Composite.Data;

using CompositeC1Contrib.ECommerce.Data.Types;

namespace CompositeC1Contrib.ECommerce
{
    public class QuickpayProvider : PaymentProvider
    {
        const string StatusOk = "000";

        public override string GeneratePaymentWindow(IShopOrder order, Uri currentUri)
        {
            var schemeAndServer = currentUri.GetComponents(UriComponents.SchemeAndServer, UriFormat.Unescaped);

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

            const string protocol = "4";
            const string msgtype = "authorize";
            string merchant = MerchantId;
            string language = cultureInfo.TwoLetterISOLanguageName;
            string ordernumber = order.Id;
            string amount = (order.OrderTotal * 100).ToString("0", CultureInfo.InvariantCulture); //NOTE: Primary store should be changed to DKK, if you do not have internatinal agreement with pbs and quickpay. Otherwise you need to do currency conversion here.
            const string currency = "DKK";
            string continueurl = schemeAndServer + ContinueUrl + "?orderid=" + order.Id;
            string cancelurl = schemeAndServer + CancelUrl;

            // optional parameters
            string callbackurl = schemeAndServer + CallbackUrl;
            string autocapture = String.Empty;
            string autofee = String.Empty;
            string cardtypelock = String.Empty;
            string description = String.Empty;
            string group = String.Empty;
            string testmode = IsTestMode ? "1" : String.Empty;
            string splitpayment = String.Empty;
            // optional end

            string md5Secret = MD5Secret;

            string stringToMd5 = String.Concat(
                protocol, msgtype, merchant, language, ordernumber, amount, currency, continueurl, cancelurl,
                callbackurl, autocapture, autofee, cardtypelock, description, group, testmode, splitpayment,
                md5Secret);

            string md5Check = GetMD5(stringToMd5);

            var data = new NameValueCollection
            {
                {"protocol", protocol},
                {"msgtype", msgtype},
                {"merchant", merchant},
                {"language", language},
                {"ordernumber", ordernumber},
                {"amount", amount},
                {"currency", currency},
                {"continueurl", continueurl},
                {"cancelurl", cancelurl},
                {"callbackurl", callbackurl},
                {"autocapture", autocapture},
                {"autofee", autofee},
                {"cardtypelock", cardtypelock},
                {"description", description},
                {"group", @group},
                {"testmode", testmode},
                {"splitpayment", splitpayment},
                {"md5check", md5Check}
            };

            Utils.WriteLog(order, "Quickpay window generated with the following data " + OrderDataToXml(data));

            return GetFormPost("QuickPay", "https://secure.quickpay.dk/form/", data);
        }

        public override void HandleCallback(NameValueCollection form)
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

            string ordernumber = GetFormString("ordernumber", form);

            using (var data = new DataConnection())
            {
                var order = data.Get<IShopOrder>().Single(f => f.Id == ordernumber);
                if (order == null)
                {
                    Utils.WriteLog(null, "Error, no order with number " + ordernumber);

                    return;
                }

                order.AuthorizationXml = OrderDataToXml(form); ;

                data.Update(order);

                string qpstat = GetFormString("qpstat", form);
                if (qpstat != StatusOk)
                {
                    Utils.WriteLog(order, "Error in status, values is " + qpstat + " but " + StatusOk + " was expected");

                    return;
                }

                string msgtype = GetFormString("msgtype", form);
                string amount = GetFormString("amount", form);
                string currency = GetFormString("currency", form);
                string time = GetFormString("time", form);
                string state = GetFormString("state", form);
                string qpstatmsg = GetFormString("qpstatmsg", form);
                string chstat = GetFormString("chstat", form);
                string chstatmsg = GetFormString("chstatmsg", form);
                string merchant = GetFormString("merchant", form);
                string merchantemail = GetFormString("merchantemail", form);
                string transactionId = GetFormString("transaction", form);
                string cardtype = GetFormString("cardtype", form);
                string cardnumber = GetFormString("cardnumber", form);
                string cardexpire = GetFormString("cardexpire", form);
                string splitpayment = GetFormString("splitpayment", form);
                string fraudprobability = GetFormString("fraudprobability", form);
                string fraudremarks = GetFormString("fraudremarks", form);
                string fraudreport = GetFormString("fraudreport", form);
                string fee = GetFormString("fee", form);
                string md5Check = GetFormString("md5check", form);

                string serverMd5Check = GetMD5(String.Concat(
                    msgtype, ordernumber, amount, currency, time, state, qpstat, qpstatmsg, chstat, chstatmsg,
                    merchant, merchantemail, transactionId, cardtype, cardnumber, cardexpire, splitpayment,
                    fraudprobability, fraudremarks, fraudreport, fee, MD5Secret
                ));

                if (md5Check != serverMd5Check)
                {
                    Utils.WriteLog(order, "Error, MD5 Check doesn't match. This may just be an error in the setting or it COULD be a hacker trying to fake a completed order");

                    return;
                }

                order.AuthorizationTransactionId = transactionId;
                order.PaymentStatus = (int)PaymentStatus.Authorized;

                data.Update(order);

                Utils.WriteLog(order, "Authorized with the following transactionid " + order.AuthorizationTransactionId);
            }
        }

        private static string GetMD5(string inputStr)
        {
            var textBytes = Encoding.Default.GetBytes(inputStr);

            using (var md5 = new MD5CryptoServiceProvider())
            {
                var hash = md5.ComputeHash(textBytes);
                string ret = "";

                foreach (byte a in hash)
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
