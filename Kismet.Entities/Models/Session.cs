using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kismet.Entities.Models 
{
    [Table("Session")]
    public class Session
    {
        [Key]
        [Column("SessionID")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SessionID { get; set; }

        [Column("UserID")]
        [ForeignKey("User")]
        public int UserID { get; set; }

        // Navigation property for the 1-to-1 relationship with User
        public User User { get; set; } = null!;

        [Column("AccessToken")]
        public string? AccessToken { get; set; }

        [Required]
        [Column("RefreshToken")]
        public string RefreshToken { get; set; } = string.Empty;

        [Column("ATExpiresAt")]
        public DateTimeOffset? ATExpiresAt { get; set; }

        [Required]
        [Column("RTExpiresAt")]
        public DateTimeOffset RTExpiresAt { get; set; }

        [Required]
        [Column("CreatedAt")]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTimeOffset CreatedAt { get; set; }

        [Column("LastUpdatedAt")]
        public DateTimeOffset? LastUpdatedAt { get; set; }
    }
}