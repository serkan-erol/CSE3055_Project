using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kismet.Entities.Models 
{
    [Table("Customer")]
    public class Customer
    {
        [Key]
        [Column("CustomerID")]
        [ForeignKey("AssociatedCustomer")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CustomerID { get; set; }

        [Required]
        [Column("UserType", TypeName = "char(8)")]
        [ForeignKey("AssociatedCustomer")]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public string UserType { get; set; } = "Customer";

        // Navigation property for the User super-type and Customer sub-type relationship
        public User AssociatedCustomer { get; set; } = null!;

        [Required]
        [Column("CustomerNumber")]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public string CustomerNumber { get; set; } = string.Empty;

        [Column("CustomerType")]
        public string? EmployeeRole { get; set; }

        [Required]
        [Column("ReliabilityStatus")]
        public bool ReliabilityStatus { get; set; }
    }
}