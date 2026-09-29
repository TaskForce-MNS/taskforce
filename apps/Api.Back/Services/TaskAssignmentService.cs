using Api.Back.Models;
using Api.Back.Data;
using Api.Back.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Api.Back.Services
{
    public interface ITaskAssignmentService
    {
        Task<(Guid? AssigneeId, DateTime? TargetWeek)> CalculateAssignmentAsync(
            Guid projectId,
            TaskDifficulty difficulty,
            IReadOnlyList<string> requiredDomains);
    }

    public class TaskAssignmentService : ITaskAssignmentService
    {
        private readonly AppDbContext _context;
        private const int MAX_POINTS_PER_WEEK = 20;

        public TaskAssignmentService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<(Guid? AssigneeId, DateTime? TargetWeek)> CalculateAssignmentAsync(
            Guid projectId,
            TaskDifficulty difficulty,
            IReadOnlyList<string> requiredDomains)
        {
            var storyPoints = difficulty.ToStoryPoints();

            // 🌟 FIX 3 : Garde-fou pour éviter la boucle infinie si une tâche est trop grosse
            if (storyPoints > MAX_POINTS_PER_WEEK)
            {
                throw new InvalidOperationException(
                    $"Cette tâche ({storyPoints} points) dépasse la capacité hebdomadaire maximale ({MAX_POINTS_PER_WEEK} points) et ne peut pas être auto-assignée. Découpez-la en sous-tâches.");
            }

            var memberIds = await _context.ProjectMembers
                .Where(pm => pm.ProjectId == projectId)
                .Select(pm => pm.IdentityId)
                .ToListAsync();

            var usersSkills = await _context.UserSkills
                .Where(us => memberIds.Contains(us.UserId))
                .ToListAsync();

            var userSkillsDict = usersSkills
                .GroupBy(us => us.UserId)
                .ToDictionary(g => g.Key, g => g.ToList());

            var eligibleUserIds = new List<Guid>();

            // 🌟 FIX 1 : On n'exclut plus les utilisateurs sans compétences prématurément
            foreach (var userId in memberIds)
            {
                var userDomains = userSkillsDict.TryGetValue(userId, out var skills)
                    ? skills.Select(s => s.Domain.ToUpperInvariant()).ToList()
                    : new List<string>();

                bool hasAllDomains = requiredDomains.All(rd => userDomains.Contains(rd.ToUpperInvariant()));

                if (hasAllDomains)
                {
                    eligibleUserIds.Add(userId);
                }
            }

            if (eligibleUserIds.Count == 0)
            {
                return (null, null);
            }

            var currentWeek = DateTime.UtcNow.GetMondayOfWeek().Date;

            var futureTasks = await _context.Tasks
                .Where(t => t.AssigneeId != null
                         && eligibleUserIds.Contains(t.AssigneeId.Value)
                         && t.TargetWeek >= currentWeek)
                .Select(t => new { t.AssigneeId, t.TargetWeek, t.StoryPoints })
                .ToListAsync();

            // 🌟 FIX 3 : Plafond de sécurité à 52 semaines maximum
            const int MAX_WEEKS_LOOKAHEAD = 52;
            var weeksChecked = 0;

            DateTime targetWeek = currentWeek;
            Guid? selectedUserId = null;

            while (weeksChecked < MAX_WEEKS_LOOKAHEAD)
            {
                var candidatesWithCapacity = new List<Guid>();
                var userWorkloads = new Dictionary<Guid, int>();
                foreach (var userId in eligibleUserIds)
                {
                    var pointsInWeek = futureTasks
                        .Where(t => t.AssigneeId == userId &&
                                    t.TargetWeek.HasValue &&
                                    t.TargetWeek.Value.Date == targetWeek.Date)
                        .Sum(t => t.StoryPoints);

                    Console.WriteLine($"[DEBUG] User {userId} — semaine {targetWeek:yyyy-MM-dd} — charge actuelle: {pointsInWeek} pts, +{storyPoints} pts demandés");
                    if (pointsInWeek + storyPoints <= MAX_POINTS_PER_WEEK)
                    {
                        candidatesWithCapacity.Add(userId);
                        userWorkloads[userId] = pointsInWeek;
                    }
                }

                if (candidatesWithCapacity.Count > 0)
                {
                    selectedUserId = GetBestMatch(candidatesWithCapacity, userSkillsDict, requiredDomains, difficulty, userWorkloads);
                    break;
                }

                targetWeek = targetWeek.AddDays(7);
                weeksChecked++;
            }

            if (selectedUserId == null)
            {
                return (null, null);
            }

            return (selectedUserId, targetWeek);
        }

        private static Guid GetBestMatch(
            List<Guid> candidates,
            Dictionary<Guid, List<DbUserSkill>> userSkillsDict,
            IReadOnlyList<string> requiredDomains,
            TaskDifficulty difficulty,
            Dictionary<Guid, int> userWorkloads)

        {
            var targetLevel = (int)difficulty;
            var requiredDomainsUpper = requiredDomains.Select(rd => rd.ToUpperInvariant()).ToList();

            return candidates.OrderBy(userId =>
            {
                // Si l'utilisateur n'a pas de compétences enregistrées, on lui donne un niveau par défaut de 1
                if (!userSkillsDict.TryGetValue(userId, out var userSkills))
                {
                    return Math.Abs(1 - targetLevel);
                }

                var skills = userSkills
                    .Where(s => requiredDomainsUpper.Contains(s.Domain.ToUpperInvariant()))
                    .ToList();

                var avgLevel = skills.Count > 0 ? skills.Average(s => (int)s.Level) : 1;

                return Math.Abs(avgLevel - targetLevel);

            })
            .ThenBy(userId => userWorkloads[userId])
            .First();
        }
    }
}