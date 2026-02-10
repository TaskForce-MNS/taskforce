using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api.Back.Models
{

    [Table("USERS_CREDENTIALS")]
    public class UserCredential
    {
        [Key]
        [Column("id_users_crendentials")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Column("descriptor_id")]
        public byte[] DescriptorId { get; set; } = [];

        [Column("public_key")]
        public byte[] PublicKey { get; set; } = [];

        [Column("user_handle")]
        [MaxLength(100)]
        public required string UserHandle { get; set; }

        [Column("signature_counter")]
        public long SignatureCounter { get; set; }

        [Column("aa_guid")]
        [MaxLength(100)]
        public string? AaGuid { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("device_name")]
        [MaxLength(50)]
        public string? DeviceName { get; set; }

        #region Relationships
        [Column("id_users")]
        public Guid UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual DbUser? User { get; set; }
        #endregion
    }
}