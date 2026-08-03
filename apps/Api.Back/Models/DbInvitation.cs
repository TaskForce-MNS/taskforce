using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api.Back.Models;

[Table("invitations")]
public class DbInvitation
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Column("project_id")]
    public Guid ProjectId { get; set; }

    [Column("token")]
    [MaxLength(64)]
    public required string Token { get; set; }

    [Column("created_by")]
    public Guid CreatedById { get; set; }

    [Column("expires_at")]
    public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddDays(7);

    [Column("uses_left")]
    public int? UsesLeft { get; set; } = null;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // ── Navigation ──────────────────────────────────────────────
    public DbProject Project { get; set; } = null!;
    public DbIdentity CreatedBy { get; set; } = null!;
}