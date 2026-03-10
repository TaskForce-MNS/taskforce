using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api.Back.Models
{
    [Table("USERS")]
    public class DbUser
    {
        #region Identity & Account
        [Key]
        [Column("id_users")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Column("name_users")]
        [MaxLength(50)]
        public required string Name { get; set; }

        [Column("firstname_users")]
        [MaxLength(50)]
        public required string FirstName { get; set; }

        [Column("email_users")]
        [MaxLength(255)]
        public required string Email { get; set; }

        [Column("email_verified_users")]
        public bool IsEmailVerified { get; set; } = false;

        [Column("password_hash_users")]
        public required string PasswordHash { get; set; }

        [Column("experience_users")]
        [MaxLength(2)]
        public required string Experience { get; set; }
        #endregion

        #region Professional Details
        [Column("title_users")]
        [MaxLength(100)]
        public string? Title { get; set; }

        [Column("current_workload_percentage_users", TypeName = "decimal(5,2)")]
        public decimal CurrentWorkload { get; set; }

        [Column("workload_point_users", TypeName = "decimal(5,2)")]
        public decimal WorkloadPoints { get; set; }
        #endregion

        #region Audit & Lifecycle

        [Column("creation_date_users")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("update_date_users")]
        public DateTime? UpdatedAt { get; set; }

        [Column("isDeleted_users")]
        public bool IsDeleted { get; set; } = false;
        #endregion

        #region Relationships

        // Foreign Key vers Preferences
        [Column("id_preferences")]
        public Guid PreferenceId { get; set; }

        // Navigation Property : Permet de faire user.Preference.Theme
        [ForeignKey("PreferenceId")]
        public virtual Preference? Preference { get; set; }

        // Navigation Property : Permet de faire user.Credentials
        public virtual List<UserCredential> Credentials { get; set; } = new();
        #endregion
    }
}