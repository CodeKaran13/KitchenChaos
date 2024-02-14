using System;
using System.Collections.Generic;
using System.Linq;
using KIdentify.Scripts.Interfaces;
using Newtonsoft.Json;
using UnityEngine;

namespace KIdentify.Services {

	[CreateAssetMenu(fileName = "CountryCodesManager", menuName = "KIdentify/CountryCodesManager", order = 1)]
	internal class CountryCodesManager : ScriptableObject, IGameService {

		[SerializeField] private TextAsset en_countries;

		private Dictionary<string, string> _countries;
		private Dictionary<string, string> sortedDictionary;
		private List<string> sortedCountries;

		internal IReadOnlyDictionary<string, string> Countries => sortedDictionary;
		internal IReadOnlyList<string> SortedCountries => sortedCountries;

		protected void OnEnable() {
			if (en_countries != null) {
				var countries = JsonConvert.DeserializeObject<CountriesCollection>(en_countries.text).countries;

				_countries = new Dictionary<string, string>(countries.Count);
				string countryToAdd;
				foreach (var country in countries) {
					if (country.Value is string v) {
						countryToAdd = v;
					}
					else if (country.Value is Newtonsoft.Json.Linq.JArray array) {
						// If the value is an array, convert it to a List<string>
						List<string> stringList = array.ToObject<List<string>>();
						countryToAdd = stringList[0];
						//Debug.Log($"[{string.Join(", ", stringList)}]");
					}
					else {
						Debug.Log(country.Value?.ToString() ?? "null");
						countryToAdd = country.Value?.ToString();
					}

					_countries.Add(country.Key, countryToAdd);
					sortedCountries = _countries.Values.OrderBy(value => value).ToList();

					// Create a new dictionary with sorted values
					sortedDictionary = new Dictionary<string, string>();

					// Populate the new dictionary
					foreach (var sortedValue in sortedCountries) {
						// Find the key associated with the sorted value in the original dictionary
						var key = _countries.First(pair => pair.Value == sortedValue).Key;
						// Add the key-value pair to the new dictionary
						sortedDictionary.Add(key, sortedValue);
					}
				}
			}
			else {
				sortedDictionary = null;
			}
		}

		internal string GetCountryCode(string country) {
			foreach (string countryNames in sortedDictionary.Values) {
				if (countryNames.Equals(country)) {
					return sortedDictionary.FirstOrDefault(x => x.Value == countryNames).Key;
				}
			}

			KidLogger.LogError($"Could not find a {country} in {nameof(sortedDictionary)}");
			return null;
		}

		[Serializable]
		private class CountriesCollection {
			public string locale;
			public Dictionary<string, object> countries = new();
		}
	}
}
