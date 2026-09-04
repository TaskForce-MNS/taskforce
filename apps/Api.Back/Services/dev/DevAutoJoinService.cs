using Api.Back.Data;
using Api.Back.Models;
using Api.Back.Repositories;

namespace Api.Back.Services.dev
{
    public interface IDevAutoJoinService
    {
        Task JoinDemoProjectsIfDevAsync(Guid userId);
    }

    public class DevAutoJoinService : IDevAutoJoinService
    {
#pragma warning disable CA1859
        private readonly IProjectMemberRepository _memberRepository;
#pragma warning restore CA1859
        private readonly IWebHostEnvironment _environment;

        public DevAutoJoinService(IProjectMemberRepository memberRepository, IWebHostEnvironment environment)
        {
            _memberRepository = memberRepository;
            _environment = environment;
        }

        public async Task JoinDemoProjectsIfDevAsync(Guid userId)
        {
            if (!_environment.IsDevelopment())
            {
                return;
            }

            foreach (var projectId in DbSeeder.DemoProjectIds)
            {
                var existing = await _memberRepository.GetAsync(projectId, userId);
                if (existing != null)
                {
                    continue;
                }
                await _memberRepository.AddAsync(new DbProjectMember
                {
                    ProjectId = projectId,
                    IdentityId = userId,
                    Role = ProjectMemberRole.Member,
                    JoinedAt = DateTime.UtcNow,
                });
            }
        }
    }
}