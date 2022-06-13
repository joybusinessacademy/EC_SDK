using System;
using System.Collections.Generic;

namespace SkillsVR.EnterpriseCloudSDK.Networking.API
{
    public class Login : AbstractAPI<Login.Data, Login.Response>
    {
        public Login() : base(RESTCore.domain + "/api/auth/login") {
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
            public UserData data;

            public override void Read(dynamic objs)
            {
                RESTCore.SetAccessToken(data.accessToken);
            }

            [Serializable]
            public class OrganisationData
            {
                [Serializable]
                public class RoleData
                {
                    public string name;
                    public string key;
                }

                public string id;
                public string name;
                public List<RoleData> roles = new List<RoleData>();
                public bool isOrganisationPte;
            }

            [System.Serializable]
            public class UserData
            {
                public string accessToken;
                public List<OrganisationData> organisations = new List<OrganisationData>();
                public bool isNeed2FA;
            }
        }
    }
}
