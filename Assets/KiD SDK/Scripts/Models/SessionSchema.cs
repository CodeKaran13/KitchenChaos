using System;
using System.Collections.Generic;

[Serializable]
public class GetSessionResponse
{
	public bool success;
	public string error;
	public string errorMessage;
	public string status;
	public string etag;
	public List<Permission> permissions = new();
	public string sessionId;
}
