using HarmonyLib;
using Landfall.Network;
using Steamworks;

namespace Unbound.Networking.Lobbies{
    [HarmonyPatch(typeof(ClientSteamLobby), "ShowInviteScreenWhenConnected")]
    class ClientSteamLobby_Patch_ShowInviteScreenWhenConnected
    {
        static bool Prefix(ClientSteamLobby __instance)
        {
            // Allow inviting multiple times in the same room
            if (__instance.CurrentLobby != CSteamID.Nil)
            {
                SteamFriends.ActivateGameOverlayInviteDialog(__instance.CurrentLobby);
                return false;
            }

            return true;
        }
    }
}
