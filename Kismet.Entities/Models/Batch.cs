using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kismet.Entities.Models 
{
    [Table("Batch")]
    public class Batch
    {
        [Key]
        [Column("BatchID")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int BatchID { get; set; }

        [Column("OrderID")]
        [ForeignKey("Order")]
        public int OrderID { get; set; }

        // Navigation property for the 1-to-M relationship with Order
        public Order Order { get; set; } = null!;

        [Column("ShipmentID")]
        [ForeignKey("Shipment")]
        public int ShipmentID { get; set; }

        // Navigation property for the 1-to-M relationship with Shipment
        public Shipment Shipment { get; set; } = null!;

        [Column("FabricID")]
        [ForeignKey("Fabric")]
        public int FabricID { get; set; }

        // Navigation property for the 1-to-M relationship with Fabric
        public Fabric Fabric { get; set; } = null!;

        [Required]
        [Column("BatchNumber")]
        public string BatchNumber { get; set; } = string.Empty;

        [Required]
        [Column("Quantity")]
        public int Quantity { get; set; }

        [Column("BatchPrice")]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public decimal BatchPrice { get; set; }

        [Column("ProductionDate")]
        public DateTime? ProductionDate { get; set; }

        [Column("QualityGrade")]
        public string? QualityGrade { get; set; }
    }
}