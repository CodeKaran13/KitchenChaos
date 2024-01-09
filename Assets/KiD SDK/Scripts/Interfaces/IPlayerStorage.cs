using KiD_SDK.Scripts.Interfaces;
using Kidentify.PlayerInfo;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Assets.KiD_SDK.Scripts.Interfaces
{
    public interface IPlayerStorage : IGameService
    {
        int PlayersCount { get; }
        KiDPlayer CurrentPlayer { get; set; }

        IEnumerable<KiDPlayer> GetPlayers();
        Task<IEnumerable<KiDPlayer>> LoadPlayers();
        Task<bool> SavePlayerAsync(KiDPlayer player);
    }
}
