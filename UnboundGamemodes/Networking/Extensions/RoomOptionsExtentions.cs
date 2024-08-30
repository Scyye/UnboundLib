using Photon.Realtime;
using System.Linq;

namespace Unbound.Networking.Extensions{
    public static class RoomOptionsExtentions
    {

        public static RoomOptions Clone(this RoomOptions original)
        {
            RoomOptions copy = new RoomOptions();
            copy.MaxPlayers = original.MaxPlayers;
            copy.PlayerTtl = original.PlayerTtl;
            copy.EmptyRoomTtl = original.EmptyRoomTtl;
            copy.CustomRoomProperties = new ExitGames.Client.Photon.Hashtable();
            original.CustomRoomProperties.ForEach(p => copy.CustomRoomProperties.Add(p.Key, p.Value));
            copy.CustomRoomPropertiesForLobby = original.CustomRoomPropertiesForLobby?.ToArray();
            copy.Plugins = original.Plugins?.ToArray();
            copy.IsVisible = original.IsVisible;
            copy.IsOpen = original.IsOpen;
            copy.CleanupCacheOnLeave = original.CleanupCacheOnLeave;
            copy.SuppressRoomEvents = original.SuppressRoomEvents;
            copy.SuppressPlayerInfo = original.SuppressPlayerInfo;
            copy.PublishUserId = original.PublishUserId;
            copy.DeleteNullProperties = original.DeleteNullProperties;
            copy.BroadcastPropsChangeToAll = original.BroadcastPropsChangeToAll;
            return copy;
        }

    }
}
