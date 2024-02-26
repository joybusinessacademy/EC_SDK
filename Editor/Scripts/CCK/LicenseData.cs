using System;

namespace SkillsVR.EnterpriseCloudSDK.Editor
{
	public class LicenseData
	{
		public string orgAdminEmail { get; set; }
		public string orgAdminFirstName { get; set; }
		public string orgAdminLastName { get; set; }
		public DateTime expiryDate { get; set; }
		public string status { get; set; }
		public bool hasCck { get; set; }
		public bool hasPermission { get; set; }
	}
}