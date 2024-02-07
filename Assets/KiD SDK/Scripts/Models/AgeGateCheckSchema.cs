using System;
using System.Collections;
using System.Collections.Generic;

public class AgeGateCheckRequest {
	public string jurisdiction = "";
	public string dateOfBirth = "";
}

public class AgeGateCheckResponse {
	public bool success;
	public string error;
	public string errorMessage;
	public string status;
	public Challenge challenge;
	public Session session;
}

[Serializable]
public class Permission {
	public string name;
	public bool enabled;
}

[Serializable]
public class Challenge {
	public string url;
	//public string etag;
	public string oneTimePassword;
	public string type;
	public string challengeId;
	public bool childLiteAccessEnabled;
}

[Serializable]
public class Session {
	public string etag;
	public List<Permission> permissions = new();
	public string sessionId;
	public string status;
}