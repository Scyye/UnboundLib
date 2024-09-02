using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;
using Unbound.Core;
using Unbound.Core.Extensions;


namespace Unbound.Patches {
    [HarmonyPatch(typeof(PlayerManager), nameof(PlayerManager.GetColorFromPlayer))]
    class PlayerManager_Patch_GetColorFromPlayer {
        static void Prefix(ref int playerID) {
            playerID = PlayerManager.instance.players[playerID].colorID();
        }
    }
    [HarmonyPatch(typeof(PlayerManager), nameof(PlayerManager.GetColorFromTeam))]
    class PlayerManager_Patch_GetColorFromTeam {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            var f_playerID = typeof(Player).GetFieldInfo("playerID");
            var m_colorID = typeof(PlayerExtensions).GetMethodInfo(nameof(PlayerExtensions.colorID));

            foreach(var ins in instructions) {
                if(ins.LoadsField(f_playerID)) {
                    // we want colorID instead of teamID
                    yield return new CodeInstruction(OpCodes.Call, m_colorID); // call the colorID method, which pops the player instance off the stack and leaves the result [colorID, ...]
                } else {
                    yield return ins;
                }
            }
        }
    }
}
