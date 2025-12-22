using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Kismet.Entities.Enums;

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
        [Column("OrderNumber")]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public string OrderNumber { get; set; } = string.Empty;

        [Required]
        [Column("OrderType", TypeName = "nvarchar(8)")]
        public string OrderType { get; set; } = string.Empty;

        [Required]
        [Column("TotalAmount")]
        public decimal TotalAmount { get; set; }

        [Required]
        [Column("OrderStatus", TypeName = "int")]
        [EnumDataType(typeof(OrderStatus), ErrorMessage = "Invalid order status")]
        public OrderStatus OrderStatus { get; set; }

        [Required]
        [Column("IsApproved")]
        public bool IsApproved { get; set; }

        [Column("ApprovedBy", TypeName = "int")]
        [ForeignKey("ApprovingEmployee")]
        public int? ApprovedBy { get; set; }

        // Navigation property for the 1-to-1 relationship with Employee
        public Employee? ApprovingEmployee { get; set; }

        [Column("ApprovalDate")]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTimeOffset? ApprovalDate { get; set; }

        [Required]
        [Column("IsLocked")]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public bool IsLocked { get; set; }

        [Column("LockedAt")]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTimeOffset? LockedAt { get; set; }

        [Required]
        [Column("OrderDate")]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTimeOffset OrderDate { get; set; }

        [Column("LastUpdatedAt")]
        public DateTimeOffset? LastUpdatedAt { get; set; }
    }
}