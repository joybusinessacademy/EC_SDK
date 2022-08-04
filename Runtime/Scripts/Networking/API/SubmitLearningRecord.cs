﻿namespace SkillsVR.EnterpriseCloudSDK.Networking.API
{
    public class SubmitLearningRecord : AbstractAPI<SubmitLearningRecord.Data, AbstractAPI.EmptyResponse>
    {
        public SubmitLearningRecord() : base(ECAPI.domain + "/api/userlearningrecord") {
            requestType = HttpRequestType.POST;
            authenticated = true;
        }

        [System.Serializable]
        public class Data
        {
            public int scenarioId;
            public string location;
            public string duration;
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
