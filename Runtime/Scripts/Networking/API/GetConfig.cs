using SkillsVR.EnterpriseCloudSDK.Data;

namespace SkillsVR.EnterpriseCloudSDK.Networking.API
{
    public partial class GetConfig : AbstractAPI<AbstractAPI.EmptyData, GetConfig.Response>
    {
        public GetConfig(string recordId) : base(string.Format(ECAPI.domain + "/api/LearningRecordTemplate/{0}", recordId.ToString()))
        {
            requestType = HttpRequestType.GET;
            authenticated = true;
        }

        [System.Serializable]
        public partial class Response : BaseResponse
        {
            public int code;
            public int message;
            public ECRecordContent[] data;
        }
    }
}
