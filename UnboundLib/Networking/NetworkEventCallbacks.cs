using Photon.Pun;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unbound.Core.Networking
{

    public class NetworkEventCallbacks : MonoBehaviourPunCallbacks
    {
        public delegate void NetworkEvent();
        public class PlayerEventArg : EventArgs { public Photon.Realtime.Player Player { get; set; } };
        public delegate void NetworkPlayerEvent<PlayerEventArg>();
        public event NetworkEvent OnJoinedRoomEvent, OnLeftRoomEvent;
        public event Action<PlayerEventArg> OnPlayerLeftRoomEvent;

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
            if (OnPlayerLeftRoomEvent != null)
                OnPlayerLeftRoomEvent(new PlayerEventArg { Player = otherPlayer });
        }
    }
}
