using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SkillsVR.EnterpriseCloudSDK.Editor.Editors;
using SkillsVR.EnterpriseCloudSDK.Data;
using SkillsVR.EnterpriseCloudSDK.Networking.API;
using System;
using SkillsVR.EnterpriseCloudSDK.Networking;

namespace SkillsVR.EnterpriseCloudSDK.Editor
{
    public class EnterpriseCloudSDKEditorWindow : EditorWindow
    {
        List<ECRecordContentEditorWidget> widgets = new List<ECRecordContentEditorWidget>();

        ECRecordCollectionAsset recordAsset = null;

        Vector2 scrollViewPos;

        SerializedProperty recordAssetSerializedProperty = null;

        protected bool interactable = true;

        protected bool enableEditScope = false;

        [MenuItem("Skills Node/Configure Enterprise Cloud", false, 2)]
        public static void ShowWindow()
        {
            EditorWindow.GetWindow<EnterpriseCloudSDKEditorWindow>("SkillsVR Enterprise Cloud");
        }

        private void OnEnable()
        {
            recordAsset = ECRecordCollectionAssetEditor.CreateOrLoadAsset();
            widgets.Clear();
            SerializedObject serializedObject = new SerializedObject(this);
            recordAssetSerializedProperty = serializedObject.FindProperty(nameof(recordAsset));
            interactable = true;
        }

        private void OnDisable()
        {
            SaveRecordAsset();
        }

        void OnGUI()
        {
            if (null == recordAsset)
            {
                return;
            }
            EditorGUILayout.Space(10);

            EditorGUILayout.BeginVertical();

            GUI.enabled = false;
            EditorGUILayout.ObjectField("Record Asset: ", recordAsset, recordAsset.GetType(), false);
            GUI.enabled = interactable;

            EditorGUILayout.BeginHorizontal();
            GUI.enabled = !EditorApplication.isPlayingOrWillChangePlaymode;

            if (recordAsset.managedConfigs.Count > 1)
            {
                List<string> nameList = new List<string>();
                foreach (var item in recordAsset.managedConfigs)
                {
                    nameList.Add(item.name.Replace("/", "").Replace("\\", ""));
                }
                recordAsset.currentConfigIndex = EditorGUILayout.Popup(new GUIContent("Switch Config"), recordAsset.currentConfigIndex, nameList.ToArray());
            }
            

            GUI.enabled = interactable;
            EditorGUILayout.EndHorizontal();

            recordAsset.currentConfig.loginData = DrawLoginDataGUI(recordAsset.currentConfig.loginData);

            GUI.enabled = interactable && recordAsset.currentConfig.loginData.IsValid();
            if (GUILayout.Button("Login"))
            {
                SendLogin();
            }
            GUI.enabled = interactable;

            recordAsset.currentConfig.scenarioId = EditorGUILayout.TextField("Scenario ID:", recordAsset.currentConfig.scenarioId);

            if (ECAPI.HasLoginToken())
            {
                GUI.enabled = interactable && !string.IsNullOrEmpty(recordAsset.currentConfig.scenarioId);
                if (GUILayout.Button("Get Config"))
                {
                    SendGetConfig();
                }
                GUI.enabled = interactable;
            }
            else
            {
                EditorGUILayout.HelpBox("Display records from local cached . Please login to enable submit and more functions.", MessageType.Warning);
            }

            if (recordAsset.currentConfig.managedRecords.Count > 0)
            {
                if (null == widgets || 0 == widgets.Count)
                {
                    widgets = ECRecordContentEditorWidget.GetEditorRenderingWidgetListFromRecordCollection(recordAsset.currentConfig.managedRecords);
                }
                scrollViewPos = EditorGUILayout.BeginScrollView(scrollViewPos, GUILayout.ExpandHeight(true));
                foreach(var item in widgets)
                {
                    item?.Draw();
                    GUILayout.Space(20);
                }
                EditorGUILayout.EndScrollView();

                if (GUILayout.Button("Save Changes"))
                {
                    SaveRecordAsset();
                }

                if (GUILayout.Button("Print Records"))
                {
                    recordAsset.PrintRecords();
                }
                if (GUILayout.Button("Reset User Scores"))
                {
                    recordAsset.ResetUserScores();
                }

                if (ECAPI.HasLoginToken() && GUILayout.Button("Submit"))
                {
                    SendLearningRecord();
                }
            }
            EditorGUILayout.EndVertical();
            GUI.enabled = true;
        }


        private SSOLoginData DrawLoginDataGUI(SSOLoginData loginData)
        {
            if (null == loginData)
            {
                loginData = new SSOLoginData();
            }

            loginData.selectedRegion = EditorGUILayout.Popup("Region", loginData.selectedRegion, SSOLoginData.regions);

            loginData.userName = EditorGUILayout.TextField("Username:", loginData.userName);
            loginData.password = EditorGUILayout.PasswordField("Password:", loginData.password);

            return loginData;
        }

        private string DrawScopeGUI(string scope)
        {
            enableEditScope = GUILayout.Toggle(enableEditScope, "Edit Scope");
            if (!enableEditScope)
            {
                return scope;
            }
            GUILayout.BeginHorizontal();
            GUILayout.Space(40);
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Reset to Default", GUILayout.ExpandWidth(false)))
            {
                scope = SSOLoginData.GetDefaultScopeString();
            }
            if (GUILayout.Button("Clear", GUILayout.ExpandWidth(false)))
            {
                scope = "";
            }
            GUILayout.EndHorizontal();
            List<string> scopeList = null;
            try
            {
                scopeList = scope.Split(' ').ToList();
            }
            catch {
                scopeList = new List<string>();
            }
            scopeList.RemoveAll(x => string.IsNullOrWhiteSpace(x));
            for (int i = 0; i < scopeList.Count; i++)
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("-", GUILayout.ExpandWidth(false)))
                {
                    scopeList[i] = "";
                    continue;
                }
                scopeList[i] = EditorGUILayout.TextField(scopeList[i]);
                GUILayout.EndHorizontal();
            }
            if (GUILayout.Button("+", GUILayout.ExpandWidth(true)))
            {
                scopeList.Add("NewScope");
            }
            scopeList.RemoveAll(x => string.IsNullOrWhiteSpace(x));
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            GUILayout.Space(20);
            return string.Join(" ", scopeList);
        }

        private void LogError(string error)
        {
            interactable = true;
            Debug.LogError(error);
        }

        private void SendLogin()
        {
            interactable = false;
            // configure my config now
            string targetId = string.Empty;

            switch (SSOLoginData.regions[recordAsset.currentConfig.loginData.selectedRegion])
            {
                case "US":
                    targetId = "prod-us";
                    break;
                case "AU":
                    targetId = "prod-au";
                    break;
                case "US-Test":
                    targetId = "test-us";
                    break;
                case "AU-Test":
                    targetId = "test-au";
                    break;
                case "AU-Dev":
                    targetId = "dev-au";
                    break;
            }

            var config = SkillsVR.EnterpriseCloudSDK.Editor.Networking.ConfigService.Get(targetId);

            recordAsset.currentConfig.loginData.clientId = config.clientId;
            recordAsset.currentConfig.loginData.loginUrl = config.ropcUrl;
            recordAsset.currentConfig.loginData.scope = config.scope;
            recordAsset.currentConfig.domain = config.domain;

            PlayerPrefs.SetString("OCAPIM_SUB_KEY", config.subscriptionKey);
            ECAPI.domain = config.domain;
            ECAPI.Login(recordAsset.currentConfig.loginData, OnLoginSuccess, LogError);
            PlayerPrefs.Save();
        }

        private void OnLoginSuccess(SSOLoginResponse response)
        {
            interactable = true;
        }

        private void SendGetConfig()
        {
            interactable = false;
            ECAPI.GetConfig(recordAsset.currentConfig.scenarioId, RecieveConfigResponse, LogError);
        }

        private void RecieveConfigResponse(GetConfig.Response obj)
        {
            widgets.Clear();
            recordAsset.currentConfig.managedRecords.Clear();

            if (null == obj || null == obj.data || 0 == obj.data.Length)
            {
                Debug.Log("No context loaded.");
                SaveRecordAsset();
                interactable = true;
                return;
            }

            recordAsset.AddRange(obj.data);
            SaveRecordAsset();

            widgets = ECRecordContentEditorWidget.GetEditorRenderingWidgetListFromRecordCollection(recordAsset.currentConfig.managedRecords);
            interactable = true;
        }

        public void SendLearningRecord()
        {
            recordAsset.SubmitUserScore(null, Debug.LogError);
        }

        private void LearningRecordResponse(AbstractAPI.EmptyResponse obj)
        {
        }

        public void SaveRecordAsset()
        {
            EditorUtility.CopySerialized(recordAsset, recordAsset);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}



