using UnityEngine;

namespace SkillsVR.EnterpriseCloudSDK.Networking.API
{
    public class SSORefreshResponse : AbstractResponse
    {
        public string access_token;
        public string id_token;
        public string token_type;
        public string expires_in;
        public string expires_on;
        public string resource;
        public string not_before;
        public string id_token_expires_in;
        public string profile_info;
        public string scope;
        public string refresh_token;
        public string refresh_token_expires_in;

        public override void Read(dynamic objs)
        {
            string json = objs as string;
            JsonUtility.FromJsonOverwrite(json, this);
            RESTCore.SetAccessToken(access_token);
        }
    }
}