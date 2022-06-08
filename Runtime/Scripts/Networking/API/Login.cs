namespace SkillsVR.EnterpriseCloudSDK.Networking.API
{
    public class Login : AbstractAPI<Login.Data, Login.Response>
    {
        public Login() : base(RESTCore.DOMAIN + "/api/auth/login") {
            requestType = HttpRequestType.POST;
        }

        [System.Serializable]
        public class Data
        {
            public string email;
            public string password;
        }

        [System.Serializable]
        public class Response : BaseResponse
        {
            public int code;
            public int message;
            public Content data;

            public override void Read(dynamic objs)
            {
                RESTCore.SetAccessToken(data.accessToken);
            }

            [System.Serializable]
            public class Content
            {
                public string accessToken;
            }
        }
    }
}
