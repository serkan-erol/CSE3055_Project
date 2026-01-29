using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Kismet.Entities.Enums;

namespace Kismet.Entities.Models 
{
    [Table("Shipment")]
    public class Shipment
    {
        [Key]
        [Column("ShipmentID")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ShipmentID { get; set; }

        [Column("OrderID")]
        [ForeignKey("Order")]
        public int OrderID { get; set; }

        // Navigation property for the 1-to-M relationship with Order
        public Order Order { get; set; } = null!;

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
        public DateOnly? ShipmentDate { get; set; }

        [Column("OriginCountry")]
        public string? OriginCountry { get; set; }

        [Column("DestinationCountry")]
        public string? DestinationCountry { get; set; }

        [Column("ExpectedDeliveryDate")]
        public DateOnly? ExpectedDeliveryDate { get; set; }

        [Column("ActualDeliveryDate")]
        public DateOnly? ActualDeliveryDate { get; set; }

        [Required]
        [Column("CreatedAt")]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTimeOffset CreatedAt { get; set; }

        [Column("LastUpdatedAt")]
        public DateTimeOffset? LastUpdatedAt { get; set; }
    }
}