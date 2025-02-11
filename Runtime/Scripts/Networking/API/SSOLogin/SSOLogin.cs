using SkillsVR.EnterpriseCloudSDK.Networking;
using SkillsVR.EnterpriseCloudSDK.Networking.API;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace SkillsVR.EnterpriseCloudSDK.Networking.API
{
    public class SSOLogin
    {
        private const string TOKEN_EXPIRATION_TIME = "TOKEN_EXPIRATION_TIME";
        private const string REFRESH_TOKEN_EXPIRATION_TIME = "REFRESH_TOKEN_EXPIRATION_TIME";
        private const string REFRESH_TOKEN = "REFRESH_TOKEN";

        private const int LENGHT_OF_REFRESH_TOKEN_EXPIRATION_TIME = 89; // In Days


        public static IEnumerator SendSSOLoginForm(SSOLoginData loginData, Action<SSOLoginResponse> onSuccess, Action<string> onFail = null)
        {
            if (null == loginData)
            {
                onFail?.Invoke("SendSSOLoginForm Fail: Login data cannot be null or empty.");
                yield break;
            }
            if (!loginData.IsValid())
            {
                onFail?.Invoke("SendSSOLoginForm Fail: Invalid Login Data.");
                yield break;
            }

            using UnityWebRequest www = UnityWebRequest.Post(loginData.loginUrl, loginData.GetLoginForm());
            Debug.Log(string.Join(" ", "Start Login", loginData.userName, "to", loginData.loginUrl));
            www.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");

            yield return www.SendWebRequest();


            // Failed to login
            if (www.isNetworkError || www.isHttpError)
            {
                Debug.LogError(string.Join(" ", "Login Fail:", www.error, loginData.userName, loginData.loginUrl));
                onFail?.Invoke(www.error);
                yield break;
            }

            SSOLoginResponse response = new();
            // Attempt to read responce
            try
            {
                response.Read(www.downloadHandler.text);
            }
            catch (Exception e)
            {
                Debug.LogError(string.Join(" ", "Login Fail:", e.Message, loginData.userName, loginData.loginUrl));
                onFail?.Invoke(e.Message);
                yield break;

            }

            Debug.Log(string.Join(" ", "Login Success ", loginData.userName, "at", loginData.loginUrl));
            onSuccess?.Invoke(response);

            SessionState.SetString(TOKEN_EXPIRATION_TIME, DateTime.Now.AddSeconds(int.Parse(response.expires_in)).ToString(CultureInfo.InvariantCulture));
            EditorPrefs.SetString(REFRESH_TOKEN_EXPIRATION_TIME, DateTime.Now.AddDays(LENGHT_OF_REFRESH_TOKEN_EXPIRATION_TIME).ToString(CultureInfo.InvariantCulture));
            EditorPrefs.SetString(REFRESH_TOKEN, response.refresh_token);
        }

        public bool ValidateKey()
        {
            if (CurrentTokenIsValid())
            {
                return true;
            }

            if (CurrentRefreshTokenIsValid())
            {

                return true;
            }


            return false;
        }

        public static IEnumerator SendRefreshToken(SSORefreshData loginData, Action<SSORefreshResponse> onSuccess, Action<string> onFail = null)
        {
            loginData.refreshToken = GetRefreshToken();

            WWWForm form = loginData.GetLoginForm();


            using UnityWebRequest www = UnityWebRequest.Post(loginData.loginUrl, form);

            www.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");
            yield return www.SendWebRequest();

            // Failed to login
            if (www.isNetworkError || www.isHttpError)
            {
                Debug.LogError(string.Join(" ", "Refresh Login Fail:", www.error, loginData.loginUrl));
                onFail?.Invoke(www.error);
                yield break;
            }

            SSORefreshResponse response = new();
            // Attempt to read responce
            try
            {
                response.Read(www.downloadHandler.text);
            }
            catch (Exception e)
            {
                Debug.LogError(string.Join(" ", "Refresh Login Fail:", e.Message, loginData.loginUrl));
                onFail?.Invoke(e.Message);
                yield break;

            }

            Debug.Log(string.Join(" ", "Refresh Login Success ", "at", loginData.loginUrl));
            onSuccess?.Invoke(response);

            SessionState.SetString(TOKEN_EXPIRATION_TIME, DateTime.Now.AddSeconds(int.Parse(response.expires_in)).ToString(CultureInfo.InvariantCulture));
            EditorPrefs.SetString(REFRESH_TOKEN_EXPIRATION_TIME, DateTime.Now.AddDays(int.Parse(response.refresh_token_expires_in)).ToString(CultureInfo.InvariantCulture));

            EditorPrefs.SetString(REFRESH_TOKEN, response.refresh_token);
        }


        public static bool CurrentTokenIsValid()
        {
            bool hasKey = DateTime.TryParse(SessionState.GetString(TOKEN_EXPIRATION_TIME, ""), out DateTime timeSaved);
            if (!hasKey || string.IsNullOrWhiteSpace(RESTCore.AccessToken))
            {
                return false;
            }

            return timeSaved >= DateTime.Now;
        }

        public static bool CurrentRefreshTokenIsValid()
        {
            bool hasKey = DateTime.TryParse(EditorPrefs.GetString(REFRESH_TOKEN_EXPIRATION_TIME, ""), out DateTime timeSaved);
            if (!hasKey)
            {
                return false;
            }

            return timeSaved >= DateTime.Now;
        }

        public static string GetRefreshToken()
        {
            return EditorPrefs.GetString(REFRESH_TOKEN, "");
        }
    }
}