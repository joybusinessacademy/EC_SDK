using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;


namespace SkillsVR.Network.EC
{
    public class RESTService : MonoBehaviour
    {

        public static string DOMAIN => "https://develop-ec-bff.skillsvr.com";

        public static string AccessToken => accessToken;
        private static string accessToken = string.Empty;
        public static void SetAccessToken(string token)
        {
            Debug.Log(token);
            accessToken = token;
        }

        private const int FAIL_RETRY_TIMES = 3;
        private bool sendingRequest = false;

        private Dictionary<string, int> apiRetryCount = new Dictionary<string, int>();

        private UnityWebRequest BuildUnityWebRequest<DATA>(string url, string httpType, DATA data, bool authenticated = false)
        {
            var request = new UnityWebRequest(url, httpType);

            if (authenticated)
                request.SetRequestHeader("Authorization", string.Format("Bearer {0}", accessToken));

            if (data != null)
                request.uploadHandler = (UploadHandler)new UploadHandlerRaw(Encoding.UTF8.GetBytes(JsonUtility.ToJson(data)));

            request.SetRequestHeader("Content-Type", "application/json");
            request.downloadHandler = new DownloadHandlerBuffer();

            return request;
        }

        public void Send<DATA, RESPONSE>(AbstractAPI<DATA, RESPONSE> request, Action<RESPONSE> onSuccess = null, Action<string> onError = null) where RESPONSE : AbstractResponse
        {
            if (!apiRetryCount.ContainsKey(request.URL))
                apiRetryCount.Add(request.URL, 0);

            apiRetryCount[request.URL] = 0;
            var unityWebRequest = BuildUnityWebRequest(request.URL, request.requestType.ToString(), request.data, request.authenticated);
            StartCoroutine(Send<RESPONSE>(unityWebRequest, onSuccess, onError));
        }

        private IEnumerator Send<RESPONSE>(UnityWebRequest request, System.Action<RESPONSE> onSuccess, System.Action<string> onError)
            where RESPONSE : AbstractResponse
        {
            Debug.Log(request.url);
            sendingRequest = true;
            yield return request.SendWebRequest();
            if (request.isHttpError)
            {
                apiRetryCount[request.url]++;
                if (apiRetryCount[request.url] < FAIL_RETRY_TIMES)
                {
                    var retryRequest = BuildUnityWebRequest(request.url, UnityWebRequest.kHttpVerbGET, request.GetRequestHeader("Authorization") != string.Empty);
                    retryRequest.uploadHandler = request.uploadHandler;
                    StartCoroutine(Send<RESPONSE>(retryRequest, onSuccess, onError));
                    yield break;
                }
                onError?.Invoke(request.error);
            }            
            else
            {
                var response = JsonUtility.FromJson<RESPONSE>(request.downloadHandler.text);
                response.Read(request.downloadHandler.text);
                onSuccess?.Invoke(response);
            }

            sendingRequest = false;
        }
    }
}
