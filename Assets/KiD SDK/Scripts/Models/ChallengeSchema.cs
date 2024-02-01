using System;

public enum ChallengeType {
	CHALLENGE_PARENTAL_CONSENT,
	CHALLENGE_DIGITAL_CONSENT_AGE,
	CHALLENGE_AGE_APPEAL
}

public class ChallengeEmailRequest {
	public string challengeId;
	public string email;
}

[Serializable]
public class GetChallengeResponse {
	public bool success;
	public string error;
	public string errorMessage;
	public Challenge challenge;
}

[Serializable]
public class AwaitChallengeResponse {
	public bool success;
	public string error;
	public string errorMessage;
	public string status;
	public string sessionId;
}