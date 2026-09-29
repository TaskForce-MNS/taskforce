using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
namespace Api.Back.Models
{
    [Table("tasks")]
    public class DbTask
    {
        [Column("task_id")]
        public Guid Id { get; set; }
        [Column("name")]
        public string Name { get; set; } = string.Empty;
        [Column("description")]
        public string? Description { get; set; }
        [Column("created_at")]
        public DateTimeOffset CreatedAt { get; set; }
        [Column("updated_at")]
        public DateTimeOffset? UpdatedAt { get; set; }
        [Column("closed_at")]
        public DateTimeOffset? ClosedAt { get; set; }
        [Column("is_checked")]
        public bool IsChecked { get; set; }
        [Column("is_archived")]
        public bool IsArchived { get; set; }
        [Column("due_date")]
        public DateTimeOffset? DueDate { get; set; }
        [Column("assignee_id")]
        public Guid? AssigneeId { get; set; }

        [Column("project_id")]
        public Guid ProjectId { get; set; }
        [Column("difficulty")]
        public TaskDifficulty Difficulty { get; init; }

        [Column("story_points")]
        public int StoryPoints { get; init; }

        [Column("target_week")]
        public DateTime? TargetWeek { get; init; }

        [Column("required_domains")]
        [SuppressMessage("Design", "CA1002:Do not expose generic lists",
            Justification = "Entité EF Core mappée nativement en text[] PostgreSQL via Npgsql.")]
        public List<string> RequiredDomains { get; init; } = new();
        public virtual DbIdentity? Assignee { get; set; }
        public DbProject? Project { get; set; }

    }
}