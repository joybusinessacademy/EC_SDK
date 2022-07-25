using SkillsVR.EnterpriseCloudSDK.Data;
using System.Collections.Generic;

namespace SkillsVR.EnterpriseCloudSDK.Networking.API
{
    public partial class GetAllScenarios : AbstractAPI<AbstractAPI.EmptyData, GetAllScenarios.Response>
    {
        public GetAllScenarios() : base(string.Format(ECAPI.domain + "/api/scenario/all"))
        {
            requestType = HttpRequestType.GET;
            authenticated = true;
        }

        [System.Serializable]
        public partial class Response : BaseResponse
        {
            public int code;
            public int message;
            public List<ScenarioData> data;
        }

        [System.Serializable]
        public class ScenarioData
        {
            public string name;
            public string version;
            public string description;

            public List<ApkConfig> apkFiles;
        }

        public class ApkConfig
        {
            public string name;            
            public string sas;
            public int scenarioId;
            public string device;
        }
    }
}
