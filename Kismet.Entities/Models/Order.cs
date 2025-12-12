using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kismet.Entities.Models 
{
    [Table("Order")]
    public class Order
    {
        [Key]
        [Column("OrderID")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int OrderID { get; set; }

        [Column("CustomerID")]
        [ForeignKey("Customer")]    
        public int CustomerID { get; set; }

        // Navigation property for the 1-to-M relationship with Customer
        public Customer Customer { get; set; } = null!;

        [Required]
        [Column("OrderDate", TypeName = "date")]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime OrderDate { get; set; }

        [Required]
        [Column("OrderStatus", TypeName = "int")]
        [EnumDataType(typeof(OrderStatus), ErrorMessage = "Invalid order status")]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public OrderStatus OrderStatus { get; set; }

        [Required]
        [Column("IsLocked")]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public bool IsLocked { get; set; }

        [Column("LockedAt")]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTimeOffset? LockedAt { get; set; }

        [Required]
        [Column("OrderType", TypeName = "nvarchar(8)")]
        public string OrderType { get; set; } = string.Empty;

        [Required]
        [Column("CreatedAt")]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTimeOffset CreatedAt { get; set; }

        [Column("LastUpdatedAt")]
        public DateTimeOffset? LastUpdatedAt { get; set; }
    }
    
}