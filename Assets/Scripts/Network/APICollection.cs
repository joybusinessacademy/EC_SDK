using SkillsVR.Network.EC;
using SkillsVR.Network.EC.API;
using UnityEngine;
using UnityEngine.Networking;

namespace SkillsVR.SkillsVR.Network.EC
{
    public class APICollection : RESTService
    {
        public void Login(string @email, string @password, System.Action<Login.Response> success = null, System.Action<string> failed = null)
        {
            Login loginRequest = new Login()
            {
                data = new Login.Data
                {
                    email = @email,
                    password = @password
                }
            };
            Send(loginRequest, success, failed);
        }


        public void LoginOrganisation(System.Action<Login.Response> success = null, System.Action<string> failed = null)
        {
            LoginOrganisation loginOrganisationRequest = new LoginOrganisation()
            {
                data = new LoginOrganisation.Data
                {
                    accessToken = RESTService.AccessToken,
                    organisation = 3707,
                    role = "OWNER",
                    project = "WE"
                }
            };
            Send(loginOrganisationRequest, success, failed);
        }

        public void SubmitUserLearningRecord(int @scenarioId, System.Action<AbstractAPI.EmptyResponse> success = null, System.Action<string> failed = null)
        {
            SubmitLearningRecord submitLearningRecordRequest = new SubmitLearningRecord()
            {
                data = new SubmitLearningRecord.Data
                {
                    scenarioId = @scenarioId,
                    location = "SKILLSVR HQ",     
                    duration =  System.DateTime.Now,
                    project = "WE",
                    scores = new SubmitLearningRecord.Data.Scores[] {
                        new SubmitLearningRecord.Data.Scores(){
                            gameScore = true,
                            code = "255_256" }
                    }
                }
            };

            Send(submitLearningRecordRequest, success, failed);
        }


        public void GetConfig(int recordId, System.Action<GetConfig.Response> success = null, System.Action<string> failed = null)
        {
            GetConfig getConfigRequest = new GetConfig(recordId);
            Send(getConfigRequest, success, failed);
        }
    }

}
