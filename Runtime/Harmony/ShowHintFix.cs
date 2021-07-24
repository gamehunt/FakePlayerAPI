using HarmonyLib;

namespace FakePlayer.Runtime.Harmony
{
    [HarmonyPatch(typeof(Hints.HintDisplay), nameof(Hints.HintDisplay.Show))]
    public class ShowHintFix
    {
        private static bool Prefix(Hints.HintDisplay __instance, Hints.Hint hint)
        {
            return __instance.netIdentity.connectionToClient != null;
        }
    }
}