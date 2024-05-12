using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;
using Unbound.Core.Extensions;
using System.Linq;
using Unbound.Core;

namespace Unbound.Patches
{
    [HarmonyPatch(typeof(PointVisualizer), "DoShowPoints")]
    class PointVisualizer_Patch_DoShowPoints
    {
        static int GetColorIDFromPlayerID(int playerID)
        {
            return PlayerManager.instance.players[playerID].colorID();
        }
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var m_GetPlayerSkinColors = typeof(PlayerSkinBank).GetMethodInfo(nameof(PlayerSkinBank.GetPlayerSkinColors));
            var m_getColorID = typeof(PointVisualizer_Patch_DoShowPoints).GetMethodInfo(nameof(GetColorIDFromPlayerID));

            foreach (var ins in instructions)
            {
                if (ins.Calls(m_GetPlayerSkinColors))
                {
                    // we want colorID instead of 0/1
                    yield return new CodeInstruction(OpCodes.Call, m_getColorID); // call the colorID method, which pops the constant 0/1 off the stack and leaves the result [colorID, ...]
                }

                yield return ins;

            }
        }
    }
}
