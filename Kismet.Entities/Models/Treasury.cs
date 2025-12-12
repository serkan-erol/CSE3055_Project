using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kismet.Entities.Models {

    [Table("Treasury")]
    public class Treasury
    {
        [Key]
        [Column("TreasuryID")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TreasuryID { get; set; }

        [Column("FTransactionID")]
        [ForeignKey("FinancialTransaction")]
        public int FTransactionID { get; set; }

        // Navigation property for the 1-to-1 relationship with FinancialTransaction
        public FinancialTransaction FinancialTransaction { get; set; } = null!;

        [Required]
        [Column("EntryDate")]
        public DateTimeOffset EntryDate { get; set; }

        [Required]
        [Column("Amount")]
        public decimal Amount { get; set; }

        [Required]
        [Column("BalanceAfter")]
        public decimal BalanceAfter { get; set; }

        [Column("Description")]
        public string? Description { get; set; }

        [Column("LastUpdatedAt")]
        public DateTimeOffset? LastUpdatedAt { get; set; }
    }
}