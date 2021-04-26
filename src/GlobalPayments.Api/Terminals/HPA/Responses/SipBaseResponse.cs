﻿using GlobalPayments.Api.Utils;
using GlobalPayments.Api.Entities;
using System.Collections.Generic;
using System.Linq;
using GlobalPayments.Api.Terminals.Abstractions;
using System;
using System.Text;

namespace GlobalPayments.Api.Terminals.HPA.Responses {
    public class SipBaseResponse : DeviceResponse {
        protected string response;
        protected string currentMessage;

        public string EcrId { get; set; }
        public string RequestId { get; set; }
        public string ResponseId { get; set; }
        public string SipId { get; set; }

        public SipBaseResponse(byte[] buffer, params string[] messageIds) {
            StringBuilder sb = new StringBuilder();
            response = string.Empty;
            foreach (byte b in buffer)
                response += (char)b;

            var messages = response.Split('\r');
            foreach (var message in messages) {
                if (string.IsNullOrEmpty(message)) 
                    continue;

                currentMessage = message;

                var root = ElementTree.Parse(message).Get("SIP");
                Command = root.GetValue<string>("Response");
                if (Command != null && !messageIds.ToList().Contains(Command)) {
                    throw new MessageException("Expected {0} but recieved {1}".FormatWith(string.Join(", ", messageIds), Command));
                }

                Version = root.GetValue<string>("Version");
                EcrId = root.GetValue<string>("ECRId");
                SipId = root.GetValue<string>("SIPId");
                RequestId = root.GetValue<string>("RequestId");
                ResponseId = root.GetValue<string>("ResponseId");
                Status = root.GetValue<string>("MultipleMessage");
                DeviceResponseCode = NormalizeResponse(root.GetValue<string>("Result"));
                DeviceResponseText = root.GetValue<string>("ResultText");

                if ((DeviceResponseCode.Equals("00", StringComparison.OrdinalIgnoreCase)) || (DeviceResponseCode.Equals("2501", StringComparison.OrdinalIgnoreCase))){
                    MapResponse(root);
                }
            }
            FinalizeResponse();
        }

        internal virtual void MapResponse(Element response) { }
        internal virtual void FinalizeResponse() { }

        protected string NormalizeResponse(string response) {
            var acceptedCodes = new List<string> { "0", "85" };
            if (acceptedCodes.Contains(response))
                return "00";
            return response;
        }

        public override string ToString() {
            return response;
        }
    }

    public class SipTerminalResponse : SipBaseResponse, ITerminalResponse {
        /// <summary>
        /// response code returned by the gateway
        /// </summary>
        public string ResponseCode { get; set; }

        /// <summary>
        /// response message returned by the gateway
        /// </summary>
        public string ResponseText { get; set; }

        /// <summary>
        /// the gateway transaction id
        /// </summary>
        public string TransactionId { get; set; }

        /// <summary>
        /// the device's transaction reference number
        /// </summary>
        public string TerminalRefNumber { get; set; }

        /// <summary>
        /// the multi-use payment token generated by the device in instances where tokenization is requested
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// value indicating the presence/non-presence of signature data 
        /// </summary>
        public string SignatureStatus { get; set; }

        /// <summary>
        /// byte array containing the bitmap data for the signature (you may have to call GetSignature depending on your device)
        /// </summary>
        public byte[] SignatureData { get; set; }

        // Transactional
        /// <summary>
        /// the type of transaction (Sale, Authorization, Verify etc...)
        /// </summary>
        public string TransactionType { get; set; }

        /// <summary>
        /// the masked credit card number
        /// </summary>
        public string MaskedCardNumber { get; set; }

        /// <summary>
        /// value denoting whether a card was swiped, inserted, tapped or manually entered
        /// </summary>
        public string EntryMethod { get; set; }

        /// <summary>
        /// the authorization code returned by the issuer
        /// </summary>
        public string AuthorizationCode { get; set; }

        /// <summary>
        /// the approval code issued by the device
        /// </summary>
        public string ApprovalCode { get; set; }

        /// <summary>
        /// the amount of the transaction
        /// </summary>
        public decimal? TransactionAmount { get; set; }

        /// <summary>
        /// the remaining balance in instances of partial approval or gift sales
        /// </summary>
        public decimal? AmountDue { get; set; }

        /// <summary>
        /// the balance of a prepaid or gift card when running a balance inquiry
        /// </summary>
        public decimal? BalanceAmount { get; set; }

        /// <summary>
        /// the card holder name as represented in the track data
        /// </summary>
        public string CardHolderName { get; set; }

        /// <summary>
        /// the BIN range of the card used
        /// </summary>
        public string CardBIN { get; set; }

        /// <summary>
        /// flag indicating whether or not the card was present during the transaction
        /// </summary>
        public bool CardPresent { get; set; }

        /// <summary>
        /// card expiration date
        /// </summary>
        public string ExpirationDate { get; set; }

        /// <summary>
        /// the tip amount applied to the transaction if any
        /// </summary>
        public decimal? TipAmount { get; set; }

        /// <summary>
        /// the cash back amount requested during a debit transaction
        /// </summary>
        public decimal? CashBackAmount { get; set; }

        /// <summary>
        /// Response code from the address verification system
        /// </summary>
        public string AvsResponseCode { get; set; }

        /// <summary>
        /// response text from the address verification system
        /// </summary>
        public string AvsResponseText { get; set; }

        /// <summary>
        /// response code from the CVN/CVV Check.
        /// </summary>
        public string CvvResponseCode { get; set; }

        /// <summary>
        /// response text from the CVN/CVV Check
        /// </summary>
        public string CvvResponseText { get; set; }

        /// <summary>
        /// For level II transactions, value indicating tax exemption status
        /// </summary>
        public bool TaxExempt { get; set; }

        /// <summary>
        /// For level II the business tax exemption ID
        /// </summary>
        public string TaxExemptId { get; set; }

        /// <summary>
        /// The ticket number associated with the transaction
        /// </summary>
        public string TicketNumber { get; set; }

        /// <summary>
        /// The type of payment method used (Credit, Debit, etc...)
        /// </summary>
        public string PaymentType { get; set; }

        // EMV
        /// <summary>
        /// The preferred name of the EMV application selected on the EMV card
        /// </summary>
        public string ApplicationPreferredName { get; set; }

        /// <summary>
        /// The aplication label from the EMV card
        /// </summary>
        public string ApplicationLabel { get; set; }

        /// <summary>
        /// the AID (Application ID) of the selected application on the EMV card
        /// </summary>
        public string ApplicationId { get; set; }

        /// <summary>
        /// The cryptogram type used during the transaction
        /// </summary>
        public ApplicationCryptogramType ApplicationCryptogramType { get; set; }

        /// <summary>
        /// The actual cryptogram value generated for the transaction
        /// </summary>
        public string ApplicationCryptogram { get; set; }

        /// <summary>
        /// The CVM used in the transaction (PIN, Signature, etc...)
        /// </summary>
        public string CardHolderVerificationMethod { get; set; }

        /// <summary>
        /// The results of the terminals attempt to verify the cards authenticity.
        /// </summary>
        public string TerminalVerificationResults { get; set; }

        public decimal? MerchantFee { get; set; }

        internal SipTerminalResponse(byte[] buffer, params string[] messageIds) : base(buffer, messageIds) { }
    }

    public class SipTerminalReport : SipBaseResponse, ITerminalReport {
        internal SipTerminalReport(byte[] buffer, params string[] messageIds) : base(buffer, messageIds) { }
    }
}
