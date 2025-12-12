using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kismet.Entities.Models 
{
    [Table("SupplyOrder")]
    public class SupplyOrder
    {
        [Key]
        [Column("SOrderID")]
        [ForeignKey("SOrder")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SOrderID { get; set; }

        [Required]
        [Column("OrderType", TypeName = "nvarchar(8)")]
        [ForeignKey("SOrder")]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public string OrderType { get; set; } = string.Empty;

        // Navigation property for the Order super-type and SupplyOrder sub-type relationship
        public Order SOrder { get; set; } = null!;

        [Required]
        [Column("AmountOwed")]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public decimal AmountOwed { get; set; }
    }
}