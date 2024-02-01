using Kidentify.Scripts.Interfaces;
using Kidentify.PlayerInfo;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Kidentify.Scripts.Interfaces {
	public interface IPlayerStorage : IGameService {
		int PlayersCount { get; }
		KiDPlayer CurrentPlayer { get; set; }

		IEnumerable<KiDPlayer> GetPlayers();
		Task<IEnumerable<KiDPlayer>> LoadPlayers();
		Task<bool> SavePlayerAsync(KiDPlayer player);
	}
}
