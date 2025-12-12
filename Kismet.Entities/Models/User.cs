using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kismet.Entities.Models 
{
    [Table("User")]
    public class User
    {
        [Key]
        [Column("UserID")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserID { get; set; }

        [Required]
        [Column("UserName")]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [Column("ContactEmail")]
        public string ContactEmail { get; set; } = string.Empty;

        [Column("ContactPhone")]
        public string? ContactPhone { get; set; }

        [Required]
        [Column("PasswordHash")]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        [Column("UserType", TypeName = "char(8)")]
        public string UserType { get; set; } = string.Empty;

        [Required]
        [Column("CreatedAt")]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTimeOffset CreatedAt { get; set; }

        [Column("LastUpdatedAt")]
        public DateTimeOffset? LastUpdatedAt { get; set; }

        // Sub-type Employee table
        public virtual ICollection<Employee> Employee { get; set; } = new List<Employee>();

        // Sub-type Customer table
        public virtual ICollection<Customer>? Customer { get; set; } = new List<Customer>();
    }
}