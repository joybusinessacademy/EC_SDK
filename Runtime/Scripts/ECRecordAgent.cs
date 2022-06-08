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
        public string user;
        public string password;
        public int scenarioId;

        [Serializable] public class UnityEventString : UnityEvent<string> { }
        [Serializable] public class UnityEventBool : UnityEvent<bool> { }
        [Serializable] public class UnityEventInt : UnityEvent<int> { }

        public bool silentLoginUseAssetAccount = false;


        public UnityEventString onLogText = new UnityEventString();

        public UnityEvent onLoginSuccess = new UnityEvent();
        public UnityEventString onLoginFail = new UnityEventString();
        public UnityEventBool onLoginStateChanged = new UnityEventBool();

        public UnityEvent onGetConfigSuccess = new UnityEvent();
        public UnityEventString onGetConfigFail = new UnityEventString();
        public UnityEventBool onGetConfigStateChanged = new UnityEventBool();

        public UnityEventInt onRecordStateChanged = new UnityEventInt();
        public ECRecordCollectionAsset.RecordBoolScoreChangeEvent onRecordBoolScoreChanged = new ECRecordCollectionAsset.RecordBoolScoreChangeEvent();
        public ECRecordCollectionAsset.RecordBoolScoreChangeEvent onGetRecordBoolScore = new ECRecordCollectionAsset.RecordBoolScoreChangeEvent();

        public UnityEvent onSubmitScoreSuccess = new UnityEvent();
        public UnityEventString onSubmitScoreFail = new UnityEventString();
        public UnityEventBool onSubmitScoreStateChanged = new UnityEventBool();

        protected ECRecordCollectionAsset recordAsset;
        private void Start()
        {
            recordAsset = ECRecordCollectionAsset.GetECRecordAsset();
            if (null != recordAsset)
            {
                recordAsset.onGameScoreBoolChanged.AddListener(OnRecordBoolScoreChangedCallback);
                onGetConfigSuccess?.Invoke();

                if (silentLoginUseAssetAccount)
                {
                    user = recordAsset.user;
                    password = recordAsset.password;
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
            }
        }

        public void RefreshLoginState()
        {
            onLoginStateChanged?.Invoke(ECAPI.HasLoginToken());
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
            onRecordBoolScoreChanged?.Invoke(id, isOn);
            onRecordStateChanged?.Invoke(id);
        }

        public void Login()
        {
            ECAPI.Login(user, password,
                (resp) => { LoginOrganisation(); },
                (error) => { onLoginFail.Invoke(error); onLoginStateChanged.Invoke(false); LogError(error); });
        }

        public void LoginOrganisation()
        {
            ECAPI.LoginOrganisation(
                (resp) => { onLoginSuccess?.Invoke(); onLoginStateChanged.Invoke(true); Log("Login Success"); },
                (error) => { onLoginFail.Invoke(error); onLoginStateChanged.Invoke(false); LogError(error); });
        }

        public void GetConfig()
        {
            ECAPI.GetConfig(scenarioId,
                (resp) =>
                {
                    string error = ProcessConfigResponse(scenarioId, resp);
                    if (null == error)
                    {
                        onGetConfigSuccess?.Invoke();
                    }
                    else
                    {
                        onGetConfigFail.Invoke(error);
                    }
                    onGetConfigStateChanged.Invoke(null == error);
                },
                (error) => { onGetConfigFail.Invoke(error); onGetConfigStateChanged.Invoke(false); LogError(error); });
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
                LogError(NO_ASSET_ERROR);
                return false;
            }
            return recordAsset.SetGameScoreBool(id, isOn);
        }

        public bool GetGameScoreBool(int id)
        {
            if (null == recordAsset)
            {
                LogError(NO_ASSET_ERROR);
                onGetRecordBoolScore?.Invoke(id, false);
                return false;
            }
            onGetRecordBoolScore?.Invoke(id, recordAsset.GetGameScoreBool(id));
            return true;
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
               (resp) => { onSubmitScoreSuccess?.Invoke(); onSubmitScoreStateChanged.Invoke(true); Log("Submit Score Success"); },
               (error) => { onSubmitScoreFail.Invoke(error); onSubmitScoreStateChanged.Invoke(false); LogError(error); });
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
            bool success = SetGameScoreBool(setScoreId, setScoreValue);
            if (!success)
            {
                LogError("Id " + setScoreId + " not found");
            }
            else
            {
                Log("Set record " + setScoreId + ": " + setScoreValue);
            }
        }

    }
}
