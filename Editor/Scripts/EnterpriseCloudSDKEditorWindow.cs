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

        bool interactable = true;
        [MenuItem("Window/SkillsVR Enterprise Cloud SDK")]
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
            ECAPI.environment = (ECAPI.Environment)EditorGUILayout.EnumPopup("Test Env", ECAPI.environment);
            GUI.enabled = interactable;
            EditorGUILayout.LabelField(RESTCore.domain);
            EditorGUILayout.EndHorizontal();

            recordAsset.currentConfig.user = EditorGUILayout.TextField("Username:", recordAsset.currentConfig.user);
            recordAsset.currentConfig.password = EditorGUILayout.PasswordField("Password:", recordAsset.currentConfig.password);

            GUI.enabled = interactable && !string.IsNullOrWhiteSpace(recordAsset.currentConfig.user) && !string.IsNullOrWhiteSpace(recordAsset.currentConfig.password);
            if (GUILayout.Button("Login"))
            {
                SendLogin();
            }
            GUI.enabled = interactable;

            recordAsset.currentConfig.scenarioId = EditorGUILayout.IntField("Scenario ID:", recordAsset.currentConfig.scenarioId);

            if (ECAPI.HasLoginToken())
            {
                GUI.enabled = interactable && 0 < recordAsset.currentConfig.scenarioId;
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

        private void LogError(string error)
        {
            interactable = true;
            Debug.LogError(error);
        }

        private void SendLogin()
        {
            interactable = false;
            ECAPI.Login(recordAsset.currentConfig.user, recordAsset.currentConfig.password, SendLoginOrganisation, LogError);
        }


        private void SendLoginOrganisation(Login.Response response)
        {
            try
            {
                var organisation = response.data.organisations[0];
                recordAsset.currentConfig.organisationId = int.Parse(organisation.id);
                recordAsset.currentConfig.userRoleName = organisation.roles[0].key;
                recordAsset.currentConfig.userProjectName = organisation.name;
            }
            catch(Exception e)
            {
                Debug.LogException(e);
            }
            
            interactable = false;
            ECAPI.LoginOrganisation(recordAsset.currentConfig.organisationId, recordAsset.currentConfig.userRoleName, recordAsset.currentConfig.userProjectName, RecieveLoginOrganisation, LogError);
        }

        private void RecieveLoginOrganisation(Login.Response response)
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
            ECAPI.SubmitUserLearningRecord(recordAsset.currentConfig.scenarioId, recordAsset.currentConfig.managedRecords, LearningRecordResponse, LogError);
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



