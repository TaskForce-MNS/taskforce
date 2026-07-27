using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api.Back.Models
{
    [Table("IDENTITIES")]
    public class DbIdentity
    {
        #region Core Identity
        [Key]
        [Column("id_identity")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Column("encrypted_profile_blob")]
        public required byte[] EncryptedProfile { get; set; }

        [Column("firstname_identity")]
        public required string FirstName { get; set; }

        [Column("Lastname_identity")]
        public required string LastName { get; set; }
        #endregion

        #region Professional Details
        [Column("experience_identity")]
        [MaxLength(2)]
        public required string Experience { get; set; }

        [Column("title_identity")]
        [MaxLength(100)]
        public string Title { get; set; } = string.Empty;

        [Column("current_workload_percentage_identity", TypeName = "decimal(5,2)")]
        public decimal CurrentWorkload { get; set; }

        [Column("workload_point_identity", TypeName = "decimal(5,2)")]
        public decimal WorkloadPoints { get; set; }
        #endregion

        #region Audit
        [Column("creation_date_identity")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("update_date_identity")]
        public DateTime? UpdatedAt { get; set; }

        [Column("isDeleted_identity")]
        public bool IsDeleted { get; set; } = false;
        #endregion

        #region Relationships
        [Column("id_preferences")]
        public Guid PreferenceId { get; set; }

        [ForeignKey("PreferenceId")]
        public virtual DbPreference? Preference { get; set; }

        public virtual ICollection<DbUserCredential> Credentials { get; } = new List<DbUserCredential>();
        #endregion
    }
}