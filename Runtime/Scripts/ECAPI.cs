using SkillsVR.EnterpriseCloudSDK.Data;
using SkillsVR.EnterpriseCloudSDK.Networking;
using SkillsVR.EnterpriseCloudSDK.Networking.API;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SkillsVR.EnterpriseCloudSDK
{
    public class ECAPI
    {
        public static string domain = ""; // https://internal-ec-bff.skillsvr.com
        public static string activePinCode = "";
        public const string domainIntentId = "DOMAIN";        
        public const string IntentScenarioIdKey = "SCENARIO_ID";
        public const string IntentPinCodeIdKey = "PIN_CODE";
        public const string refreshToken = "REFRESH_TOKEN";
        public const string accessToken = "ACCESS_TOKEN";

        /// <summary>
        /// Check already have token for authenticated requests.
        /// </summary>
        /// <returns>bool - have token or not</returns>
        public static bool HasLoginToken()
        {
            TryFetchAccessTokenFromIntent();
            return !string.IsNullOrWhiteSpace(RESTCore.AccessToken);
        }

#if !UNITY_EDITOR
        public static readonly AndroidJavaClass unityPlayerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        public static readonly AndroidJavaObject unityPlayerActivityObject = unityPlayerClass.GetStatic<AndroidJavaObject>("currentActivity");
#endif

        public static string EC_BROADCAST_ACTION = "com.skillsvr.ECAPI_ACTION";

        public static void SendToAndroid(string payload)
        {
#if !UNITY_EDITOR && UNITY_ANDROID
            AndroidJavaObject sendIntent = new AndroidJavaObject("android.content.Intent", EC_BROADCAST_ACTION);
            sendIntent.Call<AndroidJavaObject>("putExtra", "PAYLOAD", payload);
            unityPlayerActivityObject.Call("sendBroadcast", sendIntent);
#else
            Debug.Log("SendToAndroid " + payload);
#endif
        }
        
        /// <summary>
        /// Check if access token exist on activity intent
        /// Use by Skills VR B2C login
        /// </summary>        
        public static void TryFetchAccessTokenFromIntent()
        {
#if !UNITY_EDITOR && UNITY_ANDROID            
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaObject activityIntent = currentActivity.Call<AndroidJavaObject>("getIntent");
            string token = activityIntent.Call<string>("getStringExtra", accessToken);
            if (string.IsNullOrEmpty(token) == false)
            {
                RESTCore.SetAccessToken(token);
                activePinCode = TryFetchStringFromIntent(IntentPinCodeIdKey) ?? string.Empty;
                ECAPI.domain = ECAPI.TryFetchStringFromIntent(domainIntentId);
            }                        
#endif
        }

        /// <summary>
        /// Fetch intent session id from library app
        /// Use by Skills VR B2C login
        /// </summary>        
        public static string TryFetchSessionIdFromIntent()
        {
#if !UNITY_EDITOR && UNITY_ANDROID
            return TryFetchStringFromIntent("SESSION_ID");
#endif
            return string.Empty;
        }

        /// <summary>
        /// Fetch intent key value coming from library app
        /// Use by Skills VR B2C login
        /// </summary>  
        public static string TryFetchStringFromIntent(string key)
        {
#if !UNITY_EDITOR && UNITY_ANDROID            
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaObject activityIntent = currentActivity.Call<AndroidJavaObject>("getIntent");
            return activityIntent.Call<string>("getStringExtra", key);
#endif
            return null;
        }

        /// <summary>
        /// Login user to EC backend and grab access token.
        /// </summary>
        /// <param name="loginData">user login data includes: user name, password, client id, login url and scope.</param>
        /// <param name="success">Action runs when login success. Params: SSOLoginResponse - response data for login request.</param>
        /// <param name="failed">Action runs when login fail, including http and network errors. Params: string - the error message.</param>
        public static void Login(SSOLoginData loginData, System.Action<SSOLoginResponse> success = null, System.Action<string> failed = null)
        {
            RESTService.SendByCustomCoroutine(SSOLogin.SendSSOLoginForm(loginData, success, failed));
        }

        /// <summary>
        /// Set bool type game score to a record by record id.
        /// </summary>
        /// <param name="recordId">Id that matches ECRecordContent.id.</param>
        /// <param name="isOn">Tick the game score or not.</param>
        /// <param name="failed">Action runs when error occurs. Params: string - the error string.</param>
        /// <returns>Is success of setting user game score.</returns>
        public static bool SetUserGameScoreBool(string recordId, bool isOn, System.Action<string> failed = null)
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
        public static bool GetUserGameScoreBool(string recordId)
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
        public static void SubmitUserLearningRecord(Dictionary<string, string> userScore = null, System.Action<AbstractAPI.EmptyResponse> success = null, System.Action<string> failed = null)
        {
            var asset = ECRecordCollectionAsset.GetECRecordAsset();
            if (null == asset)
            {
                failed?.Invoke("No EC Record asset found.");
                return;
            }

            if (userScore != null)
            {
                var list = userScore.ToList();
                list.ForEach(pair =>
                {
                    int skId = -999;
                    int.TryParse(pair.Key, out skId);
                    if (skId != -999)
                    {
                        asset.currentConfig.skillRecords.Add(new ECRecordSkillScore()
                        {
                            skillId = skId,
                            score = pair.Value
                        });
                    }
                });
            }

            var targeRecords = asset.currentConfig.managedRecords;
            var targetScenarioId = asset.currentConfig.scenarioId;
            // remap here
            if (asset.currentConfig.managedRecords.Count == asset.currentConfig.runtimeManagedRecords.Count)
            {
                for (int x = 0; x < asset.currentConfig.managedRecords.Count; x++)
                {
                    asset.currentConfig.runtimeManagedRecords[x].gameScoreBool = asset.currentConfig.managedRecords[x].gameScoreBool;
                }
                targeRecords = asset.currentConfig.runtimeManagedRecords;
                targetScenarioId = ECAPI.TryFetchStringFromIntent(ECAPI.IntentScenarioIdKey) ?? i.currentConfig.scenarioId;
            }

            SubmitUserLearningRecord(targetScenarioId, asset.currentConfig.durationMS, targeRecords, asset.currentConfig.skillRecords, success, failed);
        }

        /// <summary>
        /// Submit user scores from custom records to a scenario. Note: for v1.0.0 only send records that type is 0 (bool type game score).
        /// </summary>
        /// <param name="xScenarioId">The id of scenario to be sent.</param>
        /// <param name="recordCollection">List of records to be sent. Note: for v1.0.0 only send records that type is 0 (bool type game score).</param>
        /// <param name="success">Action runs when submit success. Params: AbstractAPI.EmptyResponse - not in use, empty data.</param>
        /// <param name="failed">Action runs when submit fail, including http and network errors. Params: string - the error message.</param>
        public static void SubmitUserLearningRecord(string xScenarioId, long durationMS, IEnumerable<ECRecordContent> recordCollection, IEnumerable<ECRecordSkillScore> skillCollection = null, System.Action<AbstractAPI.EmptyResponse> success = null, System.Action<string> failed = null)
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


            var skillScores = new List<SubmitLearningRecord.Data.SkillScores>();

            if (skillCollection != null)
            {
                foreach (var skill in skillCollection)
                {
                    if (null == skill)
                    {
                        continue;
                    }
                    string json = JsonUtility.ToJson(skill);
                    skillScores.Add(JsonUtility.FromJson<SubmitLearningRecord.Data.SkillScores>(json));
                }
            }

            // final score exist ??
            if (skillScores.Find(k => k.skillId == 16) == null)
            {
                // force compute final score
                float total = 0;
                float summed = 0;
                skillScores.ForEach(i =>
                {
                    int parsedScore = -1;
                    int.TryParse(i.score, out parsedScore);
                    if (parsedScore > -1)
                    {
                        total += parsedScore;
                        summed++;
                    }
                });
                float ave = summed == 0 ? 0f : (total / summed) * 100f;
                skillScores.Add(new SubmitLearningRecord.Data.SkillScores() { skillId = 16, score = ave.ToString() });
            }


            var scoreArray = scores.ToArray();
            /*if (null == scoreArray || 0 == scoreArray.Length)
            {
                failed?.Invoke("Record collection cannot be empty.");
                return;
            }*/

            System.TimeSpan span = new System.TimeSpan(0, 25, 0);

            var dt = new System.DateTime(TimeSpan.TicksPerMillisecond * durationMS);
            var durationWebUTC = dt.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'").Split('.')[0];
            SubmitLearningRecord submitLearningRecordRequest = new SubmitLearningRecord()
            {
                data = new SubmitLearningRecord.Data
                {
                    moduleId = xScenarioId,
                    duration = durationWebUTC,
                    userLearningRecords = scoreArray.ToList(),
                    skillScores = skillScores.ToList()
                }
            };

            if (!string.IsNullOrEmpty(activePinCode))
                submitLearningRecordRequest.data.pinCode = activePinCode;

            RESTService.Send(submitLearningRecordRequest, success, failed);
        }

        /// <summary>
        /// Download scenario record config by id. 
        /// </summary>
        /// <param name="scenarioId">Scenario config id</param>
        /// <param name="success">Action runs when submit success. Params: GetConfig.Response - config data including a list of records.</param>
        /// <param name="failed">Action runs when submit fail, including http and network errors. Params: string - the error message.</param>
        public static void GetConfig(string scenarioId, System.Action<GetConfig.Response> success = null, System.Action<string> failed = null)
        {
            GetConfig getConfigRequest = new GetConfig(scenarioId);
            RESTService.Send(getConfigRequest, success, failed);
        }

        /// <summary>
        /// Get all scenario. 
        /// </summary>       
        /// <param name="success">Action runs when submit success. Params: GetConfig.Response - config data including a list of records.</param>
        /// <param name="failed">Action runs when submit fail, including http and network errors. Params: string - the error message.</param>
        public static void GetAllScenarios(System.Action<GetAllScenarios.Response> success = null, System.Action<string> failed = null)
        {
            GetAllScenarios getAllScenariosRequest = new GetAllScenarios();
            RESTService.Send(getAllScenariosRequest, success, failed);
        }

        /// <summary>
        /// Create new session. 
        /// </summary>       
        public static void CreateSession(string scenarioId, System.Action<CreateSession.Response> success = null, System.Action<string> failed = null)
        {
            CreateSession createSessionRequest = new CreateSession(scenarioId);
            RESTService.Send(createSessionRequest, success, failed);
        }

        /// <summary>
        /// Join session via pincode. 
        /// </summary>       
        public static void JoinSession(string scenarioId, string pinCode, System.Action<CreateSession.Response> success = null, System.Action<string> failed = null)
        {
            JoinSession joinSessionRequest = new JoinSession(scenarioId, pinCode);
            RESTService.Send(joinSessionRequest, (s1) => {
                activePinCode = pinCode;
                if (success != null)
                    success.Invoke(s1);
            }, failed);
        }

        /// <summary>
        /// Update session with target id
        /// </summary> 
        public static void UpdateSessionStatus(int sessionId, UpdateSessionStatus.Status status, System.Action<AbstractAPI.EmptyResponse> success = null, System.Action<string> failed = null)
        {
            UpdateSessionStatus updateSessionStatusRequest = new UpdateSessionStatus(sessionId, status);
            RESTService.Send(updateSessionStatusRequest, success, failed);
        }

        /// <summary>
        /// Update current session
        /// only works using skills vr SSO library app
        /// </summary> 
        public static void UpdateCurrentSessionStatus(UpdateSessionStatus.Status status, System.Action<AbstractAPI.EmptyResponse> success = null, System.Action<string> failed = null)
        {
            ECAPI.TryFetchAccessTokenFromIntent();
            if (ECAPI.HasLoginToken())
            {               
                UpdateSessionStatus updateSessionStatusRequest = new UpdateSessionStatus(int.Parse(TryFetchSessionIdFromIntent()), status);
                RESTService.Send(updateSessionStatusRequest, success, failed);
                return;
            }

            throw new FieldAccessException("Access token is missing, make sure you are logging in using Skills VR SSO?");
        }
    }
}
