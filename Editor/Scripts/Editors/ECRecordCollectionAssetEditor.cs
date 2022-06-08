using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using SkillsVR.EnterpriseCloudSDK.Data;

namespace SkillsVR.EnterpriseCloudSDK.Editor.Editors
{
    [CustomEditor(typeof(ECRecordCollectionAsset))]
    [CanEditMultipleObjects]
    public class ECRecordCollectionAssetEditor : UnityEditor.Editor
    {
        protected ECRecordCollectionAsset smartTarget;

        public static ECRecordCollectionAsset CreateOrLoadAsset()
        {
            ECRecordCollectionAsset asset = ECRecordCollectionAsset.GetECRecordAsset();
            if (null != asset)
            {
                return asset;
            }

            asset = ScriptableObject.CreateInstance<ECRecordCollectionAsset>();

            string dirAssetPath = Path.Combine(ECRecordCollectionAsset.ASSET_PATH, ECRecordCollectionAsset.RESOURCE_PATH);
            if (!AssetDatabase.IsValidFolder(dirAssetPath))
            {
                AssetDatabase.CreateFolder(ECRecordCollectionAsset.ASSET_PATH, ECRecordCollectionAsset.RESOURCE_PATH);
            }
            string fileName = ECRecordCollectionAsset.ASSET_FILE_NAME;
            string fileAssetPath = Path.Combine(dirAssetPath, fileName);
            AssetDatabase.CreateAsset(asset, fileAssetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return asset;
        }

        void OnEnable()
        {
            smartTarget = (ECRecordCollectionAsset)target;
        }

        private void OnDisable()
        {
            EditorUtility.CopySerialized(smartTarget, smartTarget);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Changed");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            GUILayout.Space(20);
            GUILayout.Label("Quick Methods");
            if (GUILayout.Button("Print Records"))
            {
                smartTarget.PrintRecords();
            }
            if (GUILayout.Button("Reset User Scores"))
            {
                smartTarget.ResetUserScores();
            }

            if (GUILayout.Button("GetConfig"))
            {
                smartTarget.GetConfig(null, Debug.LogError);
            }
            if (GUILayout.Button("SubmitUserScore"))
            {
                smartTarget.SubmitUserScore(null, Debug.LogError);
            }
        }
    }
}
