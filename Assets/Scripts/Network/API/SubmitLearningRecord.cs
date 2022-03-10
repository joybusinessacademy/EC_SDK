namespace SkillsVR.Network.EC.API
{
    public class SubmitLearningRecord : AbstractAPI<SubmitLearningRecord.Data, AbstractAPI.EmptyResponse>
    {
        public SubmitLearningRecord() : base(RESTService.DOMAIN + "/api/userlearningrecord") {
            requestType = HttpRequestType.POST;
            authenticated = true;
        }

        [System.Serializable]
        public class Data
        {
            public int scenarioId;
            public string location;
            public System.DateTime duration;
            public string project;

            public Scores[] scores;

            [System.Serializable]
            public class Scores
            {
                public bool gameScore;
                public string code;
            }
        }
    }
}
