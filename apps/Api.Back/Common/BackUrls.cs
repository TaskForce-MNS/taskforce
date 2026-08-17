namespace Api.Back.Common
{
    public static class BackUrls
    {
        public const string BasePath = "api/v1/back";

        #region Auth
        public const string Register = BasePath + "/auth/register";
        public const string Login = BasePath + "/auth/login";
        public const string Logout = BasePath + "/auth/logout";
        public const string Me = BasePath + "/auth/me";
        public const string Refresh = BasePath + "/auth/login/refresh";
        #endregion

        #region Projects
        public const string PostProject = BasePath + "/project";
        public const string GetProject = BasePath + "/project/{id}";
        public const string ListProjects = BasePath + "/projects";
        public const string PatchProject = BasePath + "/project/{id}";
        public const string PutProject = BasePath + "/project/{id}";
        #endregion

        #region Invitations
        public const string ProjectInvitations = BasePath + "/projects/{projectId}/invitations";
        public const string Invitation = BasePath + "/invitations/{invitationId}";
        public const string AcceptInvitation = BasePath + "/invitations/accept";
        public const string JoinOrganization = BasePath + "/invitations/join";
        public const string ListMembers = BasePath + "/projects/{projectId}/listMembers";
        #endregion
    }
}