using System;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public class SessionData
{
	public bool success;
	public string error;
	public string errorMessage;
	public string status;
	public Challenge challenge;
	public List<Permission> permissions = new List<Permission>();
}
