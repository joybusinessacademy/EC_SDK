using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SkillsVR.EnterpriseCloudSDK.Editor.Editors;
using SkillsVR.EnterpriseCloudSDK.Data;
using SkillsVR.EnterpriseCloudSDK.Networking.API;
using System;

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

        void OnGUI()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.BeginVertical();

            GUI.enabled = false;
            EditorGUILayout.ObjectField("Record Asset: ", recordAsset, recordAsset.GetType(), false);
            GUI.enabled = interactable;


            recordAsset.user = EditorGUILayout.TextField("Username:", recordAsset.user);
            recordAsset.password = EditorGUILayout.TextField("Password:", recordAsset.password);

            GUI.enabled = interactable && !string.IsNullOrWhiteSpace(recordAsset.user) && !string.IsNullOrWhiteSpace(recordAsset.password) && !ECAPI.HasLoginToken();
            if (GUILayout.Button("Login"))
            {
                SendLogin();
            }
            GUI.enabled = interactable;

            recordAsset.scenarioId = EditorGUILayout.IntField("Scenario ID:", recordAsset.scenarioId);

            if (ECAPI.HasLoginToken())
            {
                GUI.enabled = interactable && 0 < recordAsset.scenarioId;
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

            if (recordAsset.managedRecords.Count > 0)
            {
                if (null == widgets || 0 == widgets.Count)
                {
                    widgets = ECRecordContentEditorWidget.GetEditorRenderingWidgetListFromRecordCollection(recordAsset.managedRecords);
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
            ECAPI.Login(recordAsset.user, recordAsset.password, SendLoginOrganisation, LogError);
        }


        private void SendLoginOrganisation(Login.Response response)
        {
            interactable = false;
            ECAPI.LoginOrganisation( RecieveLoginOrganisation, LogError);
        }

        private void RecieveLoginOrganisation(Login.Response response)
        {
            interactable = true;
        }

        private void SendGetConfig()
        {
            interactable = false;
            ECAPI.GetConfig(recordAsset.scenarioId, RecieveConfigResponse, LogError);
        }

        private void RecieveConfigResponse(GetConfig.Response obj)
        {
            widgets.Clear();
            recordAsset.managedRecords.Clear();

            if (null == obj || null == obj.data || 0 == obj.data.Length)
            {
                Debug.Log("No context loaded.");
                SaveRecordAsset();
                return;
            }

            recordAsset.AddRange(obj.data);
            SaveRecordAsset();

            widgets = ECRecordContentEditorWidget.GetEditorRenderingWidgetListFromRecordCollection(recordAsset.managedRecords);
            interactable = true;
        }

        public void SendLearningRecord()
        {
            ECAPI.SubmitUserLearningRecord(recordAsset.scenarioId, recordAsset.managedRecords, LearningRecordResponse, LogError);
        }

        private void LearningRecordResponse(AbstractAPI.EmptyResponse obj)
        {
            Debug.Log(JsonUtility.ToJson(obj));
        }

        public void SaveRecordAsset()
        {
            EditorUtility.CopySerialized(recordAsset, recordAsset);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}



