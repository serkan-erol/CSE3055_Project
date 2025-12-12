using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kismet.Entities.Models 
{
    [Table("Employee")]
    public class Employee
    {
        [Key]
        [Column("EmployeeID")]
        [ForeignKey("AssociatedEmployee")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int EmployeeID { get; set; }

        [Required]
        [Column("UserType", TypeName = "char(8)")]
        [ForeignKey("AssociatedEmployee")]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public string UserType { get; set; } = "Employee";

        // Navigation property for the User super-type and Employee sub-type relationship
        public User AssociatedEmployee { get; set; } = null!;

        [Required]
        [Column("EmployeeNumber")]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public string EmployeeNumber { get; set; } = string.Empty;

        [Required]
        [Column("EmployeeRole")]
        public string EmployeeRole { get; set; } = string.Empty;

        [Required]
        [Column("AccessLevel")]
        public int AccessLevel { get; set; }
    }
}