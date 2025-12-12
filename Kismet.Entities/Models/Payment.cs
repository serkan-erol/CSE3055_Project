using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kismet.Entities.Models {

    [Table("Payment")]
    public class Payment
    {
        [Key]
        [Column("PaymentID")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PaymentID { get; set; }

        [Column("BillingID")]
        [ForeignKey("Billing")]
        public int BillingID { get; set; }

        // Navigation property for the 1-to-M relationship with Billing
        public Billing billing { get; set; } = null!;

        [Column("FTransactionID")]
        [ForeignKey("FinancialTransaction")]
        public int FTransactionID { get; set; }

        // Navigation property for the 1-to-M relationship with FinancialTransaction
        public FinancialTransaction FinancialTransaction { get; set; } = null!;

        [Required]
        [Column("PaymentAmount")]
        public decimal PaymentAmount { get; set; }

        [Required]
        [Column("PaymentType")]
        public string PaymentType { get; set; } = string.Empty;

        [Required]
        [Column("PaymentDate")]
        public DateTimeOffset PaymentDate { get; set; }

        [Column("PaymentMethod")]
        public string? PaymentMethod { get; set; }

        [Column("ReferenceNumber")]
        public string? ReferenceNumber { get; set; }
    }
}