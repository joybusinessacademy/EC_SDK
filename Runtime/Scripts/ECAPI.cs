using SkillsVR.EnterpriseCloudSDK.Data;
using SkillsVR.EnterpriseCloudSDK.Networking;
using SkillsVR.EnterpriseCloudSDK.Networking.API;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SkillsVR.EnterpriseCloudSDK
{
    public class ECAPI
    {
        public static bool HasLoginToken()
        {
            return !string.IsNullOrWhiteSpace(RESTCore.AccessToken);
        }

        public static void Login(string @email, string @password, System.Action<Login.Response> success = null, System.Action<string> failed = null)
        {
            Login loginRequest = new Login()
            {
                data = new Login.Data
                {
                    email = @email,
                    password = @password
                }
            };
            RESTService.Send(loginRequest, success, failed);
        }


        public static void LoginOrganisation(System.Action<Login.Response> success = null, System.Action<string> failed = null)
        {
            LoginOrganisation loginOrganisationRequest = new LoginOrganisation()
            {
                data = new LoginOrganisation.Data
                {
                    accessToken = RESTCore.AccessToken,
                    organisation = 3707,
                    role = "OWNER",
                    project = "WE"
                }
            };
            RESTService.Send(loginOrganisationRequest, success, failed);
        }

        public static void SubmitTestUserLearningRecord(int @scenarioId, System.Action<AbstractAPI.EmptyResponse> success = null, System.Action<string> failed = null)
        {
            SubmitLearningRecord submitLearningRecordRequest = new SubmitLearningRecord()
            {
                data = new SubmitLearningRecord.Data
                {
                    scenarioId = @scenarioId,
                    location = "SKILLSVR HQ",
                    duration = System.DateTime.Now,
                    project = "WE",
                    scores = new SubmitLearningRecord.Data.Scores[] {
                        new SubmitLearningRecord.Data.Scores(){
                            gameScore = true,
                            code = "255_256" }
                    }
                }
            };

            RESTService.Send(submitLearningRecordRequest, success, failed);
        }

        public static void SubmitUserLearningRecord(int xScenarioId, IEnumerable<ECRecordContent> recordCollection, System.Action<AbstractAPI.EmptyResponse> success = null, System.Action<string> failed = null)
        {
            if (null == recordCollection)
            {
                failed?.Invoke("Record collection cannot be null.");
                return;
            }
            var scores = new List<SubmitLearningRecord.Data.Scores>();
            foreach (var record in recordCollection)
            {
                if (null == record)
                {
                    continue;
                }
                scores.Add(new SubmitLearningRecord.Data.Scores()
                {
                    gameScore = record.gameScoreBool,
                    code = record.code,
                });
            }
            var scoreArray = scores.ToArray();
            if (null == scoreArray || 0 == scoreArray.Length)
            {
                failed?.Invoke("Record collection cannot be empty.");
                return;
            }
            SubmitLearningRecord submitLearningRecordRequest = new SubmitLearningRecord()
            {
                data = new SubmitLearningRecord.Data
                {
                    scenarioId = xScenarioId,
                    location = "SKILLSVR HQ",
                    duration = System.DateTime.Now,
                    project = "WE",
                    scores = scoreArray,
                }
            };
            RESTService.Send(submitLearningRecordRequest, success, failed);
        }

        public static void GetConfig(int scenarioId, System.Action<GetConfig.Response> success = null, System.Action<string> failed = null)
        {
            GetConfig getConfigRequest = new GetConfig(scenarioId);
            RESTService.Send(getConfigRequest, success, failed);
        }
    }
}
