using UnityEngine;

namespace SkillsVR.EnterpriseCloudSDK.Networking.API
{
    public class SSORefreshData
    {
        public string refreshToken;
        public string scope;
        public string clientId;
        public string loginUrl;


        public WWWForm GetLoginForm()
        {
            WWWForm form = new WWWForm();
            form.AddField("refresh_token", refreshToken);
            form.AddField("grant_type", "refresh_token");
            form.AddField("scope", scope);
            form.AddField("client_id", clientId);
            return form;
        }

        public SSORefreshData()
        {
            Init();
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
                "https://skvrentprodau.onmicrosoft.com/enterprise-api/ec-cck-license.read",
                "https://skvrentprodau.onmicrosoft.com/enterprise-api/ec-cck-log.write",
                "offline_access",
                "openid");
        }

        private static string GetDefaultLoginUrl()
        {
            return "https://skvrentprodau.b2clogin.com/skvrentprodau.onmicrosoft.com/B2C_1_ropc/oauth2/v2.0/token";
        }


        public bool IsValid()
        {
            return !(string.IsNullOrWhiteSpace(refreshToken)
                     && string.IsNullOrWhiteSpace(scope)
                     && string.IsNullOrWhiteSpace(scope)
                     && string.IsNullOrWhiteSpace(clientId)
                     && string.IsNullOrWhiteSpace(loginUrl));
        }
    }
}