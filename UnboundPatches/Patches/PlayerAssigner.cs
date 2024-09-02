﻿using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Unbound.Core;
using Unbound.Core.Extensions;

namespace Unbound.Patches {
    [HarmonyPatch]
    class PlayerAssigner_Patch_CreatePlayer {
        static MethodBase TargetMethod() {
            var nestedTypes = typeof(PlayerAssigner).GetNestedTypes(BindingFlags.Instance | BindingFlags.NonPublic);
            Type nestedCreatePlayerType = null;

            foreach(var type in nestedTypes) {
                if(type.Name.Contains("CreatePlayer")) {
                    nestedCreatePlayerType = type;
                }
            }

            return AccessTools.Method(nestedCreatePlayerType, "MoveNext");
        }

        static void AssignColorID(CharacterData characterData) {
            characterData.player.AssignColorID(characterData.player.teamID);
        }

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            // add player.AssignColorID right after RegisterPlayer
            // this way everything gets colored properly

            var m_registerPlayer = typeof(PlayerAssigner).GetMethodInfo("RegisterPlayer");
            var m_assignColorID = typeof(PlayerAssigner_Patch_CreatePlayer).GetMethodInfo(nameof(PlayerAssigner_Patch_CreatePlayer.AssignColorID));

            foreach(var ins in instructions) {
                if(ins.Calls(m_registerPlayer)) {
                    yield return ins;
                    // load the newly created character data onto the stack (local variable in slot 3) [characterData, ...]
                    yield return new CodeInstruction(OpCodes.Ldloc_3);
                    // call assignColorID which takes the character data off the stack [...]
                    yield return new CodeInstruction(OpCodes.Call, m_assignColorID);
                } else {
                    yield return ins;
                }
            }
        }
    }
}
