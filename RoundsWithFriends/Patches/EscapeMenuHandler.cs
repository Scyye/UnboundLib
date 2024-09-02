using HarmonyLib;


namespace RWF.Patches {
    [HarmonyPatch(typeof(EscapeMenuHandler))]
    public class EscapeMenuHandlerPath {
        [HarmonyPatch("ToggleEsc")]
        [HarmonyPrefix]
        private static bool ToggleEsc() {
            return !(PrivateRoomHandler.instance.IsOpen && !EscapeMenuHandler.isEscMenu); 
        }
    }
}