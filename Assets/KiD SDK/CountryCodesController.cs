using KiD_SDK.Scripts.Services;
using KiD_SDK.Scripts.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CountryCodesController : MonoBehaviour {
	[SerializeField] private CountryCodesManager _countryCodesManager;

	void Start() {
		ServiceLocator.Current.Register(_countryCodesManager);
	}
}
