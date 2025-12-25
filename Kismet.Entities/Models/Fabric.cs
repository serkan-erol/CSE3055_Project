using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kismet.Entities.Models 
{
    [Table("Fabric")]
    public class Fabric
    {
        [Key]
        [Column("FabricID")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int FabricID { get; set; }

        [Required]
        [Column("FabricType")]
        public string FabricType { get; set; } = string.Empty;

        [Column("Composition")]
        public string? Composition { get; set; }

        [Column("Color")]
        public string? Color { get; set; }

        [Column("WeightPerUnit")]
        public decimal? WeightPerUnit { get; set; }

        [Required]
        [Column("StockQuantity")]
        public int StockQuantity { get; set; }

        [Required]
        [Column("UnitPrice")]
        public decimal UnitPrice { get; set; }

        [Column("Description")]
        public string? Description { get; set; }
    }
}