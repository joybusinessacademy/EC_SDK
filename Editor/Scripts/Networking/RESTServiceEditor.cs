using SkillsVR.EnterpriseCloudSDK.Networking;
using SkillsVR.EnterpriseCloudSDK.Networking.API;
using System;
using Unity.EditorCoroutines.Editor;
using UnityEditor;

namespace SkillsVR.EnterpriseCloudSDK.Editor.Networking
{
    public class RESTServiceEditor : IRestServiceProvider
    {
        [InitializeOnLoadMethod] 
        public static void Init()
        {
            RESTService.SetRestServiceProvider(new RESTServiceEditor());
        }
        public void SendRequest<DATA, RESPONSE>(AbstractAPI<DATA, RESPONSE> request, Action<RESPONSE> onSuccess = null, Action<string> onError = null) where RESPONSE : AbstractResponse
        {
            EditorCoroutineUtility.StartCoroutine(RESTCore.Send<DATA, RESPONSE>(request.URL, request.requestType.ToString(), request.data, request.authenticated,
                onSuccess: onSuccess,
                onError: onError), this);
        }
    }
}
