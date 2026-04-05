using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api.Back.Models
{
    [Table("PREFERENCES")]
    public class DbPreference
    {
        #region Identity
        [Key]
        [Column("id_preferences")]
        public Guid Id { get; set; } = Guid.NewGuid();
        #endregion

        #region Visual Settings
        // "Dark", "Light", "System" ?
        [Column("theme_preferences")]
        [MaxLength(10)]
        public string? Theme { get; set; }

        [Column("appearence_preferences")]
        [MaxLength(50)]
        public string? Appearance { get; set; }

        [Column("theme_auto_preferences")]
        public bool IsAutoTheme { get; set; } = true;
        #endregion

        #region Typography
        // Utilisation de decimal pour la précision NUMERIC(15,2)
        [Column("font_size_preferences", TypeName = "decimal(15,2)")]
        public decimal FontSize { get; set; } = 14.00m; // Valeur par défaut raisonnable

        [Column("letter_spacing_preferences", TypeName = "decimal(15,2)")]
        public decimal LetterSpacing { get; set; } = 0.00m;
        #endregion

        #region Relationships
        public virtual DbIdentity? Identity { get; set; }
        #endregion
    }
}
