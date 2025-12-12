using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kismet.Entities.Models 
{
    [Table("SavedPaymentMethods")]
    public class SavedPaymentMethods
    {
        [Key]
        [Column("SPMID")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SPMID { get; set; }

        [Column("CustomerID")]
        [ForeignKey("Customer")]        
        public int CustomerID { get; set; }

        // Navigation property for the 1-to-M relationship with Customer
        public Customer Customer { get; set; } = null!;

        [Required]
        [Column("CardNumber")]
        public string CardNumber { get; set; } = string.Empty;

        [Required]
        [Column("CardType")]
        public string CardType { get; set; } = string.Empty;

        [Column("CardExpirationDate", TypeName = "date")]
        public DateTime CardExpirationDate { get; set; }

        [Column("RecordExpirationDate", TypeName = "date")]
        public DateTime? RecordExpirationDate { get; set; }

        [Required]
        [Column("CreatedAt")]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTimeOffset CreatedAt { get; set; }

        [Column("LastUpdatedAt")]
        public DateTimeOffset? LastUpdatedAt { get; set; }
    }
}