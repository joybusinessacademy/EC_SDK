using SkillsVR.EnterpriseCloudSDK.Networking;
using SkillsVR.EnterpriseCloudSDK.Networking.API;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace SkillsVR.EnterpriseCloudSDK.Data
{
    [Serializable]
    public class ECRecordCollectionAssetConfig
    {
        public string name => string.Join("_", scenarioId, null == domain ? "" : domain, (null == loginData || null== loginData.userName) ? "" : loginData.userName);
        public string domain;

        public SSOLoginData loginData = new SSOLoginData();

        public int scenarioId;
        public long durationMS;
        public List<ECRecordContent> managedRecords = new List<ECRecordContent>();

    }
    public class ECRecordCollectionAsset : ScriptableObject
    {
        public const string ASSET_PATH = "Assets";
        public const string RESOURCE_PATH = "Resources";
        public const string ASSET_FILE_NAME = "ECRecordConfig.asset";

        private static ECRecordCollectionAsset instance;

        public List<ECRecordCollectionAssetConfig> managedConfigs = new List<ECRecordCollectionAssetConfig>();
        
        [SerializeField, HideInInspector]
        public int currentConfigIndex = 0;
        public ECRecordCollectionAssetConfig currentConfig
        {
            get
            {
                if (null == managedConfigs)
                {
                    managedConfigs = new List<ECRecordCollectionAssetConfig>();
                }
                if (0 == managedConfigs.Count)
                {
                    managedConfigs.Add(new ECRecordCollectionAssetConfig());
                    currentConfigIndex = 0;
                }

                if (0 > currentConfigIndex || managedConfigs.Count <= currentConfigIndex)
                {
                    currentConfigIndex = 0;
                }

                return managedConfigs[currentConfigIndex];
            }
        }
        
        [Serializable] public class RecordBoolScoreChangeEvent : UnityEvent<int,bool> { }

        public RecordBoolScoreChangeEvent onGameScoreBoolChanged = new RecordBoolScoreChangeEvent();
        public UnityEvent onResetAllGameScores = new UnityEvent();
        public static ECRecordCollectionAsset GetECRecordAsset()
        {
            if (null != instance)
            {
                return instance;
            }
            string fileNameForResources = Path.GetFileNameWithoutExtension(ASSET_FILE_NAME);
            string fileResourcePath = fileNameForResources;
            instance = Resources.Load<ECRecordCollectionAsset>(fileResourcePath);
            if (instance == null)
            {
                var assets = Resources.LoadAll<ECRecordCollectionAsset>("").ToList();
                if (assets.Count > 0)
                    instance = assets.First();
            }
            return instance;
        }

        public bool Contains(int id)
        {
            return null != currentConfig.managedRecords.Find(x=>x.id == id);
        }

        public bool SetGameScoreBool(int id, bool isOn, Action<string> onFail = null)
        {
            if (null == onFail)
            {
                onFail = Debug.LogError;
            }
            var record = currentConfig.managedRecords.Find(x => id == x.id);
            if (null == record)
            {
                onFail.Invoke("No record found with id " + id);
                return false;
            }
            return SetGameScoreBool(record, isOn, onFail);
        }

        public bool SetGameScoreBool(ECRecordContent record, bool isOn, Action<string> onFail = null)
        {
            if (null == onFail)
            {
                onFail = Debug.LogError;
            }
            if (null == record)
            {
                onFail?.Invoke("Record cannot be null.");
                return false;
            }
            if (!record.isScoreTypeBool)
            {
                onFail.Invoke("Record " + record.id + " is not a boolean score record.");
                return false;
            }
            if (isOn == record.gameScoreBool)
            {
                return true;
            }
            record.gameScoreBool = isOn;
            onGameScoreBoolChanged?.Invoke(record.id, isOn);
            return true;
        }

        public bool GetGameScoreBool(int id)
        {
            var record = currentConfig.managedRecords.Find(x => id == x.id);
            return null == record ? false : record.gameScoreBool;
        }

        public void ResetUserScores()
        {
            if (null == currentConfig.managedRecords)
            {
                return;
            }
            foreach (var record in currentConfig.managedRecords)
            {
                if (null == record)
                {
                    continue;
                }
                if (record.isScoreTypeBool)
                {
                    SetGameScoreBool(record, false);
                }
                
            }
            onResetAllGameScores?.Invoke();
        }

        public void GetConfig(Action<GetConfig.Response> success = null, Action<string> failed = null)
        {
            TryLoginThen(() => { ECAPI.GetConfig(this.currentConfig.scenarioId, success, failed); }, failed);
        }

        public void SubmitUserScore(Action<AbstractAPI.EmptyResponse> success = null, Action<string> failed = null)
        {
            TryLoginThen(() => { ECAPI.SubmitUserLearningRecord(currentConfig.scenarioId, currentConfig.durationMS, currentConfig.managedRecords, success, failed); }, failed);
        }

        public void TryLoginThen(Action actionAfterLogin, Action<string> onError)
        {

            if (!ECAPI.HasLoginToken())
            {
                ECAPI.domain = currentConfig.domain;
                if (string.IsNullOrWhiteSpace(currentConfig.loginData.userName) || string.IsNullOrWhiteSpace(currentConfig.loginData.password))
                {
                    onError?.Invoke("User or password cannot be null or empty.");
                    return;
                }
                Action<string> loginFailedAction = (error) => { onError?.Invoke(error); }; 
                ECAPI.Login(currentConfig.loginData, (loginResp) =>
                {
                    actionAfterLogin?.Invoke();
                }, loginFailedAction);
            }
            else
            {
                actionAfterLogin?.Invoke();
            }
        }

        public void PrintRecords()
        {
            string info = "Scenario " + currentConfig.scenarioId + "\r\n";
            foreach (var record in currentConfig.managedRecords)
            {
                info += record.PrintInLine();
            }
            Debug.Log(info);
        }

        public void OrderManagedRecords()
        {
            currentConfig.managedRecords = ECRecordUtil.OrderContents(currentConfig.managedRecords);
        }

        public void AddRange(IEnumerable<ECRecordContent> contentCollection)
        {
            if (null == contentCollection)
            {
                return;
            }
            currentConfig.managedRecords.Clear();
            currentConfig.managedRecords.AddRange(contentCollection);
            OrderManagedRecords();
        }
    }
}
