using System;
using UnityEngine;

namespace SkillsVR.EnterpriseCloudSDK.Networking.API
{
	[Serializable]
    public class SSOLoginData
    {
        public string userName;
        public string password;
        public string scope;
        public string clientId;
        public string loginUrl;

        public static readonly string[] regions = { "US", "AU"
#if SKILLS_VR
            ,"US-Test"
            ,"AU-Test"
            ,"AU-Dev"
#endif
        };
        public int selectedRegion = 0;

        public SSOLoginData()
        {
            Init();
        }

        public SSOLoginData(SSOLoginData source)
        {
            Init();
            if (null != source)
            {
                userName = source.userName;
                password = source.password;
                scope = source.scope;
                clientId = source.clientId;
                loginUrl = source.loginUrl;
            }

        }

        public WWWForm GetLoginForm()
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

        public bool IsValid()
        {
            return !(string.IsNullOrWhiteSpace(userName)
                && string.IsNullOrWhiteSpace(password)
                && string.IsNullOrWhiteSpace(scope)
                && string.IsNullOrWhiteSpace(clientId)
                && string.IsNullOrWhiteSpace(loginUrl));
        }

        protected void Init()
        {
            if (string.IsNullOrWhiteSpace(scope))
            {
                scope = GetDefaultScopeString();
            }
            if (string.IsNullOrWhiteSpace(loginUrl))
            {
                loginUrl = GetDefaultLoginUrl();
            }
        }

        public static string GetDefaultScopeString()
        {
            return string.Join(" ",
                "https://skillsvr.onmicrosoft.com/enterprise-api/ec-portal-access.read",
                "https://skillsvr.onmicrosoft.com/enterprise-api/ec-portal-access.write",
                "https://skillsvr.onmicrosoft.com/enterprise-api/ec-portal-access.view",
                "https://skillsvr.onmicrosoft.com/enterprise-api/ec-scenario.read",
                "https://skillsvr.onmicrosoft.com/enterprise-api/ec-scenario.write",
                "https://skillsvr.onmicrosoft.com/enterprise-api/ec-scenario.view",
                "https://skillsvr.onmicrosoft.com/enterprise-api/ec-analytics.read",
                "https://skillsvr.onmicrosoft.com/enterprise-api/ec-analytics.write",
                "https://skillsvr.onmicrosoft.com/enterprise-api/ec-analytics.view",
                "https://skillsvr.onmicrosoft.com/enterprise-api/ec-marketplace.read",
                "https://skillsvr.onmicrosoft.com/enterprise-api/ec-marketplace.write",
                "https://skillsvr.onmicrosoft.com/enterprise-api/ec-marketplace.view",
                "https://skillsvr.onmicrosoft.com/enterprise-api/ec-device-management.read",
                "https://skillsvr.onmicrosoft.com/enterprise-api/ec-device-management.write",
                "https://skillsvr.onmicrosoft.com/enterprise-api/ec-device-management.view",
                "https://skillsvr.onmicrosoft.com/enterprise-api/ec-user-management.read",
                "https://skillsvr.onmicrosoft.com/enterprise-api/ec-user-management.write",
                "https://skillsvr.onmicrosoft.com/enterprise-api/ec-user-management.view",
                "openid",
                "profile",
                "offline_access");
        }

        private static string GetDefaultLoginUrl()
        {
            return "https://skillsvr.b2clogin.com/skillsvr.onmicrosoft.com/B2C_1_device-dashboard-dev-ropc/oauth2/v2.0/token";
        }
    }
}