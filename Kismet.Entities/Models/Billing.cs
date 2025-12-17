using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kismet.Entities.Models 
{
    [Table("Billing")]
    public class Billing
    {
        [Key]
        [Column("BillingID")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int BillingID { get; set; }

        [Column("CustomerID")]
        [ForeignKey("Customer")]    
        public int CustomerID { get; set; }

        // Navigation property for the 1-to-M relationship with Customer
        public Customer Customer { get; set; } = null!;

        [Required]
        [Column("InvoiceNumber")]
        public string InvoiceNumber { get; set; } = string.Empty;

        [Required]
        [Column("TotalDue")]
        public decimal TotalDue { get; set; }

        [Required]
        [Column("TotalPaid")]
        public decimal TotalPaid { get; set; }

        [Column("RemainingBalance")]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public decimal RemainingBalance { get; set; }

        [Column("PaymentTerms")]
        public string? PaymentTerms { get; set; }

        [Required]
        [Column("BillingDate")]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime BillingDate { get; set; }

        [Required]
        [Column("BillingStatus", TypeName = "int")]
        [EnumDataType(typeof(PaymentStatus), ErrorMessage = "Invalid billing status")]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public PaymentStatus BillingStatus { get; set; }

        [Required]
        [Column("CreatedAt")]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTimeOffset CreatedAt { get; set; }

        [Column("LastUpdatedAt")]
        public DateTimeOffset? LastUpdatedAt { get; set; }
    }
}