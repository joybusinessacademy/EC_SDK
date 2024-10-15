using SkillsVR.EnterpriseCloudSDK.Networking.API;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Security;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace SkillsVR.EnterpriseCloudSDK.Networking
{
    public static class RESTCore
    {
        [System.Serializable]
        public class RequestJson
        {
            public List<KeyValuePair> pairs = new List<KeyValuePair>();
            [System.Serializable]
            public class KeyValuePair
            {
                public string key;
                public string value;
            }
        }


        public static string AccessToken => accessToken;
        public static string RefreshToken => ECAPI.TryFetchStringFromIntent(ECAPI.refreshToken);

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

            ECAPI.TryFetchAccessTokenFromIntent();

            var request = new UnityWebRequest(url, httpType);
            string key = ECAPI.TryFetchStringFromIntent("OCAPIM_SUB_KEY") ?? PlayerPrefs.GetString("OCAPIM_SUB_KEY", "2d0a094d71ab400d866008be60a3f37c");
            request.SetRequestHeader("Ocp-Apim-Subscription-Key", key);

            string orgCode = ECAPI.TryFetchStringFromIntent("ORGCODE") ?? PlayerPrefs.GetString("ORGCODE", "skillvrnz");
            request.SetRequestHeader("x-ent-org-code", orgCode);

            if (authenticated)
                request.SetRequestHeader("Authorization", string.Format("Bearer {0}", accessToken));

            if (data != null)
            {
                var bytes = Encoding.UTF8.GetBytes(JsonUtility.ToJson(data as object));
                request.uploadHandler = new UploadHandlerRaw(bytes);
            }


            request.SetRequestHeader("Content-Type", "application/json");
            request.downloadHandler = new DownloadHandlerBuffer();

            request.disposeCertificateHandlerOnDispose = true;
            request.disposeDownloadHandlerOnDispose = true;
            request.disposeUploadHandlerOnDispose = true;

            return request;
        }
        
        public static string RepackRequestToJson<DATA>(UnityWebRequest request, DATA data)
        {
            RequestJson requestJson = new RequestJson();
            requestJson.pairs.Add(new RequestJson.KeyValuePair() { key = "url", value = request.url });
            requestJson.pairs.Add(new RequestJson.KeyValuePair() { key = "body", value = JsonUtility.ToJson(data) });
            requestJson.pairs.Add(new RequestJson.KeyValuePair() { key = "accessToken", value = RESTCore.AccessToken });
            requestJson.pairs.Add(new RequestJson.KeyValuePair() { key = "refreshToken", value = RESTCore.RefreshToken });
            requestJson.pairs.Add(new RequestJson.KeyValuePair() { key = "Ocp-Apim-Subscription-Key", value = request.GetRequestHeader("Ocp-Apim-Subscription-Key") });
            requestJson.pairs.Add(new RequestJson.KeyValuePair() { key = "x-ent-org-code", value = request.GetRequestHeader("x-ent-org-code") });
            
            return JsonUtility.ToJson(requestJson);                   
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

            ECAPI.TryFetchAccessTokenFromIntent();
            UnityWebRequest request = BuildUnityWebRequest(url, httpType, data, authenticated);

            if (0 == retryCount)
            {
                // if the session is created from the library app
                // broadcast to library app the requ                
                if (!string.IsNullOrEmpty(ECAPI.TryFetchStringFromIntent("SVR_MANAGED")) || PlayerPrefs.GetString("OFFLINEMODE") == "TRUE")
                {
                    ECAPI.SendToAndroid(RepackRequestToJson(request, data));
                    request?.Dispose();
                    onSuccess.Invoke(JsonUtility.FromJson<RESPONSE>("{}"));
                    yield break;
                }

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
                catch (Exception e)
                {
                    success = false;
                    errorMsg = request.downloadHandler.text + "\n" + e.Message;
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
                    // incase everything fails, lets send the payload library app
                    ECAPI.SendToAndroid(RepackRequestToJson(request,data));
                    Debug.LogErrorFormat("Response {0}\r\n{1} ==> Max retry reached ({2}x). Abort.", request.url, errorMsg, retryCount);
                    onError?.Invoke(errorMsg);
                }
            }

            request?.Dispose();
        }
    }

}
