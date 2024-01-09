using System;
using System.Collections;
using System.Collections.Generic;


public enum ChallengeType {
	CHALLENGE_PARENTAL_CONSENT,
	CHALLENGE_DIGITAL_CONSENT_AGE
}

public class ChallengeEmailSchema {

	public string challengeId;
	public string email;
}

[Serializable]
public class ChallengeAwaitResponse {
	public bool success;
	public string error;
	public string errorMessage;
	public string status;
}