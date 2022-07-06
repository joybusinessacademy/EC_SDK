using SkillsVR.EnterpriseCloudSDK.Networking;
using SkillsVR.EnterpriseCloudSDK.Networking.API;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;


public class ResponseToken : AbstractResponse
{
    public string access_token;
    public string token_type;
    public string expires_in;
    public string refresh_token;

    public override void Read(dynamic objs)
    {
        string json = objs as string;
        JsonUtility.FromJsonOverwrite(json, this);
        RESTCore.SetAccessToken(access_token);
    }
}

public class SSOLogin : MonoBehaviour
{

    public static WWWForm GetTextLoginForm()
    {
        WWWForm form = new WWWForm();
        form.AddField("username", "jortiga+admin@skillsvr.com");
        form.AddField("password", "123456789Aa");
        form.AddField("grant_type", "password");
        form.AddField("scope", GetScope());
        form.AddField("client_id", "3b53156b-5eeb-45d8-b503-92a6afd37325");
        form.AddField("reponse_type", "id_token");
        return form;
    }

    public static string GetScope()
    {
        return "https://skillsvr.onmicrosoft.com/enterprise-api/ec-portal-access.read" +
            " https://skillsvr.onmicrosoft.com/enterprise-api/ec-portal-access.write " +
            "https://skillsvr.onmicrosoft.com/enterprise-api/ec-portal-access.view " +
            "https://skillsvr.onmicrosoft.com/enterprise-api/ec-scenario.read " +
            "https://skillsvr.onmicrosoft.com/enterprise-api/ec-scenario.write " +
            "https://skillsvr.onmicrosoft.com/enterprise-api/ec-scenario.view " +
            "https://skillsvr.onmicrosoft.com/enterprise-api/ec-analytics.read " +
            "https://skillsvr.onmicrosoft.com/enterprise-api/ec-analytics.write " +
            "https://skillsvr.onmicrosoft.com/enterprise-api/ec-analytics.view " +
            "https://skillsvr.onmicrosoft.com/enterprise-api/ec-marketplace.read " +
            "https://skillsvr.onmicrosoft.com/enterprise-api/ec-marketplace.write " +
            "https://skillsvr.onmicrosoft.com/enterprise-api/ec-marketplace.view " +
            "https://skillsvr.onmicrosoft.com/enterprise-api/ec-device-management.read " +
            "https://skillsvr.onmicrosoft.com/enterprise-api/ec-device-management.write " +
            "https://skillsvr.onmicrosoft.com/enterprise-api/ec-device-management.view " +
            "https://skillsvr.onmicrosoft.com/enterprise-api/ec-user-management.read " +
            "https://skillsvr.onmicrosoft.com/enterprise-api/ec-user-management.write " +
            "https://skillsvr.onmicrosoft.com/enterprise-api/ec-user-management.view " +
            " openid" +
            " profile" +
            " offline_access";
    }

    public static WWWForm CreateLoginForm(string userName, string password, string scope, string clientId)
    {
        WWWForm form = new WWWForm();
        form.AddField("username", userName);
        form.AddField("password", password);
        form.AddField("grant_type", "password");
        form.AddField("scope", scope);
        form.AddField("client_id", clientId);
        form.AddField("reponse_type", "id_token");
        return form;
    }

    public static string GetTestLoginUrl()
    {
        return "https://skillsvr.b2clogin.com/skillsvr.onmicrosoft.com/B2C_1_device-dashboard-dev-ropc/oauth2/v2.0/token";
    }

    public static IEnumerator SendSSOLoginForm(WWWForm loginForm, string loginUrl, Action<ResponseToken> onSuccess, Action<string> onFail = null)
    {

        using (UnityWebRequest www = UnityWebRequest.Post(loginUrl, loginForm))
        {
            www.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
                onFail?.Invoke(www.error);
            }
            else
            {
                ResponseToken response = new ResponseToken();
                try
                {
                    response.Read(www.downloadHandler.text);
                    Debug.Log("Login Success ");
                }
                catch { }
                onSuccess?.Invoke(response);
            }
        }
    }
}
