using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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

        [Column("project_id")]
        public Guid ProjectId { get; set; }

        public DbProject? Project { get; set; }
    }
}