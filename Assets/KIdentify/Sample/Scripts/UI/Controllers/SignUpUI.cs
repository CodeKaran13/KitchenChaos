using System.Collections.Generic;
using UnityEngine;
using TMPro;
using KIdentify.Services;
using KIdentify.Sample.Tools;
using System.Linq;
using System.Globalization;
using System;

namespace KIdentify.Sample.UI
{
	public class SignUpUI : BaseUI
	{
		[SerializeField] private TMP_Dropdown locationDropdown;
		[SerializeField] private TMP_Dropdown birthMonthDropdown;
		[SerializeField] private TMP_Dropdown birthYearDropdown;

		private const int dobYears = 120;

		private void OnEnable()
		{
			locationDropdown.onValueChanged.AddListener(delegate { OnLocationValueChangedManually(); });
		}

		private void OnDisable()
		{
			locationDropdown.onValueChanged.RemoveAllListeners();
		}

		public override void ShowUI()
		{
			Initialize();
			base.ShowUI();
		}

		public override void HideUI()
		{
			base.HideUI();
		}

		#region BUTTON ONCLICK

		public void OnSignUpButtonClick()
		{
			if (locationDropdown != null)
			{
				KiDManager.Location = locationDropdown.options[locationDropdown.value].text;
			}

			if (birthMonthDropdown != null && birthYearDropdown != null)
			{
				int year = int.Parse(birthYearDropdown.options[birthYearDropdown.value].text);
				DateTime parsedDate = DateTime.ParseExact(birthMonthDropdown.options[birthMonthDropdown.value].text, "MMMM", CultureInfo.InvariantCulture);
				int month = parsedDate.Month;
				int date = DateTime.DaysInMonth(year, month);

				KiDManager.DateOfBirth = new DateTime(year, month, date);
			}

			Debug.Log($"Location: {KiDManager.Location}, DOB: {KiDManager.DateOfBirth}");
			KiDManager.Instance.AgeGateCheck();
		}

		#endregion

		private async void Initialize()
		{
			var locationManager = ServiceLocator.Current.Get<LocationManager>();
			var countries = ServiceLocator.Current.Get<CountryCodesManager>().Countries.Values.ToList();
			locationDropdown.options = new List<TMP_Dropdown.OptionData>(countries.Count);
			for (int i = 0; i < countries.Count; ++i)
			{
				locationDropdown.options.Add(new TMP_Dropdown.OptionData(countries[i]));
			}
			locationDropdown.SetValueWithoutNotify(0);
			locationDropdown.RefreshShownValue();

			var (countryCode, regionCode) = await locationManager.GetLocationByIP();
			KiDManager.Instance.CurrentPlayer.CountryCode = countryCode;
			KiDManager.Instance.CurrentPlayer.RegionCode = regionCode;
			int countryIndex = locationManager.GetCountryIndex(countryCode);
			if (locationDropdown != null && countryIndex >= 0 && countryIndex < locationDropdown.options.Count)
			{
				locationDropdown.value = countryIndex;
			}

			birthYearDropdown.options = new List<TMP_Dropdown.OptionData>(dobYears);
			for (int i = 0; i < dobYears; ++i)
			{
				birthYearDropdown.options.Add(new TMP_Dropdown.OptionData((2023 - i).ToString()));
			}
			birthYearDropdown.SetValueWithoutNotify(0);
			birthYearDropdown.RefreshShownValue();
		}

		private void OnLocationValueChangedManually()
		{
			KiDManager.Instance.CurrentPlayer.RegionCode = "";
		}
	}
}