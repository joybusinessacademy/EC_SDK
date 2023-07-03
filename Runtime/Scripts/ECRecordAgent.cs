using SkillsVR.EnterpriseCloudSDK.Data;
using SkillsVR.EnterpriseCloudSDK.Networking.API;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace SkillsVR.EnterpriseCloudSDK
{
    public class ECRecordAgent : MonoBehaviour
    {
        public const string NO_ASSET_ERROR = "No EC record asset found in resource. Create in editor with Window->Login first.";
        private SSOLoginData loginData = new SSOLoginData();
        private string scenarioId;

        [Serializable] public class UnityEventString : UnityEvent<string> { }
        [Serializable] public class UnityEventBool : UnityEvent<bool> { }
        [Serializable] public class UnityEventInt : UnityEvent<int> { }

        [Serializable] public class UnityEventResponse : UnityEvent<AbstractResponse> { }

        public bool silentLoginUseAssetAccount = false;


        public UnityEventString onLogText = new UnityEventString();

        [Serializable]
        public class EventHandlerGroup
        {
            public UnityEvent onSuccess = new UnityEvent();
            public UnityEventString onError = new UnityEventString();
            public UnityEventBool onStateChanged = new UnityEventBool();
            
            public void TriggerEvent(bool success, string msg = null)
            {
                if (success)
                {
                    onSuccess?.Invoke();
                }
                else
                {
                    onError?.Invoke(msg);
                }
                onStateChanged?.Invoke(success);
            }

            

            public void TriggerError(string error)
            {
                TriggerEvent(false, error);
            }
        }

        [Serializable]
        public class ResponsedEventHandlerGroup : EventHandlerGroup
        {
            public UnityEventResponse onRespoinseData = new UnityEventResponse();

            public void TriggerResponse(AbstractResponse response)
            {
                TriggerEvent(true, null);
                if  (null != response)
                {
                    onRespoinseData?.Invoke(response);
                }
            }
        }

        [Serializable]
        public class RecordEventHandlerGroup
        {
            public UnityEvent onResetAllGameScores = new UnityEvent();
            public UnityEventString onRecordStateChanged = new UnityEventString();
            public ECRecordCollectionAsset.RecordBoolScoreChangeEvent onRecordBoolScoreChanged = new ECRecordCollectionAsset.RecordBoolScoreChangeEvent();
            public ECRecordCollectionAsset.RecordBoolScoreChangeEvent onGetRecordBoolScore = new ECRecordCollectionAsset.RecordBoolScoreChangeEvent();
            public ECRecordCollectionAsset.RecordBoolScoreChangeEvent onSetRecordBoolScore = new ECRecordCollectionAsset.RecordBoolScoreChangeEvent();
            public EventHandlerGroup setScoreResultEvents = new EventHandlerGroup();
        }

        public ResponsedEventHandlerGroup loginEvents = new ResponsedEventHandlerGroup();
        public ResponsedEventHandlerGroup getConfigEvents = new ResponsedEventHandlerGroup();
        public ResponsedEventHandlerGroup submitScoreEvents = new ResponsedEventHandlerGroup();
        public RecordEventHandlerGroup recordEvents = new RecordEventHandlerGroup();

        protected ECRecordCollectionAsset recordAsset;
        protected ECRecordCollectionAssetConfig recordAssetConfig;
        private void Start()
        {
            recordAsset = ECRecordCollectionAsset.GetECRecordAsset();
            if (null != recordAsset)
            {
                recordAssetConfig = recordAsset.currentConfig;
                ECAPI.domain = recordAsset.currentConfig.domain;
                recordAsset.onGameScoreBoolChanged.AddListener(OnRecordBoolScoreChangedCallback);
                recordAsset.onResetAllGameScores.AddListener(recordEvents.onResetAllGameScores.Invoke);

                getConfigEvents?.onSuccess?.Invoke();

                loginData = new SSOLoginData(recordAssetConfig.loginData);

                if (silentLoginUseAssetAccount && !ECAPI.HasLoginToken())
                {
                    ECLogin();
                }
            }
        }

        private void OnEnable()
        {
            ECRefreshLoginState();
        }

        private void OnDestroy()
        {
            if (null != recordAsset)
            {
                recordAsset.onGameScoreBoolChanged.RemoveListener(OnRecordBoolScoreChangedCallback);
                recordAsset.onResetAllGameScores.RemoveListener(recordEvents.onResetAllGameScores.Invoke);
            }
        }

        public void ECRefreshLoginState()
        {
            loginEvents?.TriggerEvent(ECAPI.HasLoginToken());
        }

        public void ECSetLoginUser(string userName)
        {
            loginData.userName = userName;
        }

        public void ECSetLoginPassword(string userPassword)
        {
            loginData.password = userPassword;
        }

        public void ECSetScenarioId(string id)
        {
            scenarioId = id;
        }

        public void ECSetLoginClientId(string id)
        {
            loginData.clientId = id;
        }

        public void ECSetLoginScope(string scope)
        {
            loginData.scope = scope;
        }

        public void ECSetLoginUrl(string url)
        {
            loginData.loginUrl = url;
        }

        private void OnRecordBoolScoreChangedCallback(string id, bool isOn)
        {
            recordEvents.onRecordStateChanged?.Invoke(id);
            recordEvents.onRecordBoolScoreChanged?.Invoke(id, isOn);
        }

        public void ECLogin()
        {
            ECAPI.Login(loginData,
                (resp) => { loginEvents.TriggerResponse(resp); Log("Login Success"); },
                (error) => { loginEvents.TriggerEvent(false, error); LogError("Login Fail: " + error); });
        }

        public void ECGetConfig()
        {
            ECAPI.GetConfig(scenarioId,
                (resp) =>
                {
                    string error = ProcessConfigResponse(scenarioId, resp);
                    bool success = null == error;
                    if (success)
                    {
                        getConfigEvents.TriggerResponse(resp);
                    }
                    else
                    {
                        getConfigEvents.TriggerError(error);
                    }
                },
                (error) => { getConfigEvents.TriggerEvent(false, error); LogError("Get Config Fail: " + error); });
        }

        protected string ProcessConfigResponse(string cfgId, GetConfig.Response response)
        {
            string error = null;
            if (null == response || null == response.data || 0 == response.data.Length)
            {
                error = "Get config response has no data. Scenario id " + cfgId;
                LogError(error);
                return error;
            }
            if (null == recordAssetConfig || null == recordAsset)
            {
                error = NO_ASSET_ERROR;
                LogError(error);
                return error;
            }
            recordAssetConfig.scenarioId = cfgId;
            recordAsset.AddRange(response.data);
            Log("GetConfig " + cfgId + " Success");
            return null;
        }

        public bool ECSetGameScoreBoolFromNode(object[] param)
        {
            return ECSetGameScoreBool(((string)param[0]).Trim(), (bool)param[1]);
        }

        public bool ECSetGameScoreBool(string id, bool isOn)
        {
            if (null == recordAssetConfig || null == recordAsset)
            {
                recordEvents.setScoreResultEvents.TriggerEvent(false, NO_ASSET_ERROR);
                LogError("Set record " + setScoreId + " Fail: " + NO_ASSET_ERROR);
                return false;
            }
            bool success = recordAsset.SetGameScoreBool(id, isOn, (error) =>
            {
                recordEvents.setScoreResultEvents.TriggerError(error);
                LogError("Set record " + setScoreId + " Fail: " + error);
            });
            if (success)
            {
                recordEvents.setScoreResultEvents.TriggerEvent(true);
                recordEvents.onSetRecordBoolScore.Invoke(id, isOn);
            }
            return success;
        }

        public bool ECGetGameScoreBool(string id)
        {
            if (null == recordAsset)
            {
                return false;
            }
            return recordAsset.GetGameScoreBool(id);
        }

        public void ECResetGameScore()
        {
            if (null == recordAsset)
            {
                LogError(NO_ASSET_ERROR);
                return;
            }
            recordAsset.ResetUserScores();
        }

        public void ECSubmitScore()
        {
            if (null == recordAsset)
            {
                LogError(NO_ASSET_ERROR);
                return;
            }
            recordAsset.SubmitUserScore(
               (resp) => { submitScoreEvents.TriggerResponse(resp); Log("Submit Score Success"); },
               (error) => { submitScoreEvents.TriggerEvent(false, error); LogError(error); });
        }

        public void ECSubmitScoreThenQuitApplication()
        {
            if (null == recordAsset)
            {
                LogError(NO_ASSET_ERROR);
                return;
            }
            recordAsset.SubmitUserScore(
               (resp) => { submitScoreEvents.TriggerResponse(resp); Log("Submit Score Success"); Application.Quit(); },
               (error) => { submitScoreEvents.TriggerEvent(false, error); LogError(error); Application.Quit(); } );
        }

        protected void Log(string msg)
        {
            if (string.IsNullOrWhiteSpace(msg))
            {
                return;
            }
            Debug.Log(msg);
            onLogText?.Invoke(msg);
        }

        protected void LogError(string msg)
        {
            if (string.IsNullOrWhiteSpace(msg))
            {
                return;
            }
            Debug.LogError(msg);
            onLogText?.Invoke("<color=red>" + msg + "</color>");
        }

        private string setScoreId;
        private bool setScoreValue;

        public void ECSetScoreIdAction(string id)
        {
            setScoreId = id;
        }


        public void ECSetScoreValueAction(bool value)
        {
            setScoreValue = value;
        }

        public void ECSetScoreInvokeAction()
        {
            ECSetGameScoreBool(setScoreId, setScoreValue);
        }
        public void ECGetScoreInvokeAction()
        {
            bool value = ECGetGameScoreBool(setScoreId);
            recordEvents?.onGetRecordBoolScore?.Invoke(setScoreId, value);
        }

        public void LogScore(int id, bool value)
        {
            Log("Score " + id + " ==> " + value);
        }
    }
}
