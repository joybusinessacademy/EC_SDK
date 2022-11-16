using SkillsVR.EnterpriseCloudSDK.Data;
using UnityEngine;

namespace SkillsVR.EnterpriseCloudSDK.Networking.API
{
    
    public partial class UpdateSessionStatus : AbstractAPI<AbstractAPI.EmptyData, AbstractAPI.EmptyResponse>
    {
        public enum Status {
            Planned = 0,
            Inprogress,
            Completed,
            Incomplete,
            Passed,
            Failed
        }

        public UpdateSessionStatus(int sessionId, Status status) : base(string.Format(ECAPI.domain + "/api/plannedsession/{0}/{1}", sessionId.ToString(), ((int)status).ToString()))
        {
            requestType = HttpRequestType.PUT;
            authenticated = true;            
        }
    }
}
