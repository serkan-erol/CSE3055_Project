using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Kismet.Entities.Enums;

namespace Kismet.Entities.Models 
{
    [Table("PurchaseOrder")]
    public class PurchaseOrder
    {
        [Key]
        [Column("POrderID")]
        [ForeignKey("POrder")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int POrderID { get; set; }

        [Required]
        [Column("OrderType", TypeName = "nvarchar(8)")]
        [ForeignKey("POrder")]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public string OrderType { get; set; } = "Puchase";

        // Navigation property for the Order super-type and PurchaseOrder sub-type relationship
        public Order POrder { get; set; } = null!;

        [Required]
        [Column("TotalAmount")]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public decimal TotalAmount { get; set; }

        [Required]
        [Column("IsApproved")]
        public bool IsApproved { get; set; }

        [Column("ApprovedBy", TypeName = "int")]
        [ForeignKey("ApprovingEmployee")]
        public int? ApprovedBy { get; set; }

        // Navigation property for the 
        public Employee? ApprovingEmployee { get; set; }

        [Column("ApprovalDate")]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTimeOffset? ApprovalDate { get; set; }
    }
}