using Assets.KiD_SDK.Scripts.Interfaces;
using KiD_SDK.Scripts.Exceptions;
using Kidentify.PlayerInfo;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.KiD_SDK.Scripts.Services {
	[System.Serializable]
	public class PlayerDataDict {
		public Dictionary<string, KiDPlayer> players;
	}

	internal class PlayerStorage : IPlayerStorage {
		private Dictionary<string, KiDPlayer> _players;
		private KiDPlayer _currentPlayer;
		private readonly string _playersPath;

		public PlayerStorage() {
			_players = new();

			_playersPath = Path.Combine(Application.persistentDataPath, "kid_sdk_accounts.json");
		}

		public KiDPlayer CurrentPlayer { get => _currentPlayer; set => _currentPlayer = value; }

		public int PlayersCount {
			get {
				return _players.Values.Count();
			}
		}

		public IEnumerable<KiDPlayer> GetPlayers() {
			return _players.Values;
		}

		public async Task<IEnumerable<KiDPlayer>> LoadPlayers() {
			try {
				if (!File.Exists(_playersPath)) {
					throw new KiDException("No players db found");
				}
				using (StreamReader reader = new(_playersPath)) {
					string json = await reader.ReadToEndAsync();
					PlayerDataDict wrapper = JsonUtility.FromJson<PlayerDataDict>(json);
					_players = wrapper.players;

					return _players.Values;
				}
			}
			catch (ArgumentException ex) {
				Debug.LogError($"Error loading players: {ex.Message}");
				return null;
			}
		}

		public async Task<bool> SavePlayerAsync(KiDPlayer player) {
			if (String.IsNullOrEmpty(player.SessionId)) {
				Slug.LogT2(this, "Player id is null or empty");
				return false;
			}

			Slug.LogT2(this, "Saving player: " + player.ToJson());

			//Find player in players list and change it or add new

			if (_players.ContainsKey(player.SessionId)) {
				_players[player.SessionId] = player;
			}
			else {
				_players.Add(player.SessionId, player);
			}

			try {
				var wrapper = new PlayerDataDict { players = _players };
				await SavePlayersAsync(wrapper);
			}
			catch (ArgumentException e) {
				Slug.LogT2(this, "Error saving player: " + e.Message);
				return false;
			}

			return true;
		}

		private async Task SavePlayersAsync(PlayerDataDict players) {
			string json = JsonUtility.ToJson(players);

			using (StreamWriter writer = new(_playersPath, false, Encoding.UTF8)) {
				await writer.WriteAsync(json);
			}
		}
	}
}
