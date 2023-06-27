using SkillsVR.EnterpriseCloudSDK.Networking;
using SkillsVR.EnterpriseCloudSDK.Networking.API;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;

namespace SkillsVR.EnterpriseCloudSDK.Editor.Networking
{
    public class RESTServiceEditor : IRestServiceProvider
    {
        public void SendRequest<DATA, RESPONSE>(AbstractAPI<DATA, RESPONSE> request, Action<RESPONSE> onSuccess = null, Action<string> onError = null) where RESPONSE : AbstractResponse
        {
            EditorCoroutineUtility.StartCoroutine(RESTCore.Send<DATA, RESPONSE>(request.URL, request.requestType.ToString(), request.data, request.authenticated,
                onSuccess: onSuccess,
                onError: onError), this);
        }

        [InitializeOnLoadMethod]
        private static void RegisterPlayModeChangeEvent()
        {
            EditorApplication.playModeStateChanged += InitAtEnterEditMode;
        }

        private static void InitAtEnterEditMode(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredEditMode)
            {
                SetupEditorRestServiceProvider();
            }
        }

        [InitializeOnLoadMethod]
        private static void Init()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }
            _ = new ConfigService();
            SetupEditorRestServiceProvider();
        }

        public static void SetupEditorRestServiceProvider()
        {
            RESTService.SetRestServiceProvider(new RESTServiceEditor());
            UnityEngine.Debug.Log("Rest Service RESTServiceEditor");
        }

        public void SendCustomCoroutine(IEnumerator coro)
        {
            EditorCoroutineUtility.StartCoroutine(coro, this);
        }
    }

    [Serializable]
    public class ConfigService
    {
        private static ConfigService instance = null;

        [Serializable]
        public class Config
        {
            public string region;
            public string domain;
            public string subscriptionKey;
            public string clientId;
            public string ropcUrl;
            public string scope;
        }

        public List<Config> configs;
        public ConfigService()
        {
            instance = this;
            var json = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(Resources.Load<TextAsset>("cfgs").text));
            JsonUtility.FromJsonOverwrite(json, this);
        }

        public static Config Get(string id)
        {
            if (instance == null)
                instance = new ConfigService();

            return instance.configs.Find(k => k.region.Equals(id));
        }
    }
}
