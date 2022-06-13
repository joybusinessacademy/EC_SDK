namespace SkillsVR.EnterpriseCloudSDK.Networking.API
{
    public class LoginOrganisation : AbstractAPI<LoginOrganisation.Data, Login.Response>
    {
        public LoginOrganisation() : base(RESTCore.domain + "/api/auth/pick/organisation") {
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
                RESTCore.SetAccessToken(accessToken);
            }
        }
    }
}
