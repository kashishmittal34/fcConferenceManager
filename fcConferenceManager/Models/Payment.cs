using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic;
using System.Data;
using System.Data.SqlClient;
using nsoftware.InPay;
using System.Web;
using System.Web.UI.WebControls;
using fcConferenceManager;

namespace MAGI_API.Models
{
    public class clsPayment
    {
        public const int METHOD_Credit = 1;
        public const int METHOD_Check = 2;
        public const int METHOD_PO = 3;
        public const int METHOD_Later = 4;
        public const int METHOD_Wire = 5;
        public const int METHOD_Other = 6;
        public const int METHOD_ChooseLater = 7;
        public const int METHOD_Voucher = 8;
        public const int METHOD_Adjustment = 9;
        public const int METHOD_AlreadyPaid = 10;
        public const int METHOD_Cash = 11;
        // --org or account
        public bool bAcctPayment = true;

        // --general items
        public Label lblMsg;
        public SqlConnection sqlConn;

        public int intPayment_pKey = 0;
        public int intPaymentMethod_pKey = METHOD_Credit;
        public double dblAmount = 0;
        public double dblRefundAmount = 0;
        public string strMemo = "";
        public string strComment = "";
        public DateTime dtPaymentDate = DateTime.Today;
        public int intLoggedByAccount_pKey = 0;
        public DateTime dtLoggedOn = DateTime.Today;
        public string strPaymentReference = "";
        public bool bPaid = false;
        public string strIntendedAccounts = "";
        public bool bShowComment = false;
        public string strRegistrationSessionID = "";
        public int intRealPaymentMethod_pKey = 0;
        // --outcome
        public bool bSuccess = false;
        public int intReceiptNumber = 0;
        public int intRefundReceiptNumber = 0;
        // --who is posting?
        public int intPayerAcctPKey = 0;
        public int intEventPKey = 0;
        public string strEventID = "";

        // --credit card info
        public string strCardname = "";
        public string strCardNumber = "";
        public string strCardLastFour = "";
        public int intCardType = 0;
        public string strResponseCardType = "";
        public DateTime dtCardExpiration = DateTime.Today;
        public string strCardCode = "";
        public string strCardFirstname = "";
        public string strCardLastname = "";
        public string strCardZipcode = "";
        public string strCardAddress = "";


        // --card response
        public string strCardFailureReason = "";
        public string strCardErrorCode = "";
        public string strCardErrorText = "";
        public string strCardTransactionID = "";
        public string strRefundCardTransactionID = "";
        public string strCardApprovalCode = "";
        public string strCardReceiptNumber = "";

        // --check
        public string strCheckNum = "";
        public string strCheckName = "";
        public DateTime dtCheckDate = DateTime.Today;
        public DateTime dtCheckExpected = DateTime.Today;

        // --pay later
        public int intPayLaterMaxDaysOut = 0;
        public string strPOCompany = "";
        public string strPONum = "";
        public DateTime dtPODate = DateTime.Today;
        public string strPOItem = "";
        public string strPOInstruct = "";

        // ---purchase order
        public string strLaterReason = "";
        public string strLaterPlan = "";
        public DateTime dtLaterDate = DateTime.Today;

        // --wire
        public string strWireBank = "";
        public string strWireAccount = "";
        public DateTime dtWireDate = DateTime.Today;
        public string strWireComment = "";
        // -- other
        public string strOtherReference = "";

        public string strCustomerId = "0";
        public string strCustomerName = "";
        public string strCustomerFName = "";
        public string strCustomerLName = "";
        public string strCustomerAddress = "";
        public string strCustomerCompany = "";
        public string strCustomerZip = "";

        public string strCustomerEmail = "";
        public string strCustomerCity = "";
        public string strCustomerState = "";
        public string strCustomerCountry = "";
        public string strCustomerPhone = "";
        public string strCustomerFax = "";
        public string strSelectedCharges = "";
        public string strSelectedChargesPkey = "";
        public int intUnpaidCount = 0;
        // --errors
        public List<string> lstErrors = new List<string>();

        public bool LoadPayment()
        {
            bool LoadPayment = false;

            DataTable dt = new DataTable();

            string strTable = (this.bAcctPayment ? "Account_Payments" : "Organization_Payments");
            string strField = (this.bAcctPayment ? "Account_pKey" : "Organization_pKey");

            string qry = "select t1.*, t2.*, t3.*, t4.*, t5.*, t6.*";
            qry = qry + Constants.vbCrLf + " From " + strTable + " t1";
            qry = qry + Constants.vbCrLf + " Left Outer Join Payment_CardInfo t2 on t2.ReceiptNumber = t1.ReceiptNumber";
            qry = qry + Constants.vbCrLf + " Left Outer Join Payment_CheckInfo t3 on t3.ReceiptNumber = t1.ReceiptNumber";
            qry = qry + Constants.vbCrLf + " Left Outer Join Payment_POInfo t4 on t4.ReceiptNumber = t1.ReceiptNumber";
            qry = qry + Constants.vbCrLf + " Left Outer Join Payment_LaterInfo t5 on t5.ReceiptNumber = t1.ReceiptNumber";
            qry = qry + Constants.vbCrLf + " Left Outer Join Payment_WireInfo t6 on t6.ReceiptNumber = t1.ReceiptNumber";
            qry = qry + Constants.vbCrLf + " Where t1.ReceiptNumber = " + this.intReceiptNumber.ToString();

            SqlCommand cmd = new SqlCommand(qry);

            if (clsUtility.GetDataTable(this.sqlConn, cmd,ref dt))
            {
                if (dt.Rows.Count > 0)
                {
                    {
                        var withBlock = dt.Rows[0];
                        if (!Information.IsDBNull(withBlock["pKey"]))
                            this.intPayment_pKey = Convert.ToInt32(withBlock["pKey"]);
                        if (!Information.IsDBNull(withBlock["Event_pKey"]))
                            this.intEventPKey = Convert.ToInt32(withBlock["Event_pKey"]);
                        if (!Information.IsDBNull(withBlock[strField]))
                            this.intPayerAcctPKey = Convert.ToInt32(withBlock[strField]);

                        if (!Information.IsDBNull(withBlock["LoggedByAccount_pKey"]))
                            this.intLoggedByAccount_pKey = Convert.ToInt32(withBlock["LoggedByAccount_pKey"]);
                        if (!Information.IsDBNull(withBlock["LoggedOn"]))
                            this.dtLoggedOn = Convert.ToDateTime(withBlock["LoggedOn"]);

                        if (!Information.IsDBNull(withBlock["PaymentMethod_pkey"]))
                            this.intPaymentMethod_pKey = Convert.ToInt32(withBlock["PaymentMethod_pkey"]);
                        if (!Information.IsDBNull(withBlock["Amount"]))
                            this.dblAmount = Convert.ToDouble(withBlock["Amount"]);
                        if (!Information.IsDBNull(withBlock["RefundAmount"]))
                            this.dblRefundAmount = Convert.ToDouble(withBlock["RefundAmount"]);

                        if (!Information.IsDBNull(withBlock["PaymentDate"]))
                            this.dtPaymentDate = Convert.ToDateTime(withBlock["PaymentDate"]);

                        if (!Information.IsDBNull(withBlock["Paid"]))
                            this.bPaid = Convert.ToBoolean(withBlock["Paid"]);

                        if (this.bAcctPayment)
                            this.strIntendedAccounts = withBlock["IntendedAccounts"].ToString();

                        this.strMemo = withBlock["Memo"].ToString();
                        this.strPaymentReference = withBlock["PaymentReference"].ToString();
                        this.strOtherReference = withBlock["OtherReference"].ToString();

                        this.strCardLastFour = withBlock["CardLastFour"].ToString();
                        this.strCardname = withBlock["CardName"].ToString();
                        this.strCardCode = withBlock["CardCode"].ToString();
                        if (!Information.IsDBNull(withBlock["CardExpiration"]))
                            this.dtCardExpiration = Convert.ToDateTime(withBlock["CardExpiration"]);
                        this.strCardAddress = withBlock["CardAddress"].ToString();
                        this.strCardFirstname = withBlock["CardFirstName"].ToString();
                        this.strCardLastname = withBlock["CardLastName"].ToString();
                        this.strCardZipcode = withBlock["CardZipcode"].ToString();
                        if (!Information.IsDBNull(withBlock["CardTransactionID"]))
                            this.strCardTransactionID = withBlock["CardTransactionID"].ToString();

                        this.strCheckNum = withBlock["CheckNumber"].ToString();
                        this.strCheckName = withBlock["CheckName"].ToString();
                        if (!Information.IsDBNull(withBlock["CheckDate"]))
                            this.dtCheckDate = Convert.ToDateTime(withBlock["CheckDate"]);
                        if (!Information.IsDBNull(withBlock["CheckExpected"]))
                            this.dtCheckExpected = Convert.ToDateTime(withBlock["CheckExpected"]);

                        this.strLaterReason = withBlock["LaterReason"].ToString();
                        this.strLaterPlan = withBlock["LaterPlan"].ToString();
                        if (!Information.IsDBNull(withBlock["LaterDate"]))
                            this.dtLaterDate = Convert.ToDateTime(withBlock["LaterDate"]);

                        this.strWireBank = withBlock["WireBank"].ToString();
                        this.strWireAccount = withBlock["WireAccount"].ToString();
                        if (!Information.IsDBNull(withBlock["WireDate"]))
                            this.dtWireDate = Convert.ToDateTime(withBlock["WireDate"]);
                        this.strWireComment = withBlock["WireComment"].ToString();

                        this.strPOCompany = withBlock["POCompany"].ToString();
                        this.strPONum = withBlock["PONumber"].ToString();
                        if (!Information.IsDBNull(withBlock["PODate"]))
                            this.dtPODate = Convert.ToDateTime(withBlock["PODate"]);
                        this.strPOItem = withBlock["POItem"].ToString();
                        this.strPOInstruct = withBlock["POInstruct"].ToString();
                        this.strPOCompany = withBlock["POCompany"].ToString();

                        LoadPayment = true;
                    }
                }
            }
            else
                clsUtility.LogErrorMessage(this.lblMsg, null/* TODO Change to default(_) if this is not a reference type */, this.GetType().Name, 0, "Error loading Payment information.");

            return LoadPayment;
        }

        public bool PostPayment(bool bConsiderCheckPaid = false, nsoftware.InPay.Icharge iCharge1 = null/* TODO Change to default(_) if this is not a reference type */, nsoftware.InPay.Cardvalidator CardValidator1 = null/* TODO Change to default(_) if this is not a reference type */, bool bGenerateInv = true)
        {
            clsSettings cSettings = (clsSettings)HttpContext.Current.Session["cSettings"];
            bool IsExp = false;
            bool PostPayment = false;

            // --successful?
            this.lstErrors.Clear();

            // --amount?
            this.bPaid = false;
            // --get new receipt number
            if (bGenerateInv)
                this.intReceiptNumber = this.getReceipt();
            if (this.dblAmount <= 0)
                this.lstErrors.Add("Amount must be > 0");
            switch (this.intPaymentMethod_pKey)
            {
                case clsPayment.METHOD_Credit:
                    {
                        if (this.strCardNumber == "")
                            this.lstErrors.Add("Missing card number");
                        if (cSettings.bCreditCard_CVCode & (this.strCardCode.Trim() == ""))
                            this.lstErrors.Add("Missing CV code");
                        if (cSettings.bCreditCard_Singlename & (this.strCardname.Trim() == ""))
                            this.lstErrors.Add("Missing name on card");
                        if (cSettings.bCreditCard_Firstlastname & (this.strCardFirstname.Trim() == ""))
                            this.lstErrors.Add("Missing first name");
                        if (cSettings.bCreditCard_Firstlastname & (this.strCardLastname.Trim() == ""))
                            this.lstErrors.Add("Missing last name");
                        if (cSettings.bCreditCard_Zipcode & (this.strCardZipcode == ""))
                            this.lstErrors.Add("Missing zipcode");
                        if (cSettings.bCreditCard_Address & (this.strCardAddress == ""))
                            this.lstErrors.Add("Missing address");
                        if (this.dtCardExpiration == DateTime.MinValue)
                            this.lstErrors.Add("Expiration date");
                        else if ((this.dtCardExpiration.Year < DateTime.Now.Year | (this.dtCardExpiration.Year == DateTime.Now.Year & this.dtCardExpiration.Month < DateTime.Now.Month)))
                        {
                            this.lstErrors.Add("Expiration date (" + string.Format("{0:MM/yy}", this.dtCardExpiration) + ") cannot be in past");
                            IsExp = true;
                        }

                        if (this.strCardNumber != "" & !IsExp)
                        {
                            if ((cSettings.intCardProcessor_pkey == CreditCardUtility.PROC_None))
                            {
                                if (!CreditCardUtility.IsValidNumber(this.strCardNumber))
                                    this.lstErrors.Add("Invalid card number");
                            }
                            else
                            // --validate credit card via nSoft
                            {
                                var withBlock = CardValidator1;
                                withBlock.CardNumber = this.strCardNumber;
                                withBlock.CardExpMonth = this.dtCardExpiration.Month;
                                withBlock.CardExpYear = this.dtCardExpiration.Year;
                                try
                                {
                                    withBlock.ValidateCard();
                                    // If Not .DigitCheckPassed Then Me.lstErrors.Add("Invalid card number")
                                    if (!withBlock.DigitCheckPassed)
                                        this.lstErrors.Add("Enter credit card number with correct number of digits");
                                    if (!withBlock.DateCheckPassed)
                                        this.lstErrors.Add("Card has expired");
                                }
                                catch (nsoftware.InPay.InPayCardvalidatorException ex)
                                {
                                    this.lstErrors.Add(ex.Message.Replace("Digit check failed", "Invalid card number").Replace("Card is expired", "Card has expired"));
                                }
                            }
                            bool bValidCard = this.lstErrors.Count <= 0;
                            // --try to process the transaction
                            if (bValidCard)
                            {
                                if (!this.PostToCreditCardProcessor(iCharge1))
                                    this.lstErrors.Add(this.strCardFailureReason);
                            }
                        }

                        break;
                    }

                case clsPayment.METHOD_Check:
                    {
                        // If Me.strCheckNum = "" Then Me.lstErrors.Add("Missing Check number")
                        // If Me.strCheckNum = "" Then Me.lstErrors.Add("Enter check number")
                        if (this.strCheckName == "")
                            this.lstErrors.Add("Enter name on checking account");
                        if (this.dtCheckExpected == DateTime.MinValue)
                            this.lstErrors.Add("Enter date (mm/dd/yyyy)");
                        else if (this.dtCheckExpected.Date < DateTime.Now.Date)
                            this.lstErrors.Add("Date we can expect it to arrive cannot be in past");
                        break;
                    }

                case clsPayment.METHOD_PO:
                    {
                        // If Me.strPOCompany = "" Then Me.lstErrors.Add("Enter organization name")
                        // If Me.strPONum = "" Then Me.lstErrors.Add("Enter P.O. number")
                        if (this.dtPODate == DateTime.MinValue)
                        {
                        }
                        else
                        {
                        }

                        break;
                    }

                case clsPayment.METHOD_Later:
                    {
                        if (this.strLaterReason == "")
                            this.lstErrors.Add("Select payment method");
                        if (this.dtLaterDate == DateTime.MinValue)
                            this.lstErrors.Add("Enter date (mm/dd/yyyy)");
                        else
                        {
                            if (this.intPayLaterMaxDaysOut > 0)
                            {
                                DateTime dtTarget = DateTime.Now.AddDays(this.intPayLaterMaxDaysOut);
                                if (this.dtLaterDate > dtTarget)
                                    this.lstErrors.Add("Date must be within " + this.intPayLaterMaxDaysOut.ToString() + " days");
                            }
                            if (this.dtLaterDate.Date < DateTime.Now.Date)
                                this.lstErrors.Add("Date cannot be in past");
                        }

                        break;
                    }

                case clsPayment.METHOD_ChooseLater:
                    {
                        if (this.dtLaterDate == DateTime.MinValue)
                            this.dtLaterDate = DateTime.Now;
                        break;
                    }

                case clsPayment.METHOD_Other:
                    {
                        if (this.strOtherReference == "")
                            this.lstErrors.Add("Missing Voucher reference");
                        break;
                    }

                case clsPayment.METHOD_Voucher:
                    {
                        if (this.strOtherReference == "")
                            this.lstErrors.Add("Missing Voucher reference");
                        break;
                    }

                case clsPayment.METHOD_AlreadyPaid:
                    {
                        if (this.strOtherReference == "")
                            this.lstErrors.Add("Missing comments");
                        break;
                    }
            }
            // --NO
            this.bSuccess = (this.lstErrors.Count <= 0);
            if (!this.bSuccess)
                return PostPayment;

            // --YES
            // '--get new receipt number
            // Me.intReceiptNumber = Me.getReceipt()

            // --mark as paid (if credit card or received check)
            switch (this.intPaymentMethod_pKey)
            {
                case clsPayment.METHOD_Credit:
                    {
                        this.bPaid = true;
                        break;
                    }

                case clsPayment.METHOD_Check:
                case clsPayment.METHOD_Wire:
                case clsPayment.METHOD_Other:
                case clsPayment.METHOD_ChooseLater:
                    {
                        this.bPaid = bConsiderCheckPaid;
                        break;
                    }
            }

            PostPayment = true;

            return PostPayment;
        }

        private void ParseReturnData(string strValues)
        {
            string[] arr = Strings.Split(strValues, "|");
            if (arr.Length > 0)
            {
                for (int intIndex = 0; intIndex <= arr.Length - 1; intIndex++)
                {
                    switch (intIndex)
                    {
                        case 4:
                            {
                                
                                this.strCardApprovalCode = arr[intIndex].Replace(Strings.Chr(34), '\0');
                                break;
                            }

                        case 6:
                            {
                                this.strCardTransactionID = arr[intIndex].Replace(Strings.Chr(34), '\0');
                                break;
                            }

                        case 7:
                            {
                                this.strCardReceiptNumber = arr[intIndex].Replace(Strings.Chr(34), '\0');
                                break;
                            }
                    }
                }
            }
        }

        private void ParseRefundData(string strValues)
        {
            string[] arr = Strings.Split(strValues, "|");
            if (arr.Length > 0)
            {
                for (int intIndex = 0; intIndex <= arr.Length - 1; intIndex++)
                {
                    switch (intIndex)
                    {
                        case 4:
                            {
                                this.strCardApprovalCode = arr[intIndex].Replace(Strings.Chr(34), '\0');
                                break;
                            }

                        case 6:
                            {
                                this.strRefundCardTransactionID = arr[intIndex].Replace(Strings.Chr(34), '\0');
                                break;
                            }

                        case 7:
                            {
                                this.strCardReceiptNumber = arr[intIndex].Replace(Strings.Chr(34), '\0');
                                break;
                            }

                        case 8:
                            {
                                this.strResponseCardType = Convert.ToString(arr[intIndex].Replace(Strings.Chr(34), '\0')).ToUpper();
                                break;
                            }
                    }
                }
            }
        }


        private bool PostToCreditCardProcessor(nsoftware.InPay.Icharge iCharge1)
        {
            bool PostToCreditCardProcessor = false;

        //    // --prepare
        //    this.strCardErrorCode = "";
        //    this.strCardErrorText = "";
        //    this.strCardFailureReason = "";
        //    this.strCardTransactionID = "";
        //    this.strCardApprovalCode = "";
        //    this.strCardReceiptNumber = "";

        //    // --get settings
        //    clsSettings cSettings = (clsSettings)HttpContext.Current.Session["cSettings"];
        //    if ((cSettings.intCardProcessor_pkey == CreditCardUtility.PROC_None) | (cSettings.strMerchantLogin == "") | (cSettings.strMerchantPW == ""))
        //    {
        //        this.strCardFailureReason = "Card validates but the card processor is not properly configured";
        //        return PostToCreditCardProcessor;
        //    }

        //    // --identify provider
        //    switch (cSettings.intCardProcessor_pkey)
        //    {
        //        case CreditCardUtility.PROC_AuthorizeNet:
        //            {
        //                iCharge1.Gateway = IchargeGateways.gwAuthorizeNet;
        //                // iCharge1.GatewayURL = "https://test.authorize.net/gateway/transact.dll"
        //                iCharge1.GatewayURL = cSettings.strGatewayURL;
        //                iCharge1.TestMode = false;
        //                break;
        //            }

        //        case CreditCardUtility.PROC_AuthorizeNetNew:
        //            {
        //                iCharge1.Gateway = IchargeGateways.gwAuthorizeNet;
        //                iCharge1.GatewayURL = cSettings.strGatewayURL;
        //                iCharge1.TestMode = false;
        //                break;
        //            }

        //        case CreditCardUtility.PROC_AuthorizeNetTest:
        //            {
        //                iCharge1.Gateway = IchargeGateways.gwAuthorizeNet;
        //                iCharge1.GatewayURL = cSettings.strGatewayURL;
        //                iCharge1.TestMode = false;
        //                break;
        //            }

        //        case CreditCardUtility.PROC_BluePay:
        //            {
        //                iCharge1.Gateway = IchargeGateways.gwBluePay;
        //                // iCharge1.GatewayURL = "https://test.authorize.net/gateway/transact.dll"
        //                iCharge1.GatewayURL = cSettings.strGatewayURL;
        //                break;
        //            }

        //        case CreditCardUtility.PROC_BluePayTest:
        //            {
        //                iCharge1.Gateway = IchargeGateways.gwBluePay;
        //                // iCharge1.GatewayURL = "https://test.authorize.net/gateway/transact.dll"
        //                iCharge1.GatewayURL = cSettings.strGatewayURL;
        //                iCharge1.TestMode = true;
        //                iCharge1.AddSpecialField("x_test_request", "true");
        //                break;
        //            }

        //        default:
        //            {
        //                this.strCardFailureReason = "Card validates but the card processor is not properly configured";
        //                return PostToCreditCardProcessor;
        //            }
        //    }

        //    // --merchant info
        //    iCharge1.MerchantLogin = cSettings.strMerchantLogin;
        //    iCharge1.MerchantPassword = cSettings.strMerchantPW;
        //    // --account info
        //    iCharge1.Customer.Id = this.strCustomerId;
        //    iCharge1.Customer.FirstName = this.strCustomerFName;
        //    iCharge1.Customer.LastName = this.strCustomerLName;
        //    iCharge1.Customer.FullName = this.strCustomerName;
        //    iCharge1.Customer.Address = this.strCustomerAddress;
        //    iCharge1.Customer.Email = this.strCustomerEmail;
        //    iCharge1.Customer.City = this.strCustomerCity;
        //    iCharge1.Customer.State = this.strCustomerState;
        //    iCharge1.Customer.Country = this.strCustomerCountry;
        //    iCharge1.Customer.Phone = this.strCustomerPhone;
        //    iCharge1.Customer.Zip = this.strCustomerZip;
        //    iCharge1.Customer.Fax = this.strCustomerFax;
        //    // iCharge1.Customer.Company = Me.strCustomerCompany
        //    // iCharge1.SpecialFields.Add(New EPSpecialField("x_customer_ip", "255.123.456.78"))
        //    iCharge1.AddSpecialField("x_company", this.strCustomerCompany);
        //    // iCharge1.AddSpecialField("x_duplicate_window", "180")

        //    // iCharge1.Customer.City = "Beverly Hills"
        //    // iCharge1.Customer.Email = ""
        //    // iCharge1.Customer.Phone = ""

        //    // --card info
        //    iCharge1.Card.Number = this.strCardNumber;
        //    iCharge1.Card.CVVData = this.strCardCode;
        //    iCharge1.Card.ExpMonth = this.dtCardExpiration.Month;
        //    iCharge1.Card.ExpYear = this.dtCardExpiration.Year;
        //    iCharge1.Card.CardType = GetCardType(this.intCardType);
        //    iCharge1.TransactionAmount = this.dblAmount.ToString("f2");
        //    // iCharge1.InvoiceNumber = "MAGICC" + Me.strCardNumber + DateTime.Now.ToString + iCharge1.TransactionAmount.ToString
        //    // iCharge1.InvoiceNumber = "MAGICC" + Me.intReceiptNumber.ToString + DateTime.Now.ToString + iCharge1.TransactionAmount.ToString
        //    string strInvoice = this.strEventID != "" ? this.strEventID : (clsLastUsed)HttpContext.Current.Session["cLastUsed"].strActiveEvent.Replace("-", "");
        //    if (strInvoice.Length > 9)
        //        strInvoice = "MAGICC" + this.intEventPKey.ToString();
        //    iCharge1.InvoiceNumber = strInvoice + string.Format("{0:00000}", Conversion.Val(this.strCustomerId)) + "R" + this.intReceiptNumber.ToString(); // + DateTime.Now.ToString + iCharge1.TransactionAmount.ToString
        //    iCharge1.RequestURL = clsSettings.APP_URL;
        //    // --try to perform transaction
        //    try
        //    {
        //        // --submit transaction
        //        iCharge1.Sale();

        //        // --get the response
        //        EPResponse r = iCharge1.Response;

        //        // --evaluate response codes (vary by provider)
        //        switch (cSettings.intCardProcessor_pkey)
        //        {
        //            case CreditCardUtility.PROC_AuthorizeNet:
        //            case CreditCardUtility.PROC_AuthorizeNetTest:
        //            case CreditCardUtility.PROC_AuthorizeNetNew:
        //                {
        //                    switch (Val(r.Code))
        //                    {
        //                        case 1:
        //                        case 4   // 1=Approved. 4=Held for review. (Approved Is set to 'True', so transaction is assumed to be successful)
        //                       :
        //                            {
        //                                PostToCreditCardProcessor = true;
        //                                this.ParseReturnData(r.Data);
        //                                break;
        //                            }

        //                        case 2   // 2=Declined.
        //                 :
        //                            {
        //                                // Me.strCardFailureReason = "Card declined: " + r.Text
        //                                this.strCardFailureReason = r.Text;
        //                                break;
        //                            }

        //                        case 3   // 3=Error.
        //                 :
        //                            {
        //                                this.strCardErrorText = r.ErrorText; // reason
        //                                this.strCardErrorCode = r.ErrorCode;
        //                                // Me.strCardFailureReason = "Card error: " + r.Text
        //                                this.strCardFailureReason = r.Text;
        //                                break;
        //                            }

        //                        default:
        //                            {
        //                                this.strCardFailureReason = "Transaction failed for unknown reason";
        //                                break;
        //                            }
        //                    }

        //                    break;
        //                }

        //            case CreditCardUtility.PROC_BluePay:
        //            case CreditCardUtility.PROC_BluePayTest:
        //                {
        //                    switch (r.Code)
        //                    {
        //                        case "1"  // 1=Approved
        //                       :
        //                            {
        //                                PostToCreditCardProcessor = true;
        //                                this.ParseReturnData(r.Data);
        //                                break;
        //                            }

        //                        case "0"   // 0=Declined.
        //                 :
        //                            {
        //                                // Me.strCardFailureReason = "Card declined: " + r.Text
        //                                this.strCardFailureReason = r.Text;
        //                                break;
        //                            }

        //                        case "E"  // E=Error.
        //                 :
        //                            {
        //                                this.strCardErrorText = r.ErrorText; // reason
        //                                this.strCardErrorCode = r.ErrorCode;
        //                                // Me.strCardFailureReason = "Card error: " + r.Text
        //                                this.strCardFailureReason = r.Text;
        //                                break;
        //                            }

        //                        default:
        //                            {
        //                                this.strCardFailureReason = "Transaction failed for unknown reason";
        //                                break;
        //                            }
        //                    }

        //                    break;
        //                }
        //        }
        //        if (this.strCardFailureReason != "")
        //        {
        //            string strerrmsg = Interaction.IIf(this.strCardname != "", "card Name: " + this.strCardname, "") + Interaction.IIf(dtCardExpiration != default(DateTime), ", card expiration date: " + dtCardExpiration, "") + Interaction.IIf(strCardLastFour != "", ", card number: " + strCardLastFour, "") + Interaction.IIf(this.dblAmount != 0, ", amount: " + this.dblAmount.ToString("f2"), "") + Interaction.IIf(strCardTransactionID != "", ", transaction number: " + strCardTransactionID, "");
        //            this.LogAuditMessage("Payment Transaction Error: " + this.strCardFailureReason + strerrmsg, clsAudit.LOG_Payment);
        //        }
        //    }
        //    catch (nsoftware.InPay.InPayIchargeException ex)
        //    {
        //        this.strCardFailureReason = "Technical error processing the card: " + ex.Message;
        //        string strerrmsg = Interaction.IIf(this.strCardname != "", "card Name: " + this.strCardname, "") + Interaction.IIf(dtCardExpiration != default(DateTime), ", card expiration date: " + dtCardExpiration, "") + Interaction.IIf(strCardLastFour != "", ", card number: " + strCardLastFour, "") + Interaction.IIf(this.dblAmount != 0, ", amount: " + this.dblAmount.ToString("f2"), "") + Interaction.IIf(strCardTransactionID != "", ", transaction number: " + strCardTransactionID, "");
        //        this.LogAuditMessage("Payment Transaction Error: " + this.strCardFailureReason + strerrmsg, clsAudit.LOG_Payment);
        //    }
            return PostToCreditCardProcessor;
        }

        public int getReceipt()
        {
            int getReceipt = 0;

            SqlCommand sqlCmd = new SqlCommand("getNextReceiptNumber", this.sqlConn);
            sqlCmd.CommandType = CommandType.StoredProcedure;
            sqlCmd.CommandTimeout = 30;
            clsUtility.AddParameter(ref sqlCmd, "@NextNum", SqlDbType.Int, ParameterDirection.Output, 0);
            clsUtility.AddParameter(ref sqlCmd, "@Error", SqlDbType.VarChar, ParameterDirection.Output, "", 1000);
            if (!clsUtility.ExecuteStoredProc(sqlCmd, this.lblMsg, "Error getting next receipt number"))
                return getReceipt;

            getReceipt = Convert.ToInt32(sqlCmd.Parameters["@NextNum"].Value);

            return getReceipt;
        }

        public bool LogVoucherPayment(double dblAmt, int intMeth, int intVoucherPKey, string sMemo, int intMainReceipt = 0, string strVoucherPkey = "", bool bRestartReg = false)
        {
            bool LogVoucherPayment = false;

            //if (bRestartReg)
            //    this.UpdateOldInvoice();

            //string qry = "Insert into Account_Payments(Account_pKey, Event_pkey, PaymentDate, Amount, PaymentMethod_pkey, ReceiptNumber";
            //qry = qry + Constants.vbCrLf + ",LoggedOn, LoggedByAccount_pKey, Memo, Paid, OtherReference,IntendedAccounts,ReceiptReference)";
            //qry = qry + Constants.vbCrLf + "Values(" + this.intPayerAcctPKey.ToString() + "," + this.intEventPKey.ToString() + ",getdate(), " + dblAmt.ToString() + "," + intMeth.ToString() + ",@Receipt";
            //qry = qry + Constants.vbCrLf + ",getdate(), @LoggedBy, @Memo, @Paid, @OtherReference,@IntendedAccounts,@ReceiptReference);";

            //if (strVoucherPkey.Contains(","))
            //{
            //    qry = qry + Constants.vbCrLf + "Update Account_Vouchers";
            //    qry = qry + Constants.vbCrLf + "Set IsUsed = 1, UsedOn=getdate(), UsedOnEvent_pkey = " + this.intEventPKey.ToString() + ",UsedByAccount_pkey=" + this.intPayerAcctPKey.ToString() + ",UsageValue=Amount";
            //    qry = qry + Constants.vbCrLf + "Where pKey in (0" + strVoucherPkey.ToString() + ");";
            //}
            //else
            //{
            //    qry = qry + Constants.vbCrLf + "Update Account_Vouchers";
            //    qry = qry + Constants.vbCrLf + "Set IsUsed = 1, UsedOn=getdate(), UsedOnEvent_pkey = " + this.intEventPKey.ToString() + ",UsedByAccount_pkey=" + this.intPayerAcctPKey.ToString() + ",UsageValue=" + dblAmt.ToString();
            //    qry = qry + Constants.vbCrLf + "Where pKey = " + intVoucherPKey.ToString() + ";";
            //}

            //SqlCommand cmd = new SqlCommand(qry);
            //cmd.Parameters.AddWithValue("@Receipt", this.intReceiptNumber);
            //cmd.Parameters.AddWithValue("@Memo", Strings.Left(sMemo, 150));
            //cmd.Parameters.AddWithValue("@Paid", this.bPaid);
            //cmd.Parameters.AddWithValue("@OtherReference", Interaction.IIf(this.strOtherReference == "", sMemo, this.strOtherReference));
            //cmd.Parameters.AddWithValue("@IntendedAccounts", this.strIntendedAccounts);
            //cmd.Parameters.AddWithValue("@LoggedBy", this.intLoggedByAccount_pKey);
            //cmd.Parameters.AddWithValue("@ReceiptReference", intVoucherPKey > 0 ? intMainReceipt : 0);

            //LogVoucherPayment = clsUtility.ExecuteQuery(cmd, this.lblMsg, "Log Voucher");

            //if (intVoucherPKey > 0 && this.intPayerAcctPKey > 0 && dblAmt > 0)
            //    clsVoucher.SendVoucherUsedEmail(this.strIntendedAccounts.Contains(",") ? this.intPayerAcctPKey : this.strIntendedAccounts, intVoucherPKey, dblAmt, this.intEventPKey, "");
            return LogVoucherPayment;
        }

        public bool UpdateOldInvoice()
        {
            //UpdateOldInvoice = false;
            //string qry = "Update Account_Payments";
            //qry = qry + Constants.vbCrLf + "Set IsDelete = 1";
            //qry = qry + Constants.vbCrLf + "Where isnull(Paid,0)=0 and Event_pKey=@EventPkey and isnull(IsDelete,0)=0 and IntendedAccounts=@IntendedAccounts";

            //SqlCommand cmd = new SqlCommand(qry);

            //cmd.Parameters.AddWithValue("@IntendedAccounts", this.strIntendedAccounts);
            //cmd.Parameters.AddWithValue("@EventPKey", this.intEventPKey.ToString());

            //if (!clsUtility.ExecuteQuery(cmd, this.lblMsg, "Update old invoice due to restart registration"))
            //    return;
            //UpdateOldInvoice = true;
            return false;
        }

        public bool LogPayment(bool bRestartReg = false)
        {
            //LogPayment = false;
            //if (bRestartReg)
            //    this.UpdateOldInvoice();
            //SqlDataAdapter da = new SqlDataAdapter();
            //DataTable dt = new DataTable();

            //string strTable = (this.bAcctPayment ? "Account_Payments" : "Organization_Payments");
            //string strField = (this.bAcctPayment ? "Account_pKey" : "Organization_pKey");

            //string qry = "";
            //switch (this.intPaymentMethod_pKey)
            //{
            //    case clsPayment.METHOD_Credit:
            //        {
            //            qry = qry + Constants.vbCrLf + "Insert into Payment_CardInfo (ReceiptNumber, CardName, CardLastFour, CardExpiration, CardCode, CardAddress, CardFirstName, CardLastName, CardZipcode";
            //            qry = qry + Constants.vbCrLf + ",CardApprovalCode, CardTransactionID, CardReceiptNumber,CardType)";
            //            qry = qry + Constants.vbCrLf + "Values(@Receipt,@CardName, @CardLastFour, @CardExpiration, @CardCode, @CardAddress, @CardFirstName, @CardLastName, @CardZipcode";
            //            qry = qry + Constants.vbCrLf + ",@CardApprovalCode, @CardTransactionID, @CardReceiptNumber,@CardType);";
            //            break;
            //        }

            //    case clsPayment.METHOD_Check:
            //        {
            //            qry = qry + Constants.vbCrLf + "Insert into Payment_CheckInfo (ReceiptNumber,CheckName, CheckNumber, CheckDate, CheckExpected)";
            //            qry = qry + Constants.vbCrLf + "Values(@Receipt,@CheckName, @CheckNumber, @CheckDate, @CheckExpected);";
            //            break;
            //        }

            //    case clsPayment.METHOD_PO:
            //        {
            //            qry = qry + Constants.vbCrLf + "Insert into Payment_POInfo (ReceiptNumber, PONumber, POCompany, PODate, POItem, POInstruct)";
            //            qry = qry + Constants.vbCrLf + "Values(@Receipt,@PONumber, @POCompany, @PODate, @POItem, @POInstruct);";
            //            break;
            //        }

            //    case clsPayment.METHOD_Later:
            //    case clsPayment.METHOD_ChooseLater:
            //        {
            //            qry = qry + Constants.vbCrLf + "Insert into Payment_LaterInfo (ReceiptNumber, LaterReason, LaterDate, LaterPlan)";
            //            qry = qry + Constants.vbCrLf + "Values(@Receipt,@LaterReason, @LaterDate, @LaterPlan);";
            //            break;
            //        }

            //    case clsPayment.METHOD_Wire:
            //        {
            //            qry = qry + Constants.vbCrLf + "Insert into Payment_WireInfo (ReceiptNumber, WireBank, WireDate, WireAccount,WireComment)";
            //            qry = qry + Constants.vbCrLf + "Values(@Receipt,@WireBank, @WireDate, @WireAccount,@WireComment);";
            //            break;
            //        }
            //}
            //qry = qry + Constants.vbCrLf + "Insert into " + strTable + "(" + strField + ", Event_pkey, PaymentDate, Amount, PaymentMethod_pkey, ReceiptNumber";
            //qry = qry + Constants.vbCrLf + ",LoggedOn, LoggedByAccount_pKey, Memo, Paid, OtherReference";
            //if (this.bAcctPayment)
            //    qry = qry + Constants.vbCrLf + ",IntendedAccounts,ShowComment,IntendedAcctChargePkey";
            //if (!this.bAcctPayment)
            //    qry = qry + Constants.vbCrLf + ",IntendedInvoices,PaymentComment";

            //qry = qry + Constants.vbCrLf + ")";
            //qry = qry + Constants.vbCrLf + "Values(" + this.intPayerAcctPKey.ToString() + "," + this.intEventPKey.ToString() + ",getdate(), " + this.dblAmount.ToString() + ",@PaymentMethodPkey,@Receipt";
            //qry = qry + Constants.vbCrLf + ",getdate(), @LoggedBy, @Memo, @Paid, @OtherReference";
            //if (this.bAcctPayment)
            //    qry = qry + Constants.vbCrLf + ",@IntendedAccounts,@ShowComment,@IntendedInvoicesPkey";
            //if (!this.bAcctPayment)
            //    qry = qry + Constants.vbCrLf + ",@IntendedInvoices,@PaymentComment";
            //qry = qry + Constants.vbCrLf + ");";
            //qry = qry + Constants.vbCrLf + "Select @@IDENTITY As pKey;";

            //SqlCommand cmd = new SqlCommand(qry);
            //cmd.CommandType = CommandType.Text;
            //cmd.Connection = sqlConn;

            //switch (this.intPaymentMethod_pKey)
            //{
            //    case clsPayment.METHOD_Credit:
            //        {
            //            cmd.Parameters.AddWithValue("@CardName", this.strCardname);
            //            cmd.Parameters.AddWithValue("@CardLastFour", this.strCardLastFour);
            //            cmd.Parameters.AddWithValue("@CardExpiration", this.dtCardExpiration);
            //            cmd.Parameters.AddWithValue("@CardCode", this.strCardCode);
            //            cmd.Parameters.AddWithValue("@CardFirstName", this.strCardFirstname);
            //            cmd.Parameters.AddWithValue("@CardLastName", this.strCardLastname);
            //            cmd.Parameters.AddWithValue("@CardAddress", this.strCardAddress);
            //            cmd.Parameters.AddWithValue("@CardZipcode", this.strCardZipcode);
            //            cmd.Parameters.AddWithValue("@CardApprovalCode", this.strCardApprovalCode);
            //            cmd.Parameters.AddWithValue("@CardTransactionID", this.strCardTransactionID);
            //            cmd.Parameters.AddWithValue("@CardReceiptNumber", this.strCardReceiptNumber);
            //            cmd.Parameters.AddWithValue("@CardType", this.intCardType);
            //            break;
            //        }

            //    case clsPayment.METHOD_Check:
            //        {
            //            cmd.Parameters.AddWithValue("@CheckName", this.strCheckName);
            //            cmd.Parameters.AddWithValue("@CheckNumber", this.strCheckNum);
            //            cmd.Parameters.AddWithValue("@CheckDate", this.dtCheckDate);
            //            cmd.Parameters.AddWithValue("@CheckExpected", this.dtCheckExpected);
            //            break;
            //        }

            //    case clsPayment.METHOD_PO:
            //        {
            //            cmd.Parameters.AddWithValue("@PONumber", this.strPONum);
            //            cmd.Parameters.AddWithValue("@POCompany", this.strPOCompany);
            //            cmd.Parameters.AddWithValue("@PODate", Interaction.IIf(this.dtPODate == DateTime.MinValue, DBNull.Value, this.dtPODate));
            //            cmd.Parameters.AddWithValue("@POItem", this.strPOItem);
            //            cmd.Parameters.AddWithValue("@POInstruct", this.strPOInstruct);
            //            break;
            //        }

            //    case clsPayment.METHOD_Later:
            //    case clsPayment.METHOD_ChooseLater:
            //        {
            //            cmd.Parameters.AddWithValue("@LaterReason", this.strLaterReason);
            //            cmd.Parameters.AddWithValue("@LaterDate", this.dtLaterDate);
            //            cmd.Parameters.AddWithValue("@LaterPlan", this.strLaterPlan);
            //            break;
            //        }

            //    case clsPayment.METHOD_Wire:
            //        {
            //            cmd.Parameters.AddWithValue("@WireBank", this.strWireBank);
            //            cmd.Parameters.AddWithValue("@WireDate", this.dtWireDate == DateTime.MinValue ? DBNull.Value : this.dtWireDate);
            //            cmd.Parameters.AddWithValue("@WireAccount", this.strWireAccount);
            //            cmd.Parameters.AddWithValue("@WireComment", this.strWireComment);
            //            break;
            //        }
            //}
            //int intPaymentMethod = this.intPaymentMethod_pKey;
            //if (this.intRealPaymentMethod_pKey > 0)
            //    intPaymentMethod = this.intRealPaymentMethod_pKey;
            //cmd.Parameters.AddWithValue("@PaymentMethodPkey", Interaction.IIf(intPaymentMethod == METHOD_Voucher, METHOD_Other, intPaymentMethod).ToString());
            //cmd.Parameters.AddWithValue("@Receipt", this.intReceiptNumber);
            //cmd.Parameters.AddWithValue("@Memo", Strings.Left(this.strMemo, 150));
            //cmd.Parameters.AddWithValue("@Paid", this.bPaid);
            //cmd.Parameters.AddWithValue("@OtherReference", this.strOtherReference);
            //if (this.bAcctPayment)
            //    cmd.Parameters.AddWithValue("@IntendedAccounts", this.strIntendedAccounts);
            //if (this.bAcctPayment)
            //    cmd.Parameters.AddWithValue("@ShowComment", this.bShowComment);
            //cmd.Parameters.AddWithValue("@LoggedBy", this.intLoggedByAccount_pKey);
            //cmd.Parameters.AddWithValue("@IntendedInvoices", this.strSelectedCharges);
            //cmd.Parameters.AddWithValue("@IntendedInvoicesPkey", this.strSelectedChargesPkey);
            //if (!this.bAcctPayment)
            //{
            //    string strPComment = "";
            //    if (this.strSelectedCharges != "")
            //    {
            //        string[] arrInv = this.strSelectedCharges.Split(new char[] { ',' });
            //        if (this.intUnpaidCount > 1)
            //            strPComment = arrInv.Length.ToString() + " of " + this.intUnpaidCount.ToString();
            //    }
            //    cmd.Parameters.AddWithValue("@PaymentComment", strPComment);
            //}
            //try
            //{
            //    da.SelectCommand = cmd;
            //    da.Fill(dt);
            //    if (dt.Rows.Count == 0)
            //    {
            //        clsUtility.LogErrorMessage(this.lblMsg, null/* TODO Change to default(_) if this is not a reference type */, this.GetType().Name, 0, "Error Logging Payment");
            //        return;
            //    }

            //    this.intPayment_pKey = dt.Rows[0][0];
            //    LogPayment = true;
            //}
            //catch (SqlException ex)
            //{
            //    clsUtility.LogErrorMessage(this.lblMsg, null/* TODO Change to default(_) if this is not a reference type */, this.GetType().Name, 0, ex.Message);
            //    return;
            //}
            //finally
            //{
            //    da.Dispose();
            //}
            return false;
        }

        public bool ApplyCashToAccounts(double dblTotalToApply, double dblFixedAmount, string strPaymentType = "")
        {
            //ApplyCashToAccounts = false;
            //// -get list of accounts
            //Array arr = Strings.Split(this.strIntendedAccounts, ",");
            //if (arr.Length <= 0)
            //    return;

            //// --get $ for each
            //double dblAmountRemaining = dblTotalToApply;
            //double dblAmountToApplyForThisAcct = 0;

            //// --post to each account as lomg as the $ holds out
            //int intIndex = 0;
            //while ((intIndex < arr.Length) & (dblAmountRemaining > 0))
            //{
            //    // --account#
            //    intIndex = intIndex + 1;

            //    // --get account
            //    int intAccountPKeyToApply = Conversion.Val(arr(intIndex - 1));

            //    // -get amount for the account
            //    // --if last one, take all remaining cash
            //    // --if fixed then don;t care - use the amount
            //    if (dblFixedAmount > 0)
            //        dblAmountToApplyForThisAcct = dblFixedAmount;
            //    else if (intIndex == arr.Length)
            //        dblAmountToApplyForThisAcct = dblAmountRemaining;
            //    else
            //        dblAmountToApplyForThisAcct = this.getAmountForAccount(intAccountPKeyToApply);

            //    // --need any?
            //    if (dblAmountToApplyForThisAcct > 0)
            //    {
            //        // --do we have enough? otherwise apply as much as you can(i.e. cannot apply more cash than we have)
            //        if (dblAmountToApplyForThisAcct > dblAmountRemaining)
            //            dblAmountToApplyForThisAcct = dblAmountRemaining;
            //        dblAmountRemaining = dblAmountRemaining - dblAmountToApplyForThisAcct;

            //        // --apply the cash to the account
            //        string qry = "Insert into Account_Charges(Account_pKey, Event_pkey, ChargeType_pKey, Amount, LoggedByAccount_pKey, LoggedOn, Memo,Comment, ReceiptReference,PaymentType,IntendedCharges)";
            //        qry = qry + Constants.vbCrLf + "Values(" + intAccountPKeyToApply.ToString() + "," + this.intEventPKey.ToString();
            //        qry = qry + Constants.vbCrLf + "," + clsPrice.CHARGE_Payment.ToString + "," + dblAmountToApplyForThisAcct.ToString();
            //        qry = qry + Constants.vbCrLf + "," + this.intLoggedByAccount_pKey.ToString() + ",getdate()";
            //        qry = qry + Constants.vbCrLf + ",@Memo,@Comment, @Receipt,'" + strPaymentType + "',@IntendedCharges)";

            //        SqlCommand cmd = new SqlCommand(qry);
            //        cmd.Parameters.AddWithValue("@Receipt", this.intReceiptNumber);
            //        cmd.Parameters.AddWithValue("@Memo", Strings.Left(this.strMemo, 150));
            //        cmd.Parameters.AddWithValue("@Comment", this.strComment);
            //        cmd.Parameters.AddWithValue("@IntendedCharges", this.strSelectedCharges);
            //        if (!clsUtility.ExecuteQuery(cmd, this.lblMsg, "Post Payment To Account"))
            //            return;
            //        // If Me.strSelectedChargesPkey <> "" Then
            //        // Me.UpdateAccountChargepkey()
            //        // End If
            //        // update dinner status when paid for dinner'
            //        string[] strDinnerCharge = this.strSelectedCharges.Split(',');
            //        bool bPaidForDinner = (strDinnerCharge.Contains(clsPrice.CHARGE_SpeakerDinnerGuest.ToString) | strDinnerCharge.Contains(clsPrice.CHARGE_SpeakerDinner.ToString));
            //        if (bPaidForDinner)
            //            clsEventAccount.SetDinnerStatus(intAccountPKeyToApply, this.intEventPKey, clsEventAccount.DINNER_AttendingPaid);
            //        // --update attendee status
            //        switch (this.intPaymentMethod_pKey)
            //        {
            //            case clsPayment.METHOD_Check:
            //            case clsPayment.METHOD_Wire:
            //            case clsPayment.METHOD_Later:
            //            case clsPayment.METHOD_Credit:
            //            case clsPayment.METHOD_Other:
            //                {
            //                    double dblBalanceDue = this.getAmountForAccount(intAccountPKeyToApply);
            //                    if (dblBalanceDue <= 0)
            //                    {
            //                        int intCurStatus = clsEventAccount.getAttendeeStatus(intAccountPKeyToApply, this.intEventPKey);
            //                        if (intCurStatus != clsEventAccount.PARTICIPATION_Attending)
            //                            clsEventAccount.setAttendeeStatus(intAccountPKeyToApply, this.intEventPKey);

            //                        clsExam cExam = new clsExam();
            //                        cExam.sqlConn = sqlConn;
            //                        cExam.intExam_PKey = 1;
            //                        cExam.AccountExamRenewal_V1(intAccountPKeyToApply.ToString(), this.intEventPKey.ToString());
            //                    }

            //                    break;
            //                }
            //        }
            //    }
            //}

            //ApplyCashToAccounts = true;
            return false;
        }

        public double getAmountForAccount(int intAcctPKey)
        {
            //DataTable dt = new DataTable();
            //getAmountForAccount = 0;

            //// --get amount to zero out balance due
            //string qry = "Select -1.0*isNull(x.Balance,0) As Amt";
            //qry = qry + Constants.vbCrLf + " From event_accounts t1";
            //qry = qry + Constants.vbCrLf + " Cross Apply dbo.getAccountBalance(t1.Account_pKey, t1.Event_pKey) x";
            //qry = qry + Constants.vbCrLf + " Where t1.Account_pKey =" + intAcctPKey.ToString();
            //qry = qry + Constants.vbCrLf + " and t1.Event_pKey =" + this.intEventPKey.ToString();
            //SqlCommand cmd = new SqlCommand(qry);
            //if (clsUtility.GetDataTable(this.sqlConn, cmd, dt))
            //{
            //    if (dt.Rows.Count > 0)
            //    {
            //        double dblAmt = Conversion.Val(dt.Rows[0]["Amt"].ToString());
            //        getAmountForAccount = dblAmt;
            //    }
            //}
            return 0;
        }

        public bool MarkPaymentAsPaid()
        {
            //MarkPaymentAsPaid = false;

            //string qry = "Update Account_Payments";
            //qry = qry + Constants.vbCrLf + "Set Paid = 1, PaymentDate = @Paydate";
            //// qry = qry + vbCrLf + ",Memo=Memo+' ['+ @Memo +']'"
            //qry = qry + Constants.vbCrLf + ",Memo=@Memo";
            //qry = qry + Constants.vbCrLf + "Where ReceiptNumber =" + this.intReceiptNumber.ToString();

            //SqlCommand cmd = new SqlCommand(qry);
            //cmd.Parameters.AddWithValue("@Paydate", this.dtPaymentDate);
            //cmd.Parameters.AddWithValue("@Memo", Strings.Left(this.strMemo, 150));
            //if (!clsUtility.ExecuteQuery(cmd, this.lblMsg, "Mark Payment as Paid"))
            //    return;

            //this.bPaid = true;

            //MarkPaymentAsPaid = true;
            return false;
        }

        public bool PostToOrg(int intOrgPKeyToApply)
        {
            //PostToOrg = false;

            //string qry = "Insert into Organization_Charges(Organization_pKey, Event_pkey, OrgChargeType_pKey, Amount, LoggedByAccount_pKey, LoggedOn, Memo, ReceiptReference,IntendedCharges)";
            //qry = qry + Constants.vbCrLf + "Values(" + intOrgPKeyToApply.ToString() + "," + this.intEventPKey.ToString();
            //qry = qry + Constants.vbCrLf + "," + clsOrganization.ORGCHARGE_Payment.ToString + "," + this.dblAmount.ToString();
            //qry = qry + Constants.vbCrLf + "," + this.intLoggedByAccount_pKey.ToString() + ",getdate()";
            //qry = qry + Constants.vbCrLf + ",@Memo, @Receipt,@IntendedCharges)";

            //SqlCommand cmd = new SqlCommand(qry);
            //cmd.Parameters.AddWithValue("@Receipt", this.intReceiptNumber);
            //cmd.Parameters.AddWithValue("@Memo", this.strMemo);
            //cmd.Parameters.AddWithValue("@IntendedCharges", this.strSelectedCharges);
            //if (!clsUtility.ExecuteQuery(cmd, this.lblMsg, "Post Payment to Organization"))
            //    return;

            //PostToOrg = true;

            return false;
        }

        public bool MarkOrgPaymentAsPaid()
        {
            //MarkOrgPaymentAsPaid = false;

            //string qry = "Update Organization_Payments";
            //qry = qry + Constants.vbCrLf + "Set Paid = 1, PaymentDate = @Paydate";
            //qry = qry + Constants.vbCrLf + ",Memo=Memo+' ['+ @Memo +']'";
            //qry = qry + Constants.vbCrLf + "Where ReceiptNumber =" + this.intReceiptNumber.ToString();

            //SqlCommand cmd = new SqlCommand(qry);
            //cmd.Parameters.AddWithValue("@Paydate", this.dtPaymentDate);
            //cmd.Parameters.AddWithValue("@Memo", Strings.Left(this.strMemo, 150));
            //if (!clsUtility.ExecuteQuery(cmd, this.lblMsg, "Mark Payment as Paid"))
            //    return;

            //this.bPaid = true;

            //MarkOrgPaymentAsPaid = true;
            return false;
        }

        public bool LogAuditMessage(string strMsg, int intLogType_pKey = 0)
        {
            //LogAuditMessage = clsUtility.LogToAudit(this.sqlConn, this.lblMsg, clsUtility.TYPE_Account, (clsAccount)HttpContext.Current.Session["cAccount"].intAccount_PKey, (clsAccount)HttpContext.Current.Session["cAccount"].intAccount_PKey, strMsg, intLogType_pKey: intLogType_pKey);
            return false;
        }

        public bool UpdateVoucherUsage(double dblAmt, int intVoucherPKey, string sMemo)
        {
            //string qry = "Update Account_Vouchers";
            //qry = qry + Constants.vbCrLf + "Set IsUsed = 1, UsedOn=getdate(), UsedOnEvent_pkey = " + this.intEventPKey.ToString() + ",UsedByAccount_pkey=" + this.intPayerAcctPKey.ToString() + ",UsageValue=" + dblAmt.ToString();
            //qry = qry + Constants.vbCrLf + "Where pKey = " + intVoucherPKey.ToString() + ";";
            //SqlCommand cmd = new SqlCommand(qry);
            //UpdateVoucherUsage = clsUtility.ExecuteQuery(cmd, this.lblMsg, "Log Voucher");
            return false;
        }
        // when pay by po or promise to pay after that pay by cc,check or wire transfer then void previous receipt
        public bool MarkPaymentAsVoid(int intReceiptNumber)
        {
            //MarkPaymentAsVoid = false;
            //string qry = "Update Account_Payments";
            //qry = qry + Constants.vbCrLf + "Set IsDelete = 1";
            //qry = qry + Constants.vbCrLf + "Where ReceiptNumber =" + intReceiptNumber.ToString();
            //SqlCommand cmd = new SqlCommand(qry);
            //if (!clsUtility.ExecuteQuery(cmd, this.lblMsg, "Mark Payment as Void"))
            //    return;
            //MarkPaymentAsVoid = true;
            return false;
        }
        public static bool UpdateAccountPayment(int Account_pKey, int Event_pKey, decimal Amount, bool WithInGroup = false)
        {
            //UpdateAccountPayment = false;
            //// --set up for the stored procedure call
            //SqlConnection sqlConn = new SqlConnection(HttpContext.Current.Session["sqlConn"]);
            //SqlCommand sqlCmd = new SqlCommand("Account_UpdatePayment", sqlConn);
            //sqlCmd.CommandType = CommandType.StoredProcedure;
            //sqlCmd.CommandTimeout = 30;
            //clsUtility.AddParameter(sqlCmd, "@Account_pKey", SqlDbType.Int, ParameterDirection.Input, Account_pKey);
            //clsUtility.AddParameter(sqlCmd, "@Event_pKey", SqlDbType.Int, ParameterDirection.Input, Event_pKey);
            //clsUtility.AddParameter(sqlCmd, "@ChargeAmount", SqlDbType.Decimal, ParameterDirection.Input, Amount);
            //clsUtility.AddParameter(sqlCmd, "@WithInGroup", SqlDbType.Bit, ParameterDirection.Input, WithInGroup);
            //clsUtility.AddParameter(sqlCmd, "@Error", SqlDbType.VarChar, ParameterDirection.Output, "", 1000);
            //if (!clsUtility.ExecuteStoredProc(sqlCmd, null/* TODO Change to default(_) if this is not a reference type */, ""))
            //    return;
            //UpdateAccountPayment = true;
            return false;
        }
        public static bool VoucherReverseCharges(int Account_pKey, int Event_pKey, int LoggedByAcct_pKey)
        {
            // --reverse out the registration and discount charges for the current event
            //VoucherReverseCharges = false;
            //SqlConnection sqlConn = new SqlConnection(HttpContext.Current.Session["sqlConn"]);
            //SqlCommand sqlCmd = new SqlCommand("Voucher_ReverseCharges", sqlConn);
            //sqlCmd.CommandType = CommandType.StoredProcedure;
            //sqlCmd.CommandTimeout = 30;
            //clsUtility.AddParameter(sqlCmd, "@Account_pKey", SqlDbType.Int, ParameterDirection.Input, Account_pKey);
            //clsUtility.AddParameter(sqlCmd, "@Event_pKey", SqlDbType.Int, ParameterDirection.Input, Event_pKey);
            //clsUtility.AddParameter(sqlCmd, "@LoggedByAcct_pKey", SqlDbType.Int, ParameterDirection.Input, LoggedByAcct_pKey);
            //clsUtility.AddParameter(sqlCmd, "@Error", SqlDbType.VarChar, ParameterDirection.Output, "", 1000);
            //if (!clsUtility.ExecuteStoredProc(sqlCmd, null/* TODO Change to default(_) if this is not a reference type */, ""))
            //    return;
            //VoucherReverseCharges = true;
            return false;
        }

        public static bool CancelDinnerReservation(int Account_pKey, int Event_pKey, int LoggedByAcct_pKey, int Event_AcctPKey)
        {
            //CancelDinnerReservation = false;
            //// --set up for the stored procedure call
            //SqlConnection sqlConn = new SqlConnection(HttpContext.Current.Session["sqlConn"]);
            //SqlCommand sqlCmd = new SqlCommand("DinnerCancellation", sqlConn);
            //sqlCmd.CommandType = CommandType.StoredProcedure;
            //sqlCmd.CommandTimeout = 30;
            //clsUtility.AddParameter(sqlCmd, "@Account_pKey", SqlDbType.Int, ParameterDirection.Input, Account_pKey);
            //clsUtility.AddParameter(sqlCmd, "@Event_pKey", SqlDbType.Int, ParameterDirection.Input, Event_pKey);
            //clsUtility.AddParameter(sqlCmd, "@LoggedByAcct_pKey", SqlDbType.Int, ParameterDirection.Input, LoggedByAcct_pKey);
            //clsUtility.AddParameter(sqlCmd, "@EventAcctPKey", SqlDbType.Int, ParameterDirection.Input, Event_AcctPKey);
            //clsUtility.AddParameter(sqlCmd, "@Error", SqlDbType.VarChar, ParameterDirection.Output, "", 1000);
            //if (!clsUtility.ExecuteStoredProc(sqlCmd, null/* TODO Change to default(_) if this is not a reference type */, ""))
            //    return;
            //CancelDinnerReservation = true;
            return false;
        }
        public static string getPendingCharges(int intAcctPKey, int Event_pKey, string strChargesPkey = "")
        {
            //DataTable dt = new DataTable();
            //getPendingCharges = "";
            //string strAcctCharge = string.Empty;
            //SqlConnection sqlConn = new SqlConnection(HttpContext.Current.Session["sqlConn"]);
            //string qry = "DECLARE @Charges VARCHAR(100)";
            //qry = qry + Constants.vbCrLf + "SELECT @Charges = COALESCE(@Charges+', ' ,'') +  ISNULL(IntendedCharges,'0')";
            //qry = qry + Constants.vbCrLf + " from Account_Charges where ChargeType_pKey=13 and Account_pKey =@AccountPKey and Event_pKey=@EventPKey";
            //qry = qry + Constants.vbCrLf + " and isnull(Reversed,0)=0 and isnull(ReversalReference,0)=0 and isnull(IsDelete,0)=0";

            //qry = qry + Constants.vbCrLf + "select t1.ChargeType_pKey,t1.Pkey as AcctChargePkey";
            //qry = qry + Constants.vbCrLf + ",t2.ChargeTypeID+' ($'+cast(iif(t1.ChargeType_pKey=1,(select abs(sum(Amount)) from Account_Charges where Account_pKey=@AccountPKey ";
            //qry = qry + Constants.vbCrLf + "and isNull(Reversed,0) = 0 And isNull(IsDelete,0) = 0 And isNull(ReversalReference,0) = 0 and Event_pKey=@EventPKey and ChargeType_pKey in";
            //qry = qry + Constants.vbCrLf + "(select pKey from SYS_ChargeTypes Where TypeOfCharge = 2 or isnull(IsOtherDiscount,0)=1 or pKey=1)),abs(t1.amount)) as varchar)+')' as ChargeTypeID";
            //qry = qry + Constants.vbCrLf + ",iif(t1.ChargeType_pKey=1,(select abs(sum(Amount)) from Account_Charges where Account_pKey=@AccountPKey";
            //qry = qry + Constants.vbCrLf + "and isNull(Reversed,0) = 0 And isNull(IsDelete,0) = 0 And isNull(ReversalReference,0) = 0 and Event_pKey=@EventPKey and ChargeType_pKey in";
            //qry = qry + Constants.vbCrLf + "(select pKey from SYS_ChargeTypes Where TypeOfCharge = 2 or isnull(IsOtherDiscount,0)=1 or pKey=1)),abs(t1.amount)) as Amount";
            //qry = qry + Constants.vbCrLf + "from Account_Charges t1";
            //qry = qry + Constants.vbCrLf + " inner join Sys_chargetypes t2 on t1.ChargeType_pKey=t2.pKey";
            //qry = qry + Constants.vbCrLf + " where (t1.ChargeType_pKey in (1,2,3,4,5,17,18,19,20) or t2.TypeOfCharge=1) and t1.Event_pKey = @EventPKey";
            //qry = qry + Constants.vbCrLf + " and Account_pKey = @AccountPKey";
            //qry = qry + Constants.vbCrLf + "and t1.ChargeType_pKey not in (select num from dbo.csvtonumbertable(@Charges,','))";
            //qry = qry + Constants.vbCrLf + "and isNull(t1.Reversed,0) = 0 And isNull(t1.IsDelete,0) = 0 And isNull(t1.ReversalReference,0) = 0";

            //SqlCommand cmd = new SqlCommand(qry);
            //cmd.Parameters.AddWithValue("@AccountPKey", intAcctPKey.ToString());
            //cmd.Parameters.AddWithValue("@EventPKey", Event_pKey.ToString());
            //if (clsUtility.GetDataTable(sqlConn, cmd, dt))
            //{
            //    foreach (DataRow dr in dt.Rows)
            //    {
            //        getPendingCharges = Interaction.IIf(getPendingCharges == "", getPendingCharges + dr[0].ToString(), getPendingCharges + "," + dr[0].ToString());
            //        strAcctCharge = Interaction.IIf(strAcctCharge == "", strAcctCharge + dr[1].ToString(), strAcctCharge + "," + dr[1].ToString());
            //    }
            //}
            //getPendingCharges = getPendingCharges.TrimEnd(",").TrimStart(",");
            //strChargesPkey = strAcctCharge.TrimEnd(",").TrimStart(",");
            return "";
        }
        // Public Function AddVoucherReference(strMemo As String, intReceiptNumber As Integer) As Boolean
        // AddVoucherReference = False
        // Dim qry As String = "Update Account_Payments"
        // qry = qry + vbCrLf + "Set OtherReference=@OtherReference"
        // 'qry = qry + vbCrLf + ",Memo=Memo+' ['+ @Memo +']'"
        // 'qry = qry + vbCrLf + ",Memo=@Memo"
        // qry = qry + vbCrLf + "Where ReceiptNumber =" + Me.intReceiptNumber.ToString

        // Dim cmd As New SqlCommand(qry)
        // cmd.Parameters.AddWithValue("@OtherReference", strMemo)
        // 'cmd.Parameters.AddWithValue("@Memo", Left(Me.strMemo, 150))
        // If Not clsUtility.ExecuteQuery(cmd, Me.lblMsg, "") Then Exit Function
        // Me.bPaid = True
        // AddVoucherReference = True
        // End Function
        // Public Shared Function RefundSpeakerCharges(Account_pKey As Integer, Event_pKey As Integer, LoggedByAcct_pKey As Integer, RefundType As Integer) As Boolean
        // RefundSpeakerCharges = False
        // '--set up for the stored procedure call
        // Dim sqlConn As SqlConnection = New SqlConnection(HttpContext.Current.Session("sqlConn"))
        // Dim sqlCmd As New SqlCommand("RefundSpeakerCharges", sqlConn)
        // sqlCmd.CommandType = CommandType.StoredProcedure
        // sqlCmd.CommandTimeout = 30
        // clsUtility.AddParameter(sqlCmd, "@Account_pKey", SqlDbType.Int, ParameterDirection.Input, Account_pKey)
        // clsUtility.AddParameter(sqlCmd, "@Event_pKey", SqlDbType.Int, ParameterDirection.Input, Event_pKey)
        // clsUtility.AddParameter(sqlCmd, "@LoggedBy", SqlDbType.Int, ParameterDirection.Input, LoggedByAcct_pKey)
        // clsUtility.AddParameter(sqlCmd, "@RefundType", SqlDbType.Int, ParameterDirection.Input, RefundType)
        // clsUtility.AddParameter(sqlCmd, "@Error", SqlDbType.VarChar, ParameterDirection.Output, "", 1000)
        // If Not clsUtility.ExecuteStoredProc(sqlCmd, Nothing, "") Then Exit Function
        // RefundSpeakerCharges = True
        // End Function
        public static bool RefundSpeakerCharges(int Account_pKey, int Event_pKey, int LoggedByAcct_pKey, int RefundType, string strComment, string strReference, string strCharges, double dblAmount)
        {
            //RefundSpeakerCharges = false;
            //// --set up for the stored procedure call
            //SqlConnection sqlConn = new SqlConnection(HttpContext.Current.Session["sqlConn"]);
            //SqlCommand sqlCmd = new SqlCommand("SpeakerRefundCharges", sqlConn);
            //sqlCmd.CommandType = CommandType.StoredProcedure;
            //sqlCmd.CommandTimeout = 30;
            //clsUtility.AddParameter(sqlCmd, "@Account_pKey", SqlDbType.Int, ParameterDirection.Input, Account_pKey);
            //clsUtility.AddParameter(sqlCmd, "@Event_pKey", SqlDbType.Int, ParameterDirection.Input, Event_pKey);
            //clsUtility.AddParameter(sqlCmd, "@LoggedBy", SqlDbType.Int, ParameterDirection.Input, LoggedByAcct_pKey);
            //clsUtility.AddParameter(sqlCmd, "@RefundType", SqlDbType.Int, ParameterDirection.Input, RefundType);
            //clsUtility.AddParameter(sqlCmd, "@Comment", SqlDbType.VarChar, ParameterDirection.Input, strComment);
            //clsUtility.AddParameter(sqlCmd, "@Reference", SqlDbType.VarChar, ParameterDirection.Input, strReference);
            //clsUtility.AddParameter(sqlCmd, "@Charges", SqlDbType.VarChar, ParameterDirection.Input, strCharges);
            //clsUtility.AddParameter(sqlCmd, "@Amount", SqlDbType.Decimal, ParameterDirection.Input, dblAmount);
            //clsUtility.AddParameter(sqlCmd, "@Error", SqlDbType.VarChar, ParameterDirection.Output, "", 1000);
            //if (!clsUtility.ExecuteStoredProc(sqlCmd, null/* TODO Change to default(_) if this is not a reference type */, ""))
            //    return;
            //RefundSpeakerCharges = true;
            return false;
        }

        public static DataTable getPaymentMethod(int intReference)
        {
            //SqlConnection sqlConn = new SqlConnection(HttpContext.Current.Session["sqlConn"]);
            //string qry = "Select t1.ReceiptNumber,isnull(t1.PaymentMethod_pkey,0) as PaymentMethod_pkey,isnull(t1.ShowComment,0) as ShowComment";
            //qry = qry + Constants.vbCrLf + ",t2.CardLastFour, t2.CardExpiration, t2.CardFirstName, t2.CardLastName, t2.CardApprovalCode, t2.CardTransactionID";
            //qry = qry + Constants.vbCrLf + ",t3.CheckName, t3.CheckNumber, t3.CheckDate, t3.CheckExpected";
            //qry = qry + Constants.vbCrLf + ",t4.PONumber, t4.POCompany, t4.PODate";
            //qry = qry + Constants.vbCrLf + ",t5.WireBank, t5.WireDate";
            //qry = qry + Constants.vbCrLf + "from Account_Payments t1";
            //qry = qry + Constants.vbCrLf + " Left Outer Join Payment_CardInfo t2 on t2.ReceiptNumber = t1.ReceiptNumber";
            //qry = qry + Constants.vbCrLf + "Left Outer Join Payment_CheckInfo t3 on t3.ReceiptNumber = t1.ReceiptNumber";
            //qry = qry + Constants.vbCrLf + " Left Outer Join Payment_POInfo t4 on t4.ReceiptNumber = t1.ReceiptNumber";
            //qry = qry + Constants.vbCrLf + "Left Outer Join Payment_WireInfo t5 on t5.ReceiptNumber = t1.ReceiptNumber";
            //qry = qry + Constants.vbCrLf + " where t1.ReceiptNumber=" + intReference.ToString();
            //SqlCommand cmd = new SqlCommand(qry);
            //DataTable dt = new DataTable();
            //clsUtility.GetDataTable(sqlConn, cmd, dt);
            //getPaymentMethod = dt;
            return new DataTable();
        }

        public static bool TransferRegistration(int Account_pKey, int TragetAccount_pKey, int Event_pKey, int LoggedByAcct_pKey, string Comment)
        {
            //TransferRegistration = false;
            //// --set up for the stored procedure call
            //SqlConnection sqlConn = new SqlConnection(HttpContext.Current.Session["sqlConn"]);
            //SqlCommand sqlCmd = new SqlCommand("Account_TransferRegistration", sqlConn);
            //sqlCmd.CommandType = CommandType.StoredProcedure;
            //sqlCmd.CommandTimeout = 30;
            //clsUtility.AddParameter(sqlCmd, "@Account_pKey", SqlDbType.Int, ParameterDirection.Input, Account_pKey);
            //clsUtility.AddParameter(sqlCmd, "@TargetAccount_pKey", SqlDbType.Int, ParameterDirection.Input, TragetAccount_pKey);
            //clsUtility.AddParameter(sqlCmd, "@Event_pKey", SqlDbType.Int, ParameterDirection.Input, Event_pKey);
            //clsUtility.AddParameter(sqlCmd, "@LoggedByAcct_pKey", SqlDbType.Int, ParameterDirection.Input, LoggedByAcct_pKey);
            //clsUtility.AddParameter(sqlCmd, "@Comment", SqlDbType.VarChar, ParameterDirection.Input, Comment);
            //clsUtility.AddParameter(sqlCmd, "@Error", SqlDbType.VarChar, ParameterDirection.Output, "", 1000);
            //if (!clsUtility.ExecuteStoredProc(sqlCmd, null/* TODO Change to default(_) if this is not a reference type */, "Error in transfer registration"))
            //    return;
            //TransferRegistration = true;
            return false;
        }

        public static bool CECRCPCancellation(int Account_pKey, int Event_pKey, int LoggedByAcct_pKey, string strCharges, double dblAmount)
        {
            //// --reverse out the registration and discount charges for the current event
            //CECRCPCancellation = false;
            //SqlConnection sqlConn = new SqlConnection(HttpContext.Current.Session["sqlConn"]);
            //SqlCommand sqlCmd = new SqlCommand("CECRCPCancellation", sqlConn);
            //sqlCmd.CommandType = CommandType.StoredProcedure;
            //sqlCmd.CommandTimeout = 30;
            //clsUtility.AddParameter(sqlCmd, "@Account_pKey", SqlDbType.Int, ParameterDirection.Input, Account_pKey);
            //clsUtility.AddParameter(sqlCmd, "@Event_pKey", SqlDbType.Int, ParameterDirection.Input, Event_pKey);
            //clsUtility.AddParameter(sqlCmd, "@Charges", SqlDbType.VarChar, ParameterDirection.Input, strCharges);
            //clsUtility.AddParameter(sqlCmd, "@LoggedByAccount_pKey", SqlDbType.Int, ParameterDirection.Input, LoggedByAcct_pKey);
            //clsUtility.AddParameter(sqlCmd, "@TotalAmount", SqlDbType.Decimal, ParameterDirection.Input, dblAmount);
            //clsUtility.AddParameter(sqlCmd, "@Error", SqlDbType.VarChar, ParameterDirection.Output, "", 1000);
            //if (!clsUtility.ExecuteStoredProc(sqlCmd, null/* TODO Change to default(_) if this is not a reference type */, ""))
            //    return;
            //CECRCPCancellation = true;
            return false;
        }

        public bool LogOrgPaymentNotPaid(string strInvoices)
        {
            //LogOrgPaymentNotPaid = false;
            //SqlDataAdapter da = new SqlDataAdapter();
            //DataTable dt = new DataTable();
            //string[] ArrInvoices = strInvoices.Split(new char[] { ',' });

            //// Use For Each loop over words and display them.
            //string arrInvoice;
            //foreach (var arrInvoice in ArrInvoices)
            //{
            //    string qry = "";
            //    switch (this.intPaymentMethod_pKey)
            //    {
            //        case clsPayment.METHOD_Check:
            //            {
            //                qry = qry + Constants.vbCrLf + "if not exists (select ReceiptNumber from Payment_CheckInfo where ReceiptNumber=@Receipt)";
            //                qry = qry + Constants.vbCrLf + "Insert into Payment_CheckInfo (ReceiptNumber,CheckName, CheckNumber, CheckDate, CheckExpected)";
            //                qry = qry + Constants.vbCrLf + "Values(@Receipt,@CheckName, @CheckNumber, @CheckDate, @CheckExpected);";
            //                break;
            //            }

            //        case clsPayment.METHOD_PO:
            //            {
            //                qry = qry + Constants.vbCrLf + "if not exists (select ReceiptNumber from Payment_POInfo where ReceiptNumber=@Receipt)";
            //                qry = qry + Constants.vbCrLf + "Insert into Payment_POInfo (ReceiptNumber, PONumber, POCompany, PODate, POItem, POInstruct)";
            //                qry = qry + Constants.vbCrLf + "Values(@Receipt,@PONumber, @POCompany, @PODate, @POItem, @POInstruct);";
            //                break;
            //            }

            //        case clsPayment.METHOD_Later:
            //        case clsPayment.METHOD_ChooseLater:
            //            {
            //                qry = qry + Constants.vbCrLf + "if not exists (select ReceiptNumber from Payment_LaterInfo where ReceiptNumber=@Receipt)";
            //                qry = qry + Constants.vbCrLf + "Insert into Payment_LaterInfo (ReceiptNumber, LaterReason, LaterDate, LaterPlan)";
            //                qry = qry + Constants.vbCrLf + "Values(@Receipt,@LaterReason, @LaterDate, @LaterPlan);";
            //                break;
            //            }

            //        case clsPayment.METHOD_Wire:
            //            {
            //                qry = qry + Constants.vbCrLf + "if not exists (select ReceiptNumber from Payment_WireInfo where ReceiptNumber=@Receipt)";
            //                qry = qry + Constants.vbCrLf + "Insert into Payment_WireInfo (ReceiptNumber, WireBank, WireDate, WireAccount,WireComment)";
            //                qry = qry + Constants.vbCrLf + "Values(@Receipt,@WireBank, @WireDate, @WireAccount,@WireComment);";
            //                break;
            //            }
            //    }
            //    qry = qry + Constants.vbCrLf + "Select @@IDENTITY As pKey;";
            //    SqlCommand cmd = new SqlCommand(qry);
            //    cmd.CommandType = CommandType.Text;
            //    cmd.Connection = sqlConn;

            //    switch (this.intPaymentMethod_pKey)
            //    {
            //        case clsPayment.METHOD_Check:
            //            {
            //                cmd.Parameters.AddWithValue("@CheckName", this.strCheckName);
            //                cmd.Parameters.AddWithValue("@CheckNumber", this.strCheckNum);
            //                cmd.Parameters.AddWithValue("@CheckDate", this.dtCheckDate);
            //                cmd.Parameters.AddWithValue("@CheckExpected", this.dtCheckExpected);
            //                break;
            //            }

            //        case clsPayment.METHOD_PO:
            //            {
            //                cmd.Parameters.AddWithValue("@PONumber", this.strPONum);
            //                cmd.Parameters.AddWithValue("@POCompany", this.strPOCompany);
            //                cmd.Parameters.AddWithValue("@PODate", this.dtPODate);
            //                cmd.Parameters.AddWithValue("@POItem", this.strPOItem);
            //                cmd.Parameters.AddWithValue("@POInstruct", this.strPOInstruct);
            //                break;
            //            }

            //        case clsPayment.METHOD_Later:
            //        case clsPayment.METHOD_ChooseLater:
            //            {
            //                cmd.Parameters.AddWithValue("@LaterReason", this.strLaterReason);
            //                cmd.Parameters.AddWithValue("@LaterDate", this.dtLaterDate);
            //                cmd.Parameters.AddWithValue("@LaterPlan", this.strLaterPlan);
            //                break;
            //            }

            //        case clsPayment.METHOD_Wire:
            //            {
            //                cmd.Parameters.AddWithValue("@WireBank", this.strWireBank);
            //                cmd.Parameters.AddWithValue("@WireDate", this.dtWireDate == DateTime.MinValue ? DBNull.Value : this.dtWireDate);
            //                cmd.Parameters.AddWithValue("@WireAccount", this.strWireAccount);
            //                cmd.Parameters.AddWithValue("@WireComment", this.strWireComment);
            //                break;
            //            }
            //    }
            //    int intPaymentMethod = this.intPaymentMethod_pKey;
            //    if (this.intRealPaymentMethod_pKey > 0)
            //        intPaymentMethod = this.intRealPaymentMethod_pKey;
            //    cmd.Parameters.AddWithValue("@PaymentMethodPkey", Interaction.IIf(intPaymentMethod == METHOD_Voucher, METHOD_Other, intPaymentMethod).ToString());
            //    cmd.Parameters.AddWithValue("@Receipt", arrInvoice);
            //    cmd.Parameters.AddWithValue("@Memo", Strings.Left(this.strMemo, 150));
            //    cmd.Parameters.AddWithValue("@Paid", this.bPaid);
            //    cmd.Parameters.AddWithValue("@OtherReference", this.strOtherReference);
            //    if (this.bAcctPayment)
            //        cmd.Parameters.AddWithValue("@IntendedAccounts", this.strIntendedAccounts);
            //    if (this.bAcctPayment)
            //        cmd.Parameters.AddWithValue("@ShowComment", this.bShowComment);
            //    cmd.Parameters.AddWithValue("@LoggedBy", this.intLoggedByAccount_pKey);
            //    cmd.Parameters.AddWithValue("@IntendedInvoices", this.strSelectedCharges);
            //    try
            //    {
            //        da.SelectCommand = cmd;
            //        da.Fill(dt);
            //        if (dt.Rows.Count == 0)
            //        {
            //            clsUtility.LogErrorMessage(this.lblMsg, null/* TODO Change to default(_) if this is not a reference type */, this.GetType().Name, 0, "Error Logging Payment");
            //            return;
            //        }
            //        LogOrgPaymentNotPaid = true;
            //    }
            //    catch (SqlException ex)
            //    {
            //        clsUtility.LogErrorMessage(this.lblMsg, null/* TODO Change to default(_) if this is not a reference type */, this.GetType().Name, 0, ex.Message);
            //        return;
            //    }
            //    finally
            //    {
            //        da.Dispose();
            //    }
            //}
            return false;
        }
        public static bool UpdateCancellationComments(int Account_pKey, int Event_pKey, int intVoucherPkey, int intAccountChargePkey)
        {
            //UpdateCancellationComments = false;
            //SqlConnection sqlConn = new SqlConnection(HttpContext.Current.Session["sqlConn"]);
            //string qry = "update Account_Charges ";
            //qry = qry + Constants.vbCrLf + "SET Comment=@Comments where Event_pKey=@EventPKey and Comment is null";
            //if (intAccountChargePkey > 0)
            //    qry = qry + Constants.vbCrLf + "AND ReversalReference=" + intAccountChargePkey.ToString();
            //else
            //{
            //    qry = qry + Constants.vbCrLf + "AND ChargeType_pKey=13 and isnull(Amount,0)<0";
            //    qry = qry + Constants.vbCrLf + "And pKey=(select top(1) pKey from Account_Charges where Account_pKey=@AccountPKey AND ReversalReference is not null";
            //    qry = qry + Constants.vbCrLf + "ORDER BY pKey DESC)";
            //}

            //SqlCommand cmd = new SqlCommand(qry);

            //cmd.Parameters.AddWithValue("@AccountPKey", Account_pKey.ToString());
            //cmd.Parameters.AddWithValue("@EventPKey", Event_pKey.ToString());
            //cmd.Parameters.AddWithValue("@Comments", "Voucher created (#V" + intVoucherPkey.ToString("D5") + ")");

            //if (!clsUtility.ExecuteQuery(cmd, null/* TODO Change to default(_) if this is not a reference type */, "Update registration cancellation comments"))
            //    return;
            //UpdateCancellationComments = true;

            return false;
        }

        public bool UpdateAccountChargepkey()
        {
            //UpdateAccountChargepkey = false;

            //string qry = "Update Account_Payments";
            //qry = qry + Constants.vbCrLf + "Set IntendedAcctChargePkey =@IntendedAcctChargePkey";
            //qry = qry + Constants.vbCrLf + "Where isnull(IntendedAcctChargePkey,'')='' AND ReceiptNumber =@ReceiptNumber";

            //SqlCommand cmd = new SqlCommand(qry);
            //cmd.Parameters.AddWithValue("@IntendedAcctChargePkey", this.strSelectedChargesPkey);
            //cmd.Parameters.AddWithValue("@ReceiptNumber", this.intReceiptNumber);
            //if (!clsUtility.ExecuteQuery(cmd, this.lblMsg, ""))
            //    return;
            //UpdateAccountChargepkey = true;
            return false;
        }

        public static bool SpeakerRefund(int Account_pKey, int Event_pKey, int LoggedByAcct_pKey, int RefundType, string strComment, string strReference, string strCharges, double dblAmount, bool bCRCP)
        {
            //SpeakerRefund = false;
            //string qry = "";
            //if (dblAmount > 0)
            //{
            //    qry = qry + Constants.vbCrLf + "Declare @CRCPAmount decimal(10,2)=0";
            //    if (bCRCP)
            //    {
            //        qry = qry + Constants.vbCrLf + "SELECT @CRCPAmount=abs(sum(amount))-" + HttpContext.Current.Session["cSettings"].intSpkrCRCPCharge.ToString() + " from  Account_Charges where Account_pKey=@Account_pKey AND Event_pKey=@Event_pKey AND ChargeType_pKey=" + clsPrice.CHARGE_CRCPExam.ToString;
            //        qry = qry + Constants.vbCrLf + "AND ISNULL(Reversed,0)=0 and ISNULL(IsDelete,0)=0";
            //        qry = qry + Constants.vbCrLf + " Update Account_Charges set Memo='CRCP amount adjusted for speaker',Amount=" + (-1 * HttpContext.Current.Session["cSettings"].intSpkrCRCPCharge).ToString();
            //        qry = qry + Constants.vbCrLf + "Where Account_pKey=@Account_pKey and Event_pkey=@Event_pkey AND ChargeType_pKey=" + clsPrice.CHARGE_CRCPExam.ToString;
            //    }
            //    qry = qry + Constants.vbCrLf + "Insert into Account_Charges(Account_pKey, Event_pkey, ChargeType_pKey, Amount, LoggedByAccount_pKey, LoggedOn, Memo,Comment)";
            //    qry = qry + Constants.vbCrLf + "Values(@Account_pKey,@Event_pKey,@SpkrRefundCharge,@Amount,@LoggedBy,GETDATE()";
            //    qry = qry + Constants.vbCrLf + ",@Memo,@Comment)";

            //    qry = qry + Constants.vbCrLf + "Insert into Account_Charges(Account_pKey, Event_pkey, ChargeType_pKey, Amount, LoggedByAccount_pKey, LoggedOn,Memo,Comment)";
            //    qry = qry + Constants.vbCrLf + "Values(@Account_pKey,@Event_pKey,@SpkrRefundCharge,-1*(@Amount+@CRCPAmount),@LoggedBy,GETDATE(),'Refund to speaker',@Comment)";
            //}
            //else
            //{
            //    qry = qry + Constants.vbCrLf + "Update Account_Charges set Reversed=1 where Account_pKey=@Account_pKey AND Event_pkey=@Event_pKey";
            //    qry = qry + Constants.vbCrLf + "AND ChargeType_pKey in (select num from dbo.CSVToNumberTable(@Charges,','))";
            //}
            //SqlCommand cmd = new SqlCommand(qry);
            //clsUtility.AddParameter(cmd, "@Account_pKey", SqlDbType.Int, ParameterDirection.Input, Account_pKey);
            //clsUtility.AddParameter(cmd, "@Event_pKey", SqlDbType.Int, ParameterDirection.Input, Event_pKey);
            //clsUtility.AddParameter(cmd, "@LoggedBy", SqlDbType.Int, ParameterDirection.Input, LoggedByAcct_pKey);
            //clsUtility.AddParameter(cmd, "@Comment", SqlDbType.VarChar, ParameterDirection.Input, strComment);
            //clsUtility.AddParameter(cmd, "@Memo", SqlDbType.VarChar, ParameterDirection.Input, "Set to speaker");
            //clsUtility.AddParameter(cmd, "@Amount", SqlDbType.Decimal, ParameterDirection.Input, dblAmount);
            //clsUtility.AddParameter(cmd, "@Charges", SqlDbType.VarChar, ParameterDirection.Input, strCharges);
            //clsUtility.AddParameter(cmd, "@SpkrRefundCharge", SqlDbType.Int, ParameterDirection.Input, clsPrice.CHARGE_IssueRefundSpeaker);

            //if (!clsUtility.ExecuteQuery(cmd, null/* TODO Change to default(_) if this is not a reference type */, "Speaker refund"))
            //    return;
            //SpeakerRefund = true;
            return false;
        }

        public static string GetOnlyRegistrationCharge()
        {
            //SqlConnection sqlConn = new SqlConnection(HttpContext.Current.Session["sqlConn"]);
            //GetOnlyRegistrationCharge = "";
            //DataTable dt = new DataTable();
            //string qry = "DECLARE @Result VARCHAR(max)";
            //qry = qry + Constants.vbCrLf + "SELECT @Result =COALESCE(@Result+',','')+convert(varchar(5),pKey)";
            //qry = qry + Constants.vbCrLf + "from SYS_ChargeTypes Where TypeOfCharge in (2,3) or isnull(IsOtherDiscount,0)=1 or pKey=1";
            //qry = qry + Constants.vbCrLf + "select @Result as RegCharges";
            //SqlCommand cmd = new SqlCommand(qry);
            //if (clsUtility.GetDataTable(sqlConn, cmd,ref dt))
            //{
            //    if (dt.Rows.Count > 0)
            //        GetOnlyRegistrationCharge = dt.Rows[0]["RegCharges"].ToString();
            //}
            return string.Empty;
        }

        public static DataTable FillAccountChargeTypePkey(string strChargeType, int intAccount_PKey, int intCurEventPKey)
        {
            //SqlConnection sqlConn = new SqlConnection(HttpContext.Current.Session["sqlConn"]);
            //string qry = "SELECT Pkey,isnull(Amount,0) as Amount from Account_Charges where Account_pKey=@Account_pKey AND Event_pKey=@Event_pKey";
            //qry = qry + Constants.vbCrLf + "AND isNull(Reversed,0) = 0 And isNull(IsDelete,0) = 0 AND isNull(ReversalReference,0) = 0";
            //qry = qry + Constants.vbCrLf + "AND ChargeType_pKey IN (SELECT num FROM  dbo.csvtonumbertable(@Charges,','))";
            //SqlCommand cmd = new SqlCommand(qry);
            //DataTable dt = new DataTable();
            //FillAccountChargeTypePkey = dt;
            //cmd.Parameters.AddWithValue("@Charges", strChargeType);
            //cmd.Parameters.AddWithValue("@Account_pKey", intAccount_PKey);
            //cmd.Parameters.AddWithValue("@Event_pKey", intCurEventPKey);
            //if (clsUtility.GetDataTable(sqlConn, cmd, dt))
            //    FillAccountChargeTypePkey = dt;
            return new DataTable();
        }

        public static bool SendPaymentEmail(int intPendingAccountPKey, int intPayMethod, int intReceiptNumber)
        {
            //SendPaymentEmail = false;
            //SqlConnection sqlConn = new SqlConnection(HttpContext.Current.Session["sqlConn"]);
            //clsAnnouncement Ann = clsAnnouncement.getAnnouncementInfo(sqlConn, null/* TODO Change to default(_) if this is not a reference type */, clsAnnouncement.TEXT_InvoiceReceiptEmail);
            //string strContent = Ann.strHTMLText;
            //strContent = Microsoft.VisualBasic.Strings.Replace(strContent, "[ReceiptNumber]", intReceiptNumber.ToString(), 1, -1, Constants.vbTextCompare);
            //var reNum = clsUtility.Encrypt(intReceiptNumber);
            //string rcptLink = "";
            //// rcptLink = rcptLink + " <br/> <br/>"
            //// rcptLink = rcptLink + " <a href=" + clsSettings.APP_URL() + "/frmHome.aspx?DRI=" + reNum + ">Click Here</a>"
            //rcptLink = rcptLink + "<a href=" + clsSettings.APP_URL() + "/frmHome.aspx?DRI=" + reNum + "&PaymentMethod=" + intPayMethod.ToString() + "&dtEarlyBirdDate=" + DateTime.Now.ToString() + Strings.Chr(34) + ">click here</a>";
            //strContent = Microsoft.VisualBasic.Strings.Replace(strContent, "[ReceiptLink]", rcptLink, 1, -1, Constants.vbTextCompare);
            //string Content = strContent;
            //clsEmail cEmail = new clsEmail();
            //cEmail.sqlConn = sqlConn;
            //cEmail.lblMsg = null;
            //cEmail.strSubjectLine = Ann.strTitle;
            //// cEmail.strHTMLContent = strReceipt
            //string strEmail = "";
            //clsAccount cAcc = new clsAccount();
            //{
            //    var withBlock = cAcc;
            //    withBlock.sqlConn = sqlConn;
            //    withBlock.lblMsg = null;
            //    withBlock.intAccount_PKey = intPendingAccountPKey;
            //    withBlock.LoadAccount();
            //    // --replace general terms and logged in user account terms
            //    strContent = cAcc.ReplaceReservedWords(strContent);
            //    strEmail = withBlock.strEmail;
            //    cAcc = null/* TODO Change to default(_) if this is not a reference type */;
            //}
            //cEmail.strHTMLContent = strContent;
            //strContent = Content;
            //cEmail.intAnnouncement_pKey = clsAnnouncement.TEXT_InvoiceReceiptEmail;
            //if (!cEmail.SendEmailToAddress(strEmail, Announcement_pkey: clsAnnouncement.TEXT_InvoiceReceiptEmail))
            //    return;
            //SendPaymentEmail = true;
            return false;
        }

        public bool RefundCreditCardProcessor()
        {
            //RefundCreditCardProcessor = false;

            //// --prepare
            //this.strCardErrorCode = "";
            //this.strCardErrorText = "";
            //this.strCardFailureReason = "";
            //nsoftware.InPay.Icharge iCharge1 = new nsoftware.InPay.Icharge();
            //// --get settings
            //clsSettings cSettings = (clsSettings)HttpContext.Current.Session["cSettings"];
            //if ((cSettings.intCardProcessor_pkey == CreditCardUtility.PROC_None) | (cSettings.strMerchantLogin == "") | (cSettings.strMerchantPW == ""))
            //{
            //    this.strCardFailureReason = "Card validates but the card processor is not properly configured";
            //    return;
            //}
            //// --identify provider  
            //if (ConfigurationManager.AppSettings("QAMode").ToString() == "1")
            //{
            //    iCharge1.Gateway = IchargeGateways.gwAuthorizeNet;
            //    iCharge1.GatewayURL = cSettings.strGatewayURL;
            //    iCharge1.TestMode = false;
            //}
            //else
            //    switch (cSettings.intCardProcessor_pkey)
            //    {
            //        case CreditCardUtility.PROC_AuthorizeNet:
            //            {
            //                iCharge1.Gateway = IchargeGateways.gwAuthorizeNet;
            //                // iCharge1.GatewayURL = "https://test.authorize.net/gateway/transact.dll"
            //                iCharge1.GatewayURL = cSettings.strGatewayURL;
            //                iCharge1.TestMode = false;
            //                break;
            //            }

            //        case CreditCardUtility.PROC_AuthorizeNetNew:
            //            {
            //                iCharge1.Gateway = IchargeGateways.gwAuthorizeNet;
            //                iCharge1.GatewayURL = cSettings.strGatewayURL;
            //                iCharge1.TestMode = false;
            //                break;
            //            }

            //        case CreditCardUtility.PROC_AuthorizeNetTest:
            //            {
            //                iCharge1.Gateway = IchargeGateways.gwAuthorizeNet;
            //                iCharge1.GatewayURL = cSettings.strGatewayURL;
            //                iCharge1.TestMode = false;
            //                break;
            //            }

            //        case CreditCardUtility.PROC_BluePay:
            //            {
            //                iCharge1.Gateway = IchargeGateways.gwBluePay;
            //                // iCharge1.GatewayURL = "https://test.authorize.net/gateway/transact.dll"
            //                iCharge1.GatewayURL = cSettings.strGatewayURL;
            //                break;
            //            }

            //        case CreditCardUtility.PROC_BluePayTest:
            //            {
            //                iCharge1.Gateway = IchargeGateways.gwBluePay;
            //                // iCharge1.GatewayURL = "https://test.authorize.net/gateway/transact.dll"
            //                iCharge1.GatewayURL = cSettings.strGatewayURL;
            //                iCharge1.TestMode = true;
            //                iCharge1.AddSpecialField("x_test_request", "true");
            //                break;
            //            }

            //        default:
            //            {
            //                this.strCardFailureReason = "Card validates but the card processor is not properly configured";
            //                return;
            //            }
            //    }

            //// --merchant info
            //iCharge1.MerchantLogin = cSettings.strMerchantLogin;
            //iCharge1.MerchantPassword = cSettings.strMerchantPW;
            //// --card info
            //iCharge1.Card.Number = this.strCardLastFour;
            //iCharge1.TransactionAmount = this.dblAmount.ToString("f2");
            //iCharge1.TransactionId = this.strRefundCardTransactionID;
            //iCharge1.RequestURL = clsSettings.APP_URL;
            //iCharge1.Card.ExpMonth = this.dtCardExpiration.Month;
            //iCharge1.Card.ExpYear = this.dtCardExpiration.Year;
            //// --try to perform transaction
            //try
            //{
            //    // --submit transaction
            //    iCharge1.Refund();

            //    EPResponse r = iCharge1.Response;

            //    // --evaluate response codes (vary by provider)
            //    switch (cSettings.intCardProcessor_pkey)
            //    {
            //        case CreditCardUtility.PROC_AuthorizeNet:
            //        case CreditCardUtility.PROC_AuthorizeNetTest:
            //        case CreditCardUtility.PROC_AuthorizeNetNew:
            //            {
            //                switch (Val(r.Code))
            //                {
            //                    case 1:
            //                    case 4   // 1=Approved. 4=Held for review. (Approved Is set to 'True', so transaction is assumed to be successful)
            //                   :
            //                        {
            //                            RefundCreditCardProcessor = true;
            //                            this.ParseRefundData(r.Data);
            //                            break;
            //                        }

            //                    case 2   // 2=Declined.
            //             :
            //                        {
            //                            // Me.strCardFailureReason = "Card declined: " + r.Text
            //                            this.strCardFailureReason = r.Text;
            //                            break;
            //                        }

            //                    case 3   // 3=Error.
            //             :
            //                        {
            //                            this.strCardErrorText = r.ErrorText; // reason
            //                            this.strCardErrorCode = r.ErrorCode;
            //                            // Me.strCardFailureReason = "Card error: " + r.Text
            //                            this.strCardFailureReason = r.Text;
            //                            break;
            //                        }

            //                    default:
            //                        {
            //                            this.strCardFailureReason = "Transaction failed for unknown reason";
            //                            break;
            //                        }
            //                }

            //                break;
            //            }

            //        case CreditCardUtility.PROC_BluePay:
            //        case CreditCardUtility.PROC_BluePayTest:
            //            {
            //                switch (r.Code)
            //                {
            //                    case "1"  // 1=Approved
            //                   :
            //                        {
            //                            RefundCreditCardProcessor = true;
            //                            this.ParseRefundData(r.Data);
            //                            break;
            //                        }

            //                    case "0"   // 0=Declined.
            //             :
            //                        {
            //                            // Me.strCardFailureReason = "Card declined: " + r.Text
            //                            this.strCardFailureReason = r.Text;
            //                            break;
            //                        }

            //                    case "E"  // E=Error.
            //             :
            //                        {
            //                            this.strCardErrorText = r.ErrorText; // reason
            //                            this.strCardErrorCode = r.ErrorCode;
            //                            // Me.strCardFailureReason = "Card error: " + r.Text
            //                            this.strCardFailureReason = r.Text;
            //                            break;
            //                        }

            //                    default:
            //                        {
            //                            this.strCardFailureReason = "Transaction failed for unknown reason";
            //                            break;
            //                        }
            //                }

            //                break;
            //            }
            //    }
            //    if (this.strCardFailureReason != "")
            //    {
            //        string strerrmsg = Interaction.IIf(strCardLastFour != "", "card number: " + strCardLastFour, "") + Interaction.IIf(this.dblAmount != 0, ", amount: " + this.dblAmount.ToString("f2"), "") + Interaction.IIf(strCardTransactionID != "", ", transaction number: " + strCardTransactionID, "");
            //        this.LogAuditMessage("Payment Transaction Error: " + this.strCardFailureReason + strerrmsg, clsAudit.LOG_Payment);
            //    }
            //}
            //catch (nsoftware.InPay.InPayIchargeException ex)
            //{
            //    this.strCardFailureReason = "Technical error processing the card: " + ex.Message;
            //    string strerrmsg = Interaction.IIf(strCardLastFour != "", "card number: " + strCardLastFour, "") + Interaction.IIf(this.dblAmount != 0, ", amount: " + this.dblAmount.ToString("f2"), "") + Interaction.IIf(strCardTransactionID != "", ", transaction number: " + strCardTransactionID, "");
            //    this.LogAuditMessage("Payment Transaction Error: " + this.strCardFailureReason + strerrmsg, clsAudit.LOG_Payment);
            //}
            return false;
        }

        public static AuthorizeNet.CardType GetCardType(int cardType)
        {
            //if (cardType == 1)
            //    return AuthorizeNet.CardType.VISA;
            //else if (cardType == 2)
            //    return AuthorizeNet.CardType.AMEX;
            //else if (cardType == 3)
            //    return AuthorizeNet.CardType.MASTER;
            //else if (cardType == 4)
            //    return AuthorizeNet.CardType.DISCOVER;
            //else
            //    return AuthorizeNet.CardType.NONE;
            return AuthorizeNet.CardType.NONE;
        }

        public static bool InsertCanReq(int intAccountPKey, int intType, Label lblMsg, int intEventPkey)
        {
            //InsertCanReq = false;
            //string qry = "update Event_Accounts";
            //qry = qry + Constants.vbCrLf + "Set RegCancelRequestDate=GETDATE(),intRegCancelRequest =" + intType.ToString();
            //qry = qry + Constants.vbCrLf + ",RegCancelReqBy =" + intAccountPKey.ToString();
            //qry = qry + Constants.vbCrLf + "where Account_pKey=" + intAccountPKey.ToString() + " and Event_pKey=" + intEventPkey.ToString();

            //SqlCommand cmd = new SqlCommand(qry);
            //if (!clsUtility.ExecuteQuery(cmd, lblMsg, "Registration cancellation request from homepage."))
            //    return;
            //InsertCanReq = true;
            return false;
        }

        public static bool SendRegCancellationEmail(int intPendingAccountPKey, int intRequestType, int dblAmount, int intReceiptNumber = 0)
        {
            //SendRegCancellationEmail = false;
            //SqlConnection sqlConn = new SqlConnection(HttpContext.Current.Session["sqlConn"]);
            //clsAnnouncement Ann = clsAnnouncement.getAnnouncementInfo(sqlConn, null/* TODO Change to default(_) if this is not a reference type */, clsAnnouncement.Registration_Cancellation);
            //string strContent = Ann.strHTMLText;
            //string strSubject = Ann.strTitle;
            //strContent = Microsoft.VisualBasic.Strings.Replace(strContent, "[Voucher_Amount]", dblAmount > 0 ? string.Format("{0:c}", dblAmount) : "", 1, -1, Constants.vbTextCompare);
            //strContent = Microsoft.VisualBasic.Strings.Replace(strContent, "[Registration_CancellationRequest]", dblAmount > 0 ? intRequestType == clsEventAccount.RegCancelReq_Refund ? "Refund" : "Voucher" : "", 1, -1, Constants.vbTextCompare);
            //string rcptLink = "";
            //if (intReceiptNumber > 0)
            //    rcptLink = "To obtain a receipt, <a href=" + clsSettings.APP_URL() + "/frmHome.aspx?RefundRCPT=" + clsUtility.Encrypt(intReceiptNumber.ToString()) + ">click here</a>";
            //strContent = Microsoft.VisualBasic.Strings.Replace(strContent, "[VoucherRefund_Receipt]", rcptLink, 1, -1, Constants.vbTextCompare);
            //clsEmail cEmail = new clsEmail();
            //cEmail.sqlConn = sqlConn;
            //cEmail.lblMsg = null;
            //// cEmail.strHTMLContent = strReceipt
            //string strEmail = "";
            //clsAccount cAcc = new clsAccount();
            //{
            //    var withBlock = cAcc;
            //    withBlock.sqlConn = sqlConn;
            //    withBlock.lblMsg = null;
            //    withBlock.intAccount_PKey = intPendingAccountPKey;
            //    withBlock.LoadAccount();
            //    // --replace general terms and logged in user account terms
            //    strContent = cAcc.ReplaceReservedWords(strContent);
            //    strSubject = cAcc.ReplaceReservedWords(strSubject);
            //    strEmail = withBlock.strEmail;
            //}
            //clsEvent cEvent = new clsEvent();
            //{
            //    var withBlock = cEvent;
            //    withBlock.sqlConn = sqlConn;
            //    withBlock.lblMsg = null;
            //    withBlock.intEvent_PKey = (clsLastUsed)HttpContext.Current.Session["cLastUsed"].intActiveEventPkey;
            //    withBlock.LoadEvent();
            //    // --replace general terms and logged in user account terms
            //    strContent = cEvent.ReplaceReservedWords(strContent);
            //    strSubject = cEvent.ReplaceReservedWords(strSubject);
            //}
            //clsVenue cVenue = new clsVenue();
            //{
            //    var withBlock = cVenue;
            //    withBlock.sqlConn = sqlConn;
            //    withBlock.lblMsg = null;
            //    withBlock.intVenue_PKey = cEvent.intVenue_PKey;
            //    withBlock.LoadVenue();
            //    strContent = cVenue.ReplaceReservedWords(strContent);
            //    strSubject = cVenue.ReplaceReservedWords(strSubject);
            //}
            //cVenue = null/* TODO Change to default(_) if this is not a reference type */;
            //cEvent = null/* TODO Change to default(_) if this is not a reference type */;
            //cAcc = null/* TODO Change to default(_) if this is not a reference type */;
            //strContent = clsSettings.ReplaceTermsGeneral(strContent);
            //cEmail.strHTMLContent = strContent;
            //cEmail.strSubjectLine = strSubject;
            //cEmail.intAnnouncement_pKey = clsAnnouncement.Registration_Cancellation;
            //if (!cEmail.SendEmailToAddress(strEmail, Announcement_pkey: cEmail.intAnnouncement_pKey))
            //    return;
            //SendRegCancellationEmail = true;
            return false;
        }
        // send cancellation email from free pass and provisional
        public static bool SendRegCancellationEmailFreePass(int intPendingAccountPKey)
        {
            //SendRegCancellationEmailFreePass = false;
            //SqlConnection sqlConn = new SqlConnection(HttpContext.Current.Session["sqlConn"]);
            //clsAnnouncement Ann = clsAnnouncement.getAnnouncementInfo(sqlConn, null/* TODO Change to default(_) if this is not a reference type */, clsAnnouncement.Registration_CancellationFreePass);
            //string strContent = Ann.strHTMLText;
            //string strSubject = Ann.strTitle;
            //clsEmail cEmail = new clsEmail();
            //cEmail.sqlConn = sqlConn;
            //cEmail.lblMsg = null;
            //// cEmail.strHTMLContent = strReceipt
            //string strEmail = "";
            //clsAccount cAcc = new clsAccount();
            //{
            //    var withBlock = cAcc;
            //    withBlock.sqlConn = sqlConn;
            //    withBlock.lblMsg = null;
            //    withBlock.intAccount_PKey = intPendingAccountPKey;
            //    withBlock.LoadAccount();
            //    // --replace general terms and logged in user account terms
            //    strContent = cAcc.ReplaceReservedWords(strContent);
            //    strSubject = cAcc.ReplaceReservedWords(strSubject);
            //    strEmail = withBlock.strEmail;
            //}
            //clsEvent cEvent = new clsEvent();
            //{
            //    var withBlock = cEvent;
            //    withBlock.sqlConn = sqlConn;
            //    withBlock.lblMsg = null;
            //    withBlock.intEvent_PKey = (clsLastUsed)HttpContext.Current.Session["cLastUsed"].intActiveEventPkey;
            //    withBlock.LoadEvent();
            //    // --replace general terms and logged in user account terms
            //    strContent = cEvent.ReplaceReservedWords(strContent);
            //    strSubject = cEvent.ReplaceReservedWords(strSubject);
            //}
            //clsVenue cVenue = new clsVenue();
            //{
            //    var withBlock = cVenue;
            //    withBlock.sqlConn = sqlConn;
            //    withBlock.lblMsg = null;
            //    withBlock.intVenue_PKey = cEvent.intVenue_PKey;
            //    withBlock.LoadVenue();
            //    strContent = cVenue.ReplaceReservedWords(strContent);
            //    strSubject = cVenue.ReplaceReservedWords(strSubject);
            //}
            //cVenue = null/* TODO Change to default(_) if this is not a reference type */;
            //cEvent = null/* TODO Change to default(_) if this is not a reference type */;
            //cAcc = null/* TODO Change to default(_) if this is not a reference type */;
            //strContent = clsSettings.ReplaceTermsGeneral(strContent);
            //cEmail.strHTMLContent = strContent;
            //cEmail.strSubjectLine = strSubject;
            //cEmail.intAnnouncement_pKey = clsAnnouncement.Registration_CancellationFreePass;

            //if (!cEmail.SendEmailToAddress(strEmail, Announcement_pkey: cEmail.intAnnouncement_pKey))
            //    return;
            //SendRegCancellationEmailFreePass = true;
            return false;
        }

        // transfer registration to different event
        public static bool TransferRegistrationEvent(int Account_pKey, int TragetAccount_pKey, int SourceEvent_pKey, int TargetEvent_pKey_pKey, int LoggedByAcct_pKey, string Comment)
        {
            //TransferRegistrationEvent = false;
            //// --set up for the stored procedure call
            //SqlConnection sqlConn = new SqlConnection(HttpContext.Current.Session["sqlConn"]);
            //SqlCommand sqlCmd = new SqlCommand("Account_TransferRegistrationEvent", sqlConn);
            //sqlCmd.CommandType = CommandType.StoredProcedure;
            //sqlCmd.CommandTimeout = 30;
            //clsUtility.AddParameter(sqlCmd, "@Account_pKey", SqlDbType.Int, ParameterDirection.Input, Account_pKey);
            //clsUtility.AddParameter(sqlCmd, "@TargetAccount_pKey", SqlDbType.Int, ParameterDirection.Input, TragetAccount_pKey);
            //clsUtility.AddParameter(sqlCmd, "@SourceEvent_pKey", SqlDbType.Int, ParameterDirection.Input, SourceEvent_pKey);
            //clsUtility.AddParameter(sqlCmd, "@TargetEvent_pKey", SqlDbType.Int, ParameterDirection.Input, TargetEvent_pKey_pKey);
            //clsUtility.AddParameter(sqlCmd, "@LoggedByAcct_pKey", SqlDbType.Int, ParameterDirection.Input, LoggedByAcct_pKey);
            //clsUtility.AddParameter(sqlCmd, "@Comment", SqlDbType.VarChar, ParameterDirection.Input, Comment);
            //clsUtility.AddParameter(sqlCmd, "@Error", SqlDbType.VarChar, ParameterDirection.Output, "", 1000);
            //if (!clsUtility.ExecuteStoredProc(sqlCmd, null/* TODO Change to default(_) if this is not a reference type */, "Error in transfer event registration"))
            //    return;
            //TransferRegistrationEvent = true;
            return false;
        }
    }
}