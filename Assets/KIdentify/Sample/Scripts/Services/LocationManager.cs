using System;
using System.Linq;
using System.Threading.Tasks;
using KIdentify.Sample.Interfaces;
using KIdentify.Sample.Tools;
using UnityEngine;

namespace KIdentify.Services
{
	public class LocationManager : IGameService
	{
		public LocationManager() { }

		public async Task<(string, string)> GetLocationByIP()
		{
			try
			{
				var ipManager = new CountryAndIPManager();
				string countryInfo = await ipManager.FindCountryByExternalIP();

				if (!string.IsNullOrEmpty(countryInfo))
				{
					string[] countryAndRegion = countryInfo.Split("-");
					string countryCode = string.Empty;
					string regionCode = string.Empty;
					if (countryAndRegion.Length >= 2)
					{
						countryCode = countryAndRegion[0];
						regionCode = countryAndRegion[1];
					}
					else
					{
						Debug.LogWarning($"Invalid country information format: Found no regionCode: {countryInfo}");
						countryCode = countryAndRegion[0];
					}
					return (countryCode, regionCode);
				}
				else
				{
					Debug.LogWarning("Failed to retrieve country information from IPAPI.");
					return (string.Empty, string.Empty);
				}
			}
			catch (Exception ex)
			{
				Debug.LogError($"An error occurred while setting location by IP: {ex.Message}");
				return (string.Empty, string.Empty);
			}
		}

		public int GetCountryIndex(string countryCode)
		{
			int defaultResult = 0;

			var countryCodesManager = ServiceLocator.Current.Get<CountryCodesManager>();
			var countries = countryCodesManager.Countries.ToList();

			int index = countries.FindIndex(cnt => cnt.Key == countryCode);

			if (index == -1)
			{
				return defaultResult;
			}

			return index;
		}

		public string GetCountry(string countryCode)
		{
			int defaultResult = 0;

			var countryCodesManager = ServiceLocator.Current.Get<CountryCodesManager>();
			var countries = countryCodesManager.Countries.ToList();

			string countryName;

			int index = countries.FindIndex(cnt => cnt.Key == countryCode);

			if (index == -1)
			{
				countryName = countries[defaultResult].Value;
			}
			countryName = countries[index].Value;
			return countryName;
		}

		public bool TryConvertCountryStringToCode(string country, out string code)
		{
			code = ServiceLocator.Current.Get<CountryCodesManager>().GetCountryCode(country);
			return !string.IsNullOrEmpty(code);
		}
	}

}