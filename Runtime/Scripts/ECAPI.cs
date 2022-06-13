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
        public enum Environment
        {
            Development,
            Internal,
            Staging,
            Production,
        }
        public static Environment environment = Environment.Internal;

        [RuntimeInitializeOnLoadMethod]
        private static void SetupEnvironment()
        {
            environment = GetEnvironmentByDefineSymbol();
            Debug.Log("EC ENV: " + environment);
        }

        public static Environment GetEnvironmentByDefineSymbol()
        {
#if ENVIRONMENT_DEVELOPMENT
            return Environment.Development;
#elif ENVIRONMENT_INTERNAL
            return Environment.Internal;
#elif ENVIRONMENT_STAGING
            return Environment.Staging;
#elif ENVIRONMENT_PRODUCTION
            return Environment.Production;
#else
            return environment;
#endif
        }

        /// <summary>
        /// Check already have token for authenticated requests.
        /// </summary>
        /// <returns>bool - have token or not</returns>
        public static bool HasLoginToken()
        {
            return !string.IsNullOrWhiteSpace(RESTCore.AccessToken);
        }

        /// <summary>
        /// Login user to EC backend and grab access token.
        /// </summary>
        /// <param name="email">user account</param>
        /// <param name="password">user password</param>
        /// <param name="success">Action runs when login success. Params: Login.Response - response data for login request.</param>
        /// <param name="failed">Action runs when login fail, including http and network errors. Params: string - the error message.</param>
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
        /// <summary>
        /// Login use to organisation with access token.
        /// </summary>
        /// <param name="organisationId">User organisation id.Can get from LoginResponse.data.organisations.id.</param>
        /// <param name="userRoleName">User role name. Can get from LoginResponse.data.organisations.roles.key.</param>
        /// <param name="userProjectName">User project name. Can get from LoginResponse.data.organisations.name.</param>
        /// <param name="success">Action runs when login success. Params: Login.Response - response data for login request.</param>
        /// <param name="failed">Action runs when login fail, including http and network errors. Params: string - the error message.</param>
        public static void LoginOrganisation(int organisationId, string userRoleName, string userProjectName, System.Action<Login.Response> success = null, System.Action<string> failed = null)
        {
            LoginOrganisation loginOrganisationRequest = new LoginOrganisation()
            {
                data = new LoginOrganisation.Data
                {
                    accessToken = RESTCore.AccessToken,
                    organisation = organisationId,
                    role = userRoleName,
                    project = userProjectName
                }
            };
            RESTService.Send(loginOrganisationRequest, success, failed);
        }

       
        /// <summary>
        /// Set bool type game score to a record by record id.
        /// </summary>
        /// <param name="recordId">Id that matches ECRecordContent.id.</param>
        /// <param name="isOn">Tick the game score or not.</param>
        /// <param name="failed">Action runs when error occurs. Params: string - the error string.</param>
        /// <returns>Is success of setting user game score.</returns>
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

        /// <summary>
        /// Get bool type game score from a record by record id.
        /// </summary>
        /// <param name="recordId">Id that matches ECRecordContent.id.</param>
        /// <returns>Boolean type value of game score.</returns>
        public static bool GetUserGameScoreBool(int recordId)
        {
            var asset = ECRecordCollectionAsset.GetECRecordAsset();
            if (null == asset)
            {
                return false;
            }
            return asset.GetGameScoreBool(recordId);
        }

        /// <summary>
        /// Reset all user scores to init stats. Any user changes will be lost.
        /// </summary>
        public static void ResetAllUserScores()
        {
            ECRecordCollectionAsset.GetECRecordAsset()?.ResetUserScores();
        }

        /// <summary>
        /// Submit all user scores to EC backend. Note: for v1.0.0 only send records that type is 0 (bool type game score).
        /// </summary>
        /// <param name="success">Action runs when submit success. Params: AbstractAPI.EmptyResponse - not in use, empty data.</param>
        /// <param name="failed">Action runs when submit fail, including http and network errors. Params: string - the error message.</param>
        public static void SubmitUserLearningRecord(System.Action<AbstractAPI.EmptyResponse> success = null, System.Action<string> failed = null)
        {
            var asset = ECRecordCollectionAsset.GetECRecordAsset();
            if (null == asset)
            {
                failed?.Invoke("No EC Record asset found.");
                return;
            }
            SubmitUserLearningRecord(asset.currentConfig.scenarioId, asset.currentConfig.managedRecords, success, failed);
        }

        /// <summary>
        /// Submit user scores from custom records to a scenario. Note: for v1.0.0 only send records that type is 0 (bool type game score).
        /// </summary>
        /// <param name="xScenarioId">The id of scenario to be sent.</param>
        /// <param name="recordCollection">List of records to be sent. Note: for v1.0.0 only send records that type is 0 (bool type game score).</param>
        /// <param name="success">Action runs when submit success. Params: AbstractAPI.EmptyResponse - not in use, empty data.</param>
        /// <param name="failed">Action runs when submit fail, including http and network errors. Params: string - the error message.</param>
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
                // for v1.0.0: backend server only accept bool game score records.
                // otherwise will receive error 400.
                // May changes later.
                if (record.isScoreTypeBool)
                {
                    scores.Add(new SubmitLearningRecord.Data.Scores()
                    {
                        gameScore = record.gameScoreBool,
                        code = record.code,
                    });
                }
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

        /// <summary>
        /// Download scenario record config by id. 
        /// </summary>
        /// <param name="scenarioId">Scenario config id</param>
        /// <param name="success">Action runs when submit success. Params: GetConfig.Response - config data including a list of records.</param>
        /// <param name="failed">Action runs when submit fail, including http and network errors. Params: string - the error message.</param>
        public static void GetConfig(int scenarioId, System.Action<GetConfig.Response> success = null, System.Action<string> failed = null)
        {
            GetConfig getConfigRequest = new GetConfig(scenarioId);
            RESTService.Send(getConfigRequest, success, failed);
        }
    }
}
