using SkillsVR.EnterpriseCloudSDK.Networking.API;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace SkillsVR.EnterpriseCloudSDK.Networking
{
    public static class RESTCore
    {
       
        public static string AccessToken => accessToken;
        private static string accessToken = string.Empty;

        [RuntimeInitializeOnLoadMethod]
        public static void ResetAssessToken()
        {
            accessToken = string.Empty;
        }
        public static void SetAccessToken(string token)
        {
            accessToken = token;
        }

        private const int FAIL_RETRY_TIMES = 3;

        public static UnityWebRequest BuildUnityWebRequest<DATA>(string url, string httpType, DATA data, bool authenticated = false)
        {
            if (data is WWWForm)
            {
                var postRequest = UnityWebRequest.Post(url, data as WWWForm);
                return postRequest; 
            }
            
            var request = new UnityWebRequest(url, httpType);

            if (authenticated)
                request.SetRequestHeader("Authorization", string.Format("Bearer {0}", accessToken));

            if (data != null)
                request.uploadHandler = (UploadHandler)new UploadHandlerRaw(Encoding.UTF8.GetBytes(JsonUtility.ToJson(data)));

            
            request.SetRequestHeader("Content-Type", "application/json");
            request.downloadHandler = new DownloadHandlerBuffer();

            return request;
        }

        public static IEnumerator Send<DATA, RESPONSE>(string url, string httpType, DATA data, bool authenticated, System.Action<RESPONSE> onSuccess, System.Action<string> onError, int retryCount = 0)
            where RESPONSE : AbstractResponse
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                onError?.Invoke("Url cannot be null or empty.");
                yield break;
            }
            if (string.IsNullOrWhiteSpace(httpType))
            {
                onError?.Invoke("httpType cannot be null or empty.");
                yield break;
            }

            UnityWebRequest request = BuildUnityWebRequest(url, httpType, data, authenticated);
            if (0 == retryCount)
            {
                string dataStr = "";
                try
                {
                    dataStr = JsonUtility.ToJson(data);
                }
                catch { }
                Debug.LogFormat("{0} {1}\r\n{2}", request.method, request.url, dataStr);
            }
            

            yield return request.SendWebRequest();
            bool success = false;
            string errorMsg = null;
            RESPONSE response = null;
            if (request.isHttpError || request.isNetworkError)
            {
                success = false;
                errorMsg = request.error;
            }
            else
            {
                try
                {
                    response = JsonUtility.FromJson<RESPONSE>(request.downloadHandler.text);
                    Debug.LogFormat("Response {0}\r\n{1}", request.url, request.downloadHandler.text);
                    response.Read(request.downloadHandler.text);
                    success = null != response;
                    errorMsg = null == response ? "No response data received." : null;
                }
                catch(Exception e)
                {
                    success = false;
                    errorMsg = e.Message;
                }
                
            }

            if (success)
            {
                onSuccess?.Invoke(response);
            }
            else
            {
                if (retryCount < FAIL_RETRY_TIMES)
                {
                    ++retryCount;
                    Debug.LogErrorFormat("Response {0}\r\n{1} ==> start {2}x retry.", request.url, errorMsg, retryCount);
                    yield return Send<DATA, RESPONSE>(url, httpType, data, authenticated, onSuccess, onError, retryCount);
                }
                else
                {
                    Debug.LogErrorFormat("Response {0}\r\n{1} ==> Max retry reached ({2}x). Abort.", request.url, errorMsg, retryCount);
                    onError?.Invoke(errorMsg);
                }
            }
        }
    }
}
