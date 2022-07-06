using SkillsVR.EnterpriseCloudSDK.Networking;
using SkillsVR.EnterpriseCloudSDK.Networking.API;
using System;
using System.Collections;
using Unity.EditorCoroutines.Editor;
using UnityEditor;

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
}
