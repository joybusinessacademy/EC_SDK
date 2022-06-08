﻿using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using SkillsVR.EnterpriseCloudSDK.Networking.API;

namespace SkillsVR.EnterpriseCloudSDK.Networking
{
    public interface IRestServiceProvider
    {
        void SendRequest<DATA, RESPONSE>(AbstractAPI<DATA, RESPONSE> request, Action<RESPONSE> onSuccess = null, Action<string> onError = null) where RESPONSE : AbstractResponse;
    }

    public class RESTService : MonoBehaviour, IRestServiceProvider
    {

        private static IRestServiceProvider globalRestServiceProvider;

        public static void Send<DATA, RESPONSE>(AbstractAPI<DATA, RESPONSE> request, Action<RESPONSE> onSuccess = null, Action<string> onError = null) where RESPONSE : AbstractResponse
        {
            globalRestServiceProvider.SendRequest(request, onSuccess, onError);
        }

        public static void SetRestServiceProvider(IRestServiceProvider provider)
        {
            globalRestServiceProvider = provider;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void InitRuntimeRestService()
        {
            if (null != globalRestServiceProvider && typeof(RESTService) == globalRestServiceProvider.GetType())
            {
                return;
            }
            GameObject runtimeServiceObject = new GameObject(nameof(RESTService));
            GameObject.DontDestroyOnLoad(runtimeServiceObject);
            globalRestServiceProvider = runtimeServiceObject.AddComponent<RESTService>();
            Debug.Log("Reset sevice ready: " + globalRestServiceProvider.GetType().Name);
        }

        public void SendRequest<DATA, RESPONSE>(AbstractAPI<DATA, RESPONSE> request, Action<RESPONSE> onSuccess = null, Action<string> onError = null) where RESPONSE : AbstractResponse
        {
            StartCoroutine(RESTCore.Send<DATA, RESPONSE>(request.URL, request.requestType.ToString(), request.data, request.authenticated, 
                onSuccess: onSuccess, 
                onError: onError));
        }
    }
}
