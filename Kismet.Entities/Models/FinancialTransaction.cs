using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Kismet.Entities.Enums;

namespace Kismet.Entities.Models {

    [Table("FinancialTransaction")]
    public class FinancialTransaction
    {
        [Key]
        [Column("FTransactionID")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int FTransactionID { get; set; }

        [Column("CustomerID")]
        [ForeignKey("Customer")]
        public int CustomerID { get; set; }

        // Navigation property for the 1-to-M relationship with Customer
        public Customer Customer { get; set; } = null!;

        [Column("BillingID")]
        [ForeignKey("Billing")]
        public int BillingID { get; set; }

        // Navigation property for the 1-to-M relationship with Billing
        public Billing Billing { get; set; } = null!;

        [Column("OrderID")]
        [ForeignKey("Order")]
        public int OrderID { get; set; }

        // Navigation property for the 1-to-M relationship with Order
        public Order Order { get; set; } = null!;

        [Required]
        [Column("TransactionType", TypeName = "nvarchar(10)")]
        public string TransactionType { get; set; } = string.Empty;

        [Required]
        [Column("TotalAmount")]
        public decimal TotalAmount { get; set; }

        [Required]
        [Column("TotalPaid")]
        public decimal TotalPaid { get; set; }

        [Column("RemainingBalance")]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public decimal RemainingBalance { get; set; }

        [Required]
        [Column("PaymentStatus", TypeName = "int")]
        [EnumDataType(typeof(PaymentStatus), ErrorMessage = "Invalid payment status")]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public PaymentStatus PaymentStatus { get; set; }

        [Column("Description")]
        public string? Description { get; set; }

        [Required]
        [Column("TransactionDate")]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTimeOffset TransactionDate { get; set; }

        [Column("LastUpdatedAt")]
        public DateTimeOffset? LastUpdatedAt { get; set; }
    }
}