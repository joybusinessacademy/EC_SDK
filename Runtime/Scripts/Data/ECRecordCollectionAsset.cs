using SkillsVR.EnterpriseCloudSDK.Networking.API;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;

namespace SkillsVR.EnterpriseCloudSDK.Data
{
    public class ECRecordCollectionAsset : ScriptableObject
    {
        public const string ASSET_PATH = "Assets";
        public const string RESOURCE_PATH = "Resources";
        public const string ASSET_FILE_NAME = "ECRecordConfig.asset";

        private static ECRecordCollectionAsset instance;

        public string user;
        public string password;

        public int organisationId;
        public string userRoleName;
        public string userProjectName;

        public int scenarioId;
        public List<ECRecordContent> managedRecords = new List<ECRecordContent>();

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
            return instance;
        }

        public bool Contains(int id)
        {
            return null != managedRecords.Find(x=>x.id == id);
        }

        public bool SetGameScoreBool(int id, bool isOn, Action<string> onFail = null)
        {
            if (null == onFail)
            {
                onFail = Debug.LogError;
            }
            var record = managedRecords.Find(x => id == x.id);
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
            var record = managedRecords.Find(x => id == x.id);
            return null == record ? false : record.gameScoreBool;
        }

        public void ResetUserScores()
        {
            if (null == managedRecords)
            {
                return;
            }
            foreach (var record in managedRecords)
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
            TryLoginThen(() => { ECAPI.GetConfig(this.scenarioId, success, failed); }, failed);
        }

        public void SubmitUserScore(Action<AbstractAPI.EmptyResponse> success = null, Action<string> failed = null)
        {
            TryLoginThen(() => { ECAPI.SubmitUserLearningRecord(scenarioId, managedRecords, success, failed); }, failed);
        }

        public void TryLoginThen(Action actionAfterLogin, Action<string> onError)
        {

            if (!ECAPI.HasLoginToken())
            {
                if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(password))
                {
                    onError?.Invoke("User or password cannot be null or empty.");
                    return;
                }
                Action<string> loginFailedAction = (error) => { onError?.Invoke(error); };
                ECAPI.Login(user, password, (loginResp) =>
                {
                    ECAPI.LoginOrganisation(organisationId,userRoleName, userProjectName,
                        (loginOrgResp) =>
                        {
                            actionAfterLogin?.Invoke();
                        }, loginFailedAction);
                }, loginFailedAction);
            }
            else
            {
                actionAfterLogin?.Invoke();
            }
        }

        public void PrintRecords()
        {
            string info = "Scenario " + scenarioId + "\r\n";
            foreach (var record in managedRecords)
            {
                info += record.PrintInLine();
            }
            Debug.Log(info);
        }

        public void OrderManagedRecords()
        {
            managedRecords = ECRecordUtil.OrderContents(managedRecords);
        }

        public void AddRange(IEnumerable<ECRecordContent> contentCollection)
        {
            if (null == contentCollection)
            {
                return;
            }
            managedRecords.Clear();
            managedRecords.AddRange(contentCollection);
            OrderManagedRecords();
        }
    }
}
