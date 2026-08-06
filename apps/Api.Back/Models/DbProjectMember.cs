using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api.Back.Models;

public enum ProjectMemberRole
{
    Owner,
    Admin,
    Member
}

[Table("project_members")]
public class DbProjectMember
{
    [Column("project_id")]
    public Guid ProjectId { get; set; }

    [Column("identity_id")]
    public Guid IdentityId { get; set; }

    [Column("role")]
    [MaxLength(20)]
    public ProjectMemberRole Role { get; set; } = ProjectMemberRole.Member;

    [Column("joined_at")]
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    // ── Navigation ──────────────────────────────────────────────
    public DbProject Project { get; set; } = null!;
    public DbIdentity Identity { get; set; } = null!;
}