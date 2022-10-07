using SkillsVR.EnterpriseCloudSDK.Data;

namespace SkillsVR.EnterpriseCloudSDK.Networking.API
{
    public partial class JoinSession : AbstractAPI<AbstractAPI.EmptyData, JoinSession.Response>
    {
        public JoinSession(int scenarioId, string pinCode) : base(string.Format(ECAPI.domain + "/api/plannedsession/join/{0}/{1}", scenarioId.ToString(), pinCode))
        {
            requestType = HttpRequestType.POST;
            authenticated = true;
        }

        [System.Serializable]
        public partial class Response : BaseResponse
        {
            public int code;
            public int message;
        }
    }
}
