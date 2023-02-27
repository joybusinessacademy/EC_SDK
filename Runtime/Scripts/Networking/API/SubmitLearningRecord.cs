using System.Collections.Generic;

namespace SkillsVR.EnterpriseCloudSDK.Networking.API
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
            public string moduleId;
            public string location;
            public string duration;
            public string project;
            public string pinCode;

            public List<Scores> userLearningRecords = new List<Scores>();
            public List<SkillScores> skillScores = new List<SkillScores>();

            [System.Serializable]
            public class Scores
            {
                public bool gameScore;
                public string code;
            }
            
            [System.Serializable]
            public class SkillScores
            {
                public int skillId;
                public string score;
            }
        }
    }
}
