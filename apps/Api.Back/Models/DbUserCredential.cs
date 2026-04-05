using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api.Back.Models
{
    [Table("USER_CREDENTIALS")]
    public class DbUserCredential
    {
        [Key]
        [Column("id_credential")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Column("descriptor_id")]
        public required byte[] DescriptorId { get; set; }

        [Column("public_key")]
        public required byte[] PublicKey { get; set; }

        [Column("user_handle")]
        public required byte[] UserHandle { get; set; }

        [Column("signature_counter")]
        public uint SignatureCounter { get; set; }

        [Column("aa_guid")]
        public Guid? AaGuid { get; set; }

        [Column("device_name")]
        [MaxLength(100)]
        public string? DeviceName { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        #region Relationships
        [Column("id_identity")]
        public Guid IdentityId { get; set; }

        [ForeignKey("IdentityId")]
        public virtual DbIdentity? Identity { get; set; }
        #endregion
    }
}