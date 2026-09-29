
using Api.Back.Models;

namespace Api.Back.Extensions
{
    public static class TaskDifficultyExtensions
    {
        public static int ToStoryPoints(this TaskDifficulty difficulty)
        {
            return difficulty switch
            {
                TaskDifficulty.Simple => 2,
                TaskDifficulty.Medium => 5,
                TaskDifficulty.Complex => 8,
                _ => 0
            };
        }
    }
}