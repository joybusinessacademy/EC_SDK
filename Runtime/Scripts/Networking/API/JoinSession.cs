using SkillsVR.EnterpriseCloudSDK.Data;
using UnityEngine;

namespace SkillsVR.EnterpriseCloudSDK.Networking.API
{
    public partial class JoinSession : AbstractAPI<JoinSession.Data, CreateSession.Response>
    {
        public JoinSession(string scenarioId, string pinCode) : base(string.Format(ECAPI.domain + "/api/plannedsession/join/{0}/{1}", scenarioId.ToString(), pinCode))
        {
            requestType = HttpRequestType.POST;
            authenticated = true;
            data = new Data();
        }

        [System.Serializable]
        public class Data
        {
            public string deviceId = SystemInfo.deviceUniqueIdentifier;
        }
    }
}
