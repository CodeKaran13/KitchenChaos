using System;
using System.Collections;
using System.Collections.Generic;

public class CreateSessionSchema {
	public string jurisdiction = "";
	public string dateOfBirth = "";
}

[Serializable]
public class CreateSessionResponse {
	public bool success;
	public string error;
	public string errorMessage;
	public string status;
	public string sessionId;

	public Challenge challenge;
	public List<Permission> permissions = new List<Permission>();
}

[Serializable]
public class Permission {
	public string name;
	public bool enabled;
}

[Serializable]
public class Challenge {
	public string url;
	public string oneTimePassword;
	public string type;
	public string challengeId;
}