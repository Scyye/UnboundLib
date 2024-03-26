using BepInEx;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unbound.Core;

namespace Unbound.Patches
{

    [BepInDependency("dev.rounds.unbound.core")]
    [BepInPlugin("dev.rounds.unbound.patches", "Unbound Lib Patches", "1.0.0")]
    public class UnboundPatches : BaseUnityPlugin
    {
        private void Awake()
        {
            this.PatchAll();
        }
    }
}
