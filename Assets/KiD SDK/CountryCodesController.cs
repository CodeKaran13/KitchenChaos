using Kidentify.Scripts.Tools;
using Kidentify.Scripts.Services;
using UnityEngine;

public class CountryCodesController : MonoBehaviour {
	[SerializeField] private CountryCodesManager _countryCodesManager;

	void Start() {
		ServiceLocator.Current.Register(_countryCodesManager);
	}
}
