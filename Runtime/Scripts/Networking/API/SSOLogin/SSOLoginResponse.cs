using UnityEngine;

namespace SkillsVR.EnterpriseCloudSDK.Networking.API
{
	public class SSOLoginResponse : AbstractResponse
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
}