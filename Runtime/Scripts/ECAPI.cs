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
        public static bool SetUserGameScoreBool(int recordId, bool isOn, System.Action<string> failed = null)
        {
            failed = null == failed ? Debug.LogError : failed;
            var asset = ECRecordCollectionAsset.GetECRecordAsset();
            if (null == asset)
            {
                failed?.Invoke("No EC Record asset found.");
                return false;
            }
            return asset.SetGameScoreBool(recordId, isOn, failed);
        }
        public static bool GetUserGameScoreBool(int recordId)
        {
            var asset = ECRecordCollectionAsset.GetECRecordAsset();
            if (null == asset)
            {
                return false;
            }
            return asset.GetGameScoreBool(recordId);
        }

        public static void ResetAllUserScores()
        {
            ECRecordCollectionAsset.GetECRecordAsset()?.ResetUserScores();
        }
        public static void SubmitUserLearningRecord(System.Action<AbstractAPI.EmptyResponse> success = null, System.Action<string> failed = null)
        {
            var asset = ECRecordCollectionAsset.GetECRecordAsset();
            if (null == asset)
            {
                failed?.Invoke("No EC Record asset found.");
                return;
            }
            SubmitUserLearningRecord(asset.scenarioId, asset.managedRecords, success, failed);
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
