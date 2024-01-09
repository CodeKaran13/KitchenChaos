using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using KiD_SDK.Scripts.Services;
using KiD_SDK.Scripts.Tools;
using System.Linq;
using System.Globalization;
using System;

public class SignUpUI : BaseUI {

	[SerializeField] private TMP_Dropdown locationDropdown;
	[SerializeField] private TMP_Dropdown birthMonthDropdown;
	[SerializeField] private TMP_Dropdown birthYearDropdown;

	private const int dobYears = 120;

	public override void ShowUI() {
		Initialize();
		base.ShowUI();
	}

	public override void HideUI() {
		base.HideUI();
	}

	#region BUTTON ONCLICK

	public void OnSignUpButtonClick() {
		if (locationDropdown != null) {
			KiDManager.Location = locationDropdown.options[locationDropdown.value].text;
		}

		if (birthMonthDropdown != null && birthYearDropdown != null) {
			int year = int.Parse(birthYearDropdown.options[birthYearDropdown.value].text);
			DateTime parsedDate = DateTime.ParseExact(birthMonthDropdown.options[birthMonthDropdown.value].text, "MMMM", CultureInfo.InvariantCulture);
			int month = parsedDate.Month;

			KiDManager.DateOfBirth = new DateTime(year, month, 1);
		}

		Debug.Log($"Location: {KiDManager.Location}, DOB: {KiDManager.DateOfBirth}");
		KiDManager.Instance.CreateSession();
	}

	#endregion

	private void Initialize() {
		var countries = ServiceLocator.Current.Get<CountryCodesManager>().Countries.Keys.ToList();
		locationDropdown.options = new List<TMP_Dropdown.OptionData>(countries.Count);
		for (int i = 0; i < countries.Count; ++i) {
			locationDropdown.options.Add(new TMP_Dropdown.OptionData(countries[i]));
		}
		locationDropdown.SetValueWithoutNotify(0);
		locationDropdown.RefreshShownValue();

		SetLocationByIP();

		birthYearDropdown.options = new List<TMP_Dropdown.OptionData>(dobYears);
		for (int i = 0; i < dobYears; ++i) {
			birthYearDropdown.options.Add(new TMP_Dropdown.OptionData((2023 - i).ToString()));
		}
		birthYearDropdown.SetValueWithoutNotify(0);
		birthYearDropdown.RefreshShownValue();
	}

	private async void SetLocationByIP() {
		var ipManager = new CountryAndIPManager();
		string country = await ipManager.FindCountryByExternalIP();
		int countryIndex = GetCountryIndex(country);

		if (locationDropdown != null && countryIndex >= 0 && countryIndex < locationDropdown.options.Count) {
			locationDropdown.value = countryIndex;
		}
	}

	private int GetCountryIndex(string countryCode) {
		int defaultResult = 0;

		var countryCodesManager = ServiceLocator.Current.Get<CountryCodesManager>();
		var countries = countryCodesManager.Countries.ToList();

		int index = countries.FindIndex(cnt => cnt.Value == countryCode);

		if (index == -1) {
			return defaultResult;
		}

		return index;
	}
}
