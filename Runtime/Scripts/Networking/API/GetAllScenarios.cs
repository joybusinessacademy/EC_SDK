using SkillsVR.EnterpriseCloudSDK.Data;
using System.Collections.Generic;

namespace SkillsVR.EnterpriseCloudSDK.Networking.API
{
    public partial class GetAllScenarios : AbstractAPI<AbstractAPI.EmptyData, GetAllScenarios.Response>
    {
        public GetAllScenarios() : base(string.Format(ECAPI.domain + "/api/module/all-for-headsets"))
        {
            requestType = HttpRequestType.GET;
            authenticated = true;
        }

        [System.Serializable]
        public partial class Response : BaseResponse
        {
            public int code;
            public int message;
            public List<ScenarioData> data = new List<ScenarioData>();
        }

        [System.Serializable]
        public class ScenarioData
        {
            public string id;
            public string name;
            public string version;
            public string description;
            public string uri;      
            public string bannerUri;
            public List<ApkConfig> apkFiles = new List<ApkConfig>();
        }

        [System.Serializable]
        public class ApkConfig
        {
            public string name;            
            public string sas;
            public string videoLink;
            public int scenarioId;
            public string device;
            public string apkFileVersion;
            public string appName;      
            public string description;
        }
    }
}
