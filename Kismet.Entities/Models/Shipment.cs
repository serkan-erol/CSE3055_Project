using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kismet.Entities.Models 
{
    [Table("Shipment")]
    public class Shipment
    {
        [Key]
        [Column("ShipmentID")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ShipmentID { get; set; }

        [Column("POrderID")]
        [ForeignKey("PurchaseOrder")]
        public int POrderID { get; set; }

        // Navigation property for the 1-to-M relationship with PurchaseOrder
        public PurchaseOrder PurchaseOrder { get; set; } = null!;

        [Column("CustomDocRef")]
        public string? CustomsDocRef { get; set; }

        
        [Required]
        [Column("ShipmentStatus", TypeName = "int")]
        [EnumDataType(typeof(ShipmentStatus), ErrorMessage = "Invalid shipment status")]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public ShipmentStatus ShipmentStatus { get; set; }

        [Required]
        [Column("IsLocked")]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public bool IsLocked { get; set; }

        [Column("LockedAt")]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTimeOffset? LockedAt { get; set; }

        [Column("ShipmentDate")]
        public DateTime? ShipmentDate { get; set; }

        [Column("OriginCountry")]
        public string? OriginCountry { get; set; }

        [Column("DestinationCountry")]
        public string? DestinationCountry { get; set; }

        [Column("ExpectedDeliveryDate")]
        public DateTime? ExpectedDeliveryDate { get; set; }

        [Column("ActualDeliveryDate")]
        public DateTime? ActualDeliveryDate { get; set; }

        [Required]
        [Column("CreatedAt")]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTimeOffset CreatedAt { get; set; }

        [Column("LastUpdatedAt")]
        public DateTimeOffset? LastUpdatedAt { get; set; }
    }
}