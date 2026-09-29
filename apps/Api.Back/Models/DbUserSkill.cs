namespace Api.Back.Models
{
    public enum ExperienceLevel
    {
        None = 0,
        Junior = 1,
        Intermediate = 2,
        Senior = 3
    }

    public enum TaskDifficulty
    {
        None = 0,    // 0 point
        Simple = 1,  //  2 points
        Medium = 2,  // 5 points
        Complex = 3  // 8 points
    }
    public class DbUserSkill
    {
        public Guid Id { get; init; } = Guid.NewGuid();

        public Guid UserId { get; init; }

        public required string Domain { get; init; }

        public ExperienceLevel Level { get; init; }
    }
}