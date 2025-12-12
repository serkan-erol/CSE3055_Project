using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kismet.Entities.Models 
{
    [Table("SavedBankIndormation")]
    public class SavedBankIndormation
    {
        [Key]
        [Column("SBIID")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SBIID { get; set; }

        [Column("CustomerID")]
        [ForeignKey("Customer")]        
        public int CustomerID { get; set; }

        // Navigation property for the 1-to-M relationship with Customer
        public Customer Customer { get; set; } = null!;

        [Column("BankName")]
        public string? BankName { get; set; }

        [Column("AccountNo")]
        public string? AccountNo { get; set; }

        [Column("IBAN")]
        public string? IBAN { get; set; }

        [Required]
        [Column("CreatedAt")]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTimeOffset CreatedAt { get; set; }

        [Column("LastUpdatedAt")]
        public DateTimeOffset? LastUpdatedAt { get; set; }
    }
}