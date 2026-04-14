namespace Api.Back.Common
{
    public static class BackUrls
    {
        public const string BasePath = "api/v1/back";
        public const string Register = BasePath + "/auth/register";
        public const string Login = BasePath + "/auth/login";
        public const string Logout = BasePath + "/auth/logout";
        public const string Me = BasePath + "/auth/me";
    }
}