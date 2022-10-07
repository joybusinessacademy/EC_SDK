using SkillsVR.EnterpriseCloudSDK.Data;
using System.Collections.Generic;

namespace SkillsVR.EnterpriseCloudSDK.Networking.API
{
    public partial class CreateSession : AbstractAPI<AbstractAPI.EmptyData, CreateSession.Response>
    {
        public CreateSession(int scenarioId) : base(string.Format(ECAPI.domain + "/api/plannedsession/create/{0}", scenarioId))
        {
            requestType = HttpRequestType.POST;
            authenticated = true;
        }

        [System.Serializable]
        public partial class Response : BaseResponse
        {
            public int code;
            public int message;
            public Data data = new Data();
        }

        [System.Serializable]
        public class Data {
            public int id;
            public List<PlayerData> players = new List<PlayerData>();
            public int scenarioId;
            public List<PlayerRoles> availablePlayerRoles = new List<PlayerRoles>();
        }

        [System.Serializable]
        public class PlayerData {
            public int id;
            public string pinCode;
            public string playerRoleId;
            public string plannedSessionId;
        }

        [System.Serializable]
        public class PlayerRoles
        {
            public int id;
            public string name;
            public string description;
        }
    }
}
