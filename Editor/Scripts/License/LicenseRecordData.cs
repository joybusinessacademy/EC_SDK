using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum LicenseType
{
	FREE,
	PAID,
	MISSING
}

[Serializable]
public class LicenseRecordData
{
	public LicenseType licenseType = LicenseType.PAID;
	public string licenseActivationDate = "";
	public string licenseExpirationDate = "";
}
