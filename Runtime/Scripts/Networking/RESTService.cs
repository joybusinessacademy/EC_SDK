using SkillsVR.EnterpriseCloudSDK.Networking.API;
using System;
using System.Collections;
using UnityEngine;

namespace SkillsVR.EnterpriseCloudSDK.Networking
{
    public interface IRestServiceProvider
    {
        void SendRequest<DATA, RESPONSE>(AbstractAPI<DATA, RESPONSE> request, Action<RESPONSE> onSuccess = null, Action<string> onError = null) where RESPONSE : AbstractResponse;

        void SendCustomCoroutine(IEnumerator coro);
    }

    public class RESTService : MonoBehaviour, IRestServiceProvider
    {

        private static IRestServiceProvider globalRestServiceProvider;

        public static void Send<DATA, RESPONSE>(AbstractAPI<DATA, RESPONSE> request, Action<RESPONSE> onSuccess = null, Action<string> onError = null) where RESPONSE : AbstractResponse
        {
            globalRestServiceProvider.SendRequest(request, onSuccess, onError);
        }

        public static void SendByCustomCoroutine(IEnumerator coro)
        {
            globalRestServiceProvider.SendCustomCoroutine(coro);
        }

        public static void SetRestServiceProvider(IRestServiceProvider provider)
        {
            globalRestServiceProvider = provider;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void InitRuntimeRestService()
        {
            if (null != globalRestServiceProvider && typeof(RESTService) == globalRestServiceProvider.GetType())
            {
                return;
            }
            GameObject runtimeServiceObject = new GameObject(nameof(RESTService));
            runtimeServiceObject.AddComponent<ECRecordAgent>();
            
            GameObject.DontDestroyOnLoad(runtimeServiceObject);
            globalRestServiceProvider = runtimeServiceObject.AddComponent<RESTService>();
            PlayerPrefs.SetString("StartTimeStamp", System.DateTime.Now.Ticks.ToString());

#if UNITY_EDITOR
            // replace string empty with your access token if you want to do editor tests
            RESTCore.SetAccessToken(string.Empty);
#endif
            // lets get config here too
            if (ECAPI.HasLoginToken())
            {
                // create session
                var i = Data.ECRecordCollectionAsset.GetECRecordAsset();
                i.currentConfig.scenarioId = ECAPI.TryFetchStringFromIntent(ECAPI.IntentScenarioIdKey) ?? i.currentConfig.scenarioId;
                i.currentConfig.domain = ECAPI.TryFetchStringFromIntent(ECAPI.domainIntentId) ?? i.currentConfig.domain;
                ECAPI.domain = i.currentConfig.domain;

                ECAPI.GetConfig(i.currentConfig.scenarioId, (res) =>
                {
                    i.OrderRuntimeManagedRecords(res);
                });
            }
            
#if UNITY_EDITOR
            Debug.Log("Rest Service " + globalRestServiceProvider.GetType().Name);
#endif
        }

        public void SendCustomCoroutine(IEnumerator coro)
        {
            StartCoroutine(coro);
        }

        public void SendRequest<DATA, RESPONSE>(AbstractAPI<DATA, RESPONSE> request, Action<RESPONSE> onSuccess = null, Action<string> onError = null) where RESPONSE : AbstractResponse
        {
            StartCoroutine(RESTCore.Send<DATA, RESPONSE>(request.URL, request.requestType.ToString(), request.data, request.authenticated, 
                onSuccess: onSuccess, 
                onError: onError));
        }
    }      
}
