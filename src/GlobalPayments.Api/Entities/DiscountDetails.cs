﻿namespace GlobalPayments.Api.Entities {
    public class DiscountDetails {
        public string DiscountName { get; set; }
        public decimal? DiscountAmount { get; set; }
        public decimal? DiscountPercentage { get; set; }
        public string DiscountType { get; set; }
    }
}
