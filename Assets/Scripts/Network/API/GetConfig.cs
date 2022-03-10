namespace SkillsVR.Network.EC.API
{
    public class GetConfig : AbstractAPI<AbstractAPI.EmptyData, GetConfig.Response>
    {
        public GetConfig(int recordId) : base(string.Format(RESTService.DOMAIN + "/api/LearningRecordTemplate/{0}", recordId.ToString()))
        {
            requestType = HttpRequestType.GET;
            authenticated = true;
        }

        [System.Serializable]
        public class Response : BaseResponse
        {
            public int code;
            public int message;
            public Content[] data;

            [System.Serializable]
            public class Content
            {
                public int id;
                public string parentId;
                public string code;
                public int index;
                public int depth;
                public int scenarioId;
                public string scenario;
                public string name;
                public int type;
                public string passCondition;
                public string requirement;
            }
        }
    }
}
