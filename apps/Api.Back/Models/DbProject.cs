using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api.Back.Models
{

    [Table("projects")]
    public class DbProject
    {
        [Key]
        [Column("id_project")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Column("name")]
        [MaxLength(50)]
        public required string Name { get; set; }

        [Column("description")]
        public string? Description { get; set; }

        [Column("color_hex")]
        [MaxLength(7)]
        public string? ColorHex { get; set; }

        [Column("image_url")]
        [MaxLength(255)]
        public string? ImageUrl { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [Column("created_by")]
        public Guid CreatedById { get; set; }

        public DbIdentity CreatedBy { get; set; } = null!;
    }
}
