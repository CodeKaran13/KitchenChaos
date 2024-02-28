using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationManager : MonoBehaviour
{
	public static AnimationManager Instance { get; private set; }

	[Header("Rocket Animation")]
	[SerializeField] private Rocket rocket;

	private Action caller;

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
		}
		else
		{
			if (Instance != this)
			{
				Destroy(this);
			}
		}
	}

	public void PlayRocketAnimation(Action callback)
	{
		caller = callback;
		rocket.PlayAnimation();
	}

	public void FinishAnimation()
	{
		caller?.Invoke();
	}
}
