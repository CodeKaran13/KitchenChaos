using System;
using System.Collections.Generic;
using KiD_SDK.Scripts.Interfaces;
using UnityEngine;

namespace KiD_SDK.Scripts.Services
{
    [CreateAssetMenu(fileName = "CountryCodesManager", menuName = "KiD/CountryCodesManager", order = 1)]
    internal class CountryCodesManager : ScriptableObject, IGameService
    {
        [SerializeField]
        private TextAsset _countriesDataJson;

        private Dictionary<string, string> _countries;

        internal IReadOnlyDictionary<string, string> Countries => _countries;


        protected void OnEnable()
        {
            if (_countriesDataJson != null)
            {
                var countries = JsonUtility.FromJson<CountriesData>(_countriesDataJson.text).data;
                _countries = new Dictionary<string, string>(countries.Count);
                foreach (CountryData country in countries)
                {
                    _countries.Add(country.name, country.iso2);
                }
            }
            else
            {
                _countries = null;
            }
        }


        internal string GetCountryCode(string country)
        {
            if (_countries.ContainsKey(country))
            {
                return _countries[country];
            }
            KidLogger.LogError($"Could not find a {country} in {nameof(_countries)}");
            return null;
        }


        [Serializable]
        private class CountryData
        {
            public string name;
            public string iso2;
        }
        

        [Serializable]
        private class CountriesData
        {
            public List<CountryData> data;
        }
    }
}
