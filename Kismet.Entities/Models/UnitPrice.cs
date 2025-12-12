using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kismet.Entities.Models 
{
    [Table("UnitPrice")]
    public class UnitPrice
    {
        [Key]
        [Column("UPID")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UPID { get; set; }

        [Column("FabricID")]
        [ForeignKey("Fabric")]
        public int FabricID { get; set; }

        //// Navigation property for the 1-to-1 relationship with Fabric
        public Fabric Fabric { get; set; } = null!;

        [Required]
        [Column("Price")]
        public decimal Price { get; set; }

        [Required]
        [Column("Currency")]
        public string Currency { get; set; } = string.Empty;

        [Column("EquivalentTLPrice")]
        public decimal EquivalentTLPrice { get; set; }
    }
}