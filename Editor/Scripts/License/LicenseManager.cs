using SkillsVR.EnterpriseCloudSDK;
using SkillsVR.EnterpriseCloudSDK.Data;
using System;
using System.IO;
using UnityEngine;

public class LicenseManager
{
	private ECRecordCollectionAsset recordConfig;

	private static LicenseRecordData licenseRecordData;
	private string jsonFileName = "LicenseMockData.txt";
	private string enterpriseLicenseURL = "https://skillsvr.com/";

	public void GetLicenseDetailsFromEnterprise()
	{
		//Send API Call for License
		//ECAPI.TryFetchLicenseData();

		string path = (Application.streamingAssetsPath + "/" + jsonFileName);
		if (File.Exists(path))
		{
			string licenseJson = File.ReadAllText(path);
			licenseRecordData = JsonUtility.FromJson<LicenseRecordData>(licenseJson);
		}
		else
		{
			licenseRecordData = new LicenseRecordData();
			licenseRecordData.licenseType = LicenseType.MISSING;
		}
	}

	public bool IsLicenseValid()
	{
		if(licenseRecordData == null)
			return false;

		if (licenseRecordData.licenseType == LicenseType.MISSING)
			return false;

		if(IsExpiredDate(licenseRecordData.licenseExpirationDate))
			return false;

		return true;
	}

	private bool IsExpiredDate(string expirationDate)
	{
		string dateFormat = "dd/MM/yyyy";

		if (DateTime.TryParseExact(expirationDate, dateFormat, null, System.Globalization.DateTimeStyles.None, out DateTime parsedDate))
		{
			DateTime currentDate = DateTime.Now;
			int result = DateTime.Compare(parsedDate, currentDate);

			//In the past == Expired
			if (result < 0)
				return true;
			else if (result > 0) // In Future
				return false;
		}

		return false;
	}

	internal string GetActivationDate()
	{
		return licenseRecordData != null ? licenseRecordData.licenseActivationDate : "Missing Date";
	}

	internal string GetExpirationDate()
	{
		return licenseRecordData != null ? licenseRecordData.licenseExpirationDate : "Missing Date";
	}

	internal bool HasData()
	{
		if (licenseRecordData == null)
			return false;
		else
			return true;
	}
}
