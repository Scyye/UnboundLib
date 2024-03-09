using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Unbound.Core;
using Unbound.Core.Extensions;

namespace Unbound.Patches
{
    [HarmonyPatch(typeof(Player), "Start")]
    class Player_Patch_Start
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            /* Removes
             *   GM_ArmsRace instance = GM_ArmsRace.instance;
			 *   instance.StartGameAction += this.GetFaceOffline;
			 * from Player::Start
             */
            var f_gmInstance = AccessTools.Field(typeof(GM_ArmsRace), "instance");
            var f_startGameAction = typeof(GM_ArmsRace).GetFieldInfo("StartGameAction");
            var m_getFace = typeof(Player).GetMethodInfo("GetFaceOffline");

            var list = instructions.ToList();
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].LoadsField(f_gmInstance) &&
                    list[i + 2].LoadsField(f_startGameAction) &&
                    list[i + 4].OperandIs(m_getFace) &&
                    list[i + 8].LoadsField(f_startGameAction))
                {
                    i += 8;
                }
                else
                {
                    yield return list[i];
                }
            }
        }
    }

    
    [HarmonyPatch(typeof(Player), "SetColors")]
    class Player_Patch_SetColors
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var f_playerID = typeof(Player).GetFieldInfo("playerID");
            var m_colorID = typeof(PlayerExtensions).GetMethodInfo(nameof(PlayerExtensions.colorID));

            foreach (var ins in instructions)
            {
                if (ins.LoadsField(f_playerID))
                {
                    // we want colorID instead of teamID
                    yield return new CodeInstruction(OpCodes.Call, m_colorID); // call the colorID method, which pops the player instance off the stack and leaves the result [colorID, ...]
                }
                else
                {
                    yield return ins;
                }
            }
        }
    }
    [HarmonyPatch(typeof(Player), "GetTeamColors")]
    class Player_Patch_GetTeamColors
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var f_playerID = typeof(Player).GetFieldInfo("playerID");
            var m_colorID = typeof(PlayerExtensions).GetMethodInfo(nameof(PlayerExtensions.colorID));

            foreach (var ins in instructions)
            {
                if (ins.LoadsField(f_playerID))
                {
                    // we want colorID instead of teamID
                    yield return new CodeInstruction(OpCodes.Call, m_colorID); // call the colorID method, which pops the player instance off the stack and leaves the result [colorID, ...]
                }
                else
                {
                    yield return ins;
                }
            }
        }
    }
}
