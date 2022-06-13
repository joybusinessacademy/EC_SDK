﻿using SkillsVR.EnterpriseCloudSDK.Data;
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
        private string user;
        private string password;
        private int organisationId;
        private string userRoleName;
        private string userProjectName;
        private int scenarioId;

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
            public UnityEventInt onRecordStateChanged = new UnityEventInt();
            public ECRecordCollectionAsset.RecordBoolScoreChangeEvent onRecordBoolScoreChanged = new ECRecordCollectionAsset.RecordBoolScoreChangeEvent();
            public ECRecordCollectionAsset.RecordBoolScoreChangeEvent onGetRecordBoolScore = new ECRecordCollectionAsset.RecordBoolScoreChangeEvent();
            public ECRecordCollectionAsset.RecordBoolScoreChangeEvent onSetRecordBoolScore = new ECRecordCollectionAsset.RecordBoolScoreChangeEvent();
            public EventHandlerGroup setScoreResultEvents = new EventHandlerGroup();
        }

        public ResponsedEventHandlerGroup loginEvents = new ResponsedEventHandlerGroup();
        public ResponsedEventHandlerGroup loginOrganisationEvents = new ResponsedEventHandlerGroup();
        public ResponsedEventHandlerGroup getConfigEvents = new ResponsedEventHandlerGroup();
        public ResponsedEventHandlerGroup submitScoreEvents = new ResponsedEventHandlerGroup();
        public RecordEventHandlerGroup recordEvents = new RecordEventHandlerGroup();

        protected ECRecordCollectionAsset recordAsset;
        private void Start()
        {
            recordAsset = ECRecordCollectionAsset.GetECRecordAsset();
            if (null != recordAsset)
            {
                recordAsset.onGameScoreBoolChanged.AddListener(OnRecordBoolScoreChangedCallback);
                recordAsset.onResetAllGameScores.AddListener(recordEvents.onResetAllGameScores.Invoke);

                getConfigEvents?.onSuccess?.Invoke();

                user = recordAsset.user;
                password = recordAsset.password;

                organisationId = recordAsset.organisationId;
                userRoleName = recordAsset.userRoleName;
                userProjectName = recordAsset.userProjectName;

                if (silentLoginUseAssetAccount && !ECAPI.HasLoginToken())
                {
                    Login();
                }
            }
        }

        private void OnEnable()
        {
            RefreshLoginState();
        }

        private void OnDestroy()
        {
            if (null != recordAsset)
            {
                recordAsset.onGameScoreBoolChanged.RemoveListener(OnRecordBoolScoreChangedCallback);
                recordAsset.onResetAllGameScores.RemoveListener(recordEvents.onResetAllGameScores.Invoke);
            }
        }

        public void RefreshLoginState()
        {
            loginEvents?.TriggerEvent(ECAPI.HasLoginToken());
        }

        public void SetUser(string userName)
        {
            user = userName;
        }

        public void SetScenarioId(int id)
        {
            scenarioId = id;
        }
        public void SetScenarioId(string id)
        {
            int.TryParse(id, out scenarioId);
        }

        public void SetPassworkd(string userPassword)
        {
            password = userPassword;
        }
        private void OnRecordBoolScoreChangedCallback(int id, bool isOn)
        {
            recordEvents.onRecordStateChanged?.Invoke(id);
            recordEvents.onRecordBoolScoreChanged?.Invoke(id, isOn);
        }

        public void Login()
        {
            ECAPI.Login(user, password,
                (resp) => { loginEvents.TriggerResponse(resp); Log("Login Success"); },
                (error) => { loginEvents.TriggerEvent(false, error); LogError("Login Fail: " + error); });
        }

        public void LoginOrganisation()
        {
            ECAPI.LoginOrganisation(
                organisationId,
                userRoleName,
                userProjectName,
                (resp) => {loginOrganisationEvents.TriggerResponse(resp); Log("Login Organisation Success"); },
                (error) => { loginOrganisationEvents.TriggerEvent(false, error); LogError("Login Organisation Fail: " + error); });
        }

        public void GetConfig()
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

        protected string ProcessConfigResponse(int cfgId, GetConfig.Response response)
        {
            string error = null;
            if (null == response || null == response.data || 0 == response.data.Length)
            {
                error = "Get config response has no data. Scenario id " + cfgId;
                LogError(error);
                return error;
            }
            if (null == recordAsset)
            {
                error = NO_ASSET_ERROR;
                LogError(error);
                return error;
            }
            recordAsset.scenarioId = cfgId;
            recordAsset.AddRange(response.data);
            Log("GetConfig " + cfgId + " Success");
            return null;
        }

        public bool SetGameScoreBool(int id, bool isOn)
        {
            if (null == recordAsset)
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

        public bool GetGameScoreBool(int id)
        {
            if (null == recordAsset)
            {
                return false;
            }
            return recordAsset.GetGameScoreBool(id);
        }

        public void ResetGameScore()
        {
            if (null == recordAsset)
            {
                LogError(NO_ASSET_ERROR);
                return;
            }
            recordAsset.ResetUserScores();
        }

        public void SubmitScore()
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

        private int setScoreId;
        private bool setScoreValue;

        public void SetScoreIdAction(int id)
        {
            setScoreId = id;
        }

        public void SetScoreIdAction(string id)
        {
            int.TryParse(id, out setScoreId);
        }

        public void SetScoreValueAction(bool value)
        {
            setScoreValue = value;
        }

        public void SetScoreInvokeAction()
        {
            SetGameScoreBool(setScoreId, setScoreValue);
        }
        public void GetScoreInvokeAction()
        {
            bool value = GetGameScoreBool(setScoreId);
            recordEvents?.onGetRecordBoolScore?.Invoke(setScoreId, value);
        }

        public void LogScore(int id, bool value)
        {
            Log("Score " + id + " ==> " + value);
        }
    }
}
