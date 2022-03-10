namespace SkillsVR.Network.EC.API
{
    public class LoginOrganisation : AbstractAPI<LoginOrganisation.Data, Login.Response>
    {
        public LoginOrganisation() : base(RESTService.DOMAIN + "/api/auth/pick/organisation") {
            requestType = HttpRequestType.POST;
        }

        [System.Serializable]
        public class Data
        {
            public string accessToken;
            public int organisation;
            public string role;
            public string project;
        }

        [System.Serializable]
        public class Response : BaseResponse
        {
            public string accessToken;

            public override void Read(dynamic objs)
            {
                RESTService.SetAccessToken(accessToken);
            }
        }
    }
}
