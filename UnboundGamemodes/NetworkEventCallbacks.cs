using Photon.Pun;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unbound.Gamemodes
{
    public class NetworkEventCallbacks : MonoBehaviourPunCallbacks
    {
        public delegate void NetworkEvent();
        public event NetworkEvent OnJoinedRoomEvent, OnLeftRoomEvent;

        public override void OnJoinedRoom()
        {
            OnJoinedRoomEvent?.Invoke();
        }

        public override void OnLeftRoom()
        {
            OnLeftRoomEvent?.Invoke();
        }

        public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
        {
            List<Player> disconnected = PlayerManager.instance.players.Where(p => p.data.view.ControllerActorNr == otherPlayer.ActorNumber).ToList();

            foreach (Player player in disconnected)
                GameModeManager.CurrentHandler.PlayerLeft(player);
        }
    }   
}
