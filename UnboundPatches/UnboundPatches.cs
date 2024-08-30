using BepInEx;
using Unbound.Core;

namespace Unbound.Patches {

    [BepInDependency("dev.rounds.unbound.core")]
    [BepInPlugin("dev.rounds.unbound.patches", "Unbound Lib Patches", "1.0.0")]
    public class UnboundPatches:BaseUnityPlugin {
        private void Awake() {
            this.PatchAll();
        }
    }
}
