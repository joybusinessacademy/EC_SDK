using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using SkillsVR.EnterpriseCloudSDK.Networking.API;

#if SKILLS_VR
    using UnityEngine.XR;
#endif

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
            GameObject.DontDestroyOnLoad(runtimeServiceObject);
            globalRestServiceProvider = runtimeServiceObject.AddComponent<RESTService>();
            PlayerPrefs.SetString("StartTimeStamp", System.DateTime.Now.Ticks.ToString());
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

#if SKILLS_VR


        private List<InputDevice> leftController = new List<InputDevice>();
        private List<InputDevice> rightController = new List<InputDevice>();

        private const string debugString = "ABBAABBAAA";
        private string runningDebugString = "";

        void Start()
        {
            InputDeviceCharacteristics controllerCharacteristics = InputDeviceCharacteristics.Left;
            InputDevices.GetDevicesWithCharacteristics(controllerCharacteristics, leftController);

            controllerCharacteristics = InputDeviceCharacteristics.Right;
            InputDevices.GetDevicesWithCharacteristics(controllerCharacteristics, rightController);
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.A) || (Input.GetKeyDown(KeyCode.B)))
            {
                if (Input.GetKeyDown(KeyCode.A))
                    runningDebugString += "A";
                else
                    runningDebugString += "B";

                if (runningDebugString.Contains(debugString))
                    ECAPI.SubmitUserLearningRecord(null, (response) => Application.Quit(), (error) => Debug.LogError(error));
            }


            if (leftController.Count == 0 || rightController.Count == 0)
                return;
                
            var leftControllerX = leftController[0];
            var rightControllerX = rightController[0];

            if (leftControllerX != null)
            {
                if (leftControllerX.TryGetFeatureValue(CommonUsages.trigger, out float triggerValue))
                {
                    if (triggerValue > 0.5f)
                    {
                        if (rightControllerX.TryGetFeatureValue(CommonUsages.primaryButton, out bool primaryButtonValue))
                        {
                            if (primaryButtonValue)
                                runningDebugString += "A";
                        }

                        if (rightControllerX.TryGetFeatureValue(CommonUsages.secondaryButton, out bool secondaryButton))
                        {
                            if (secondaryButton)
                                runningDebugString += "B";
                        }

                        if (runningDebugString.Contains(debugString))
                            ECAPI.SubmitUserLearningRecord(null, (response) => Application.Quit(), (error) => Debug.LogError(error));
                    }
                }
                else 
                    runningDebugString = "";
            }
   
        }
#endif   
    }      
}
