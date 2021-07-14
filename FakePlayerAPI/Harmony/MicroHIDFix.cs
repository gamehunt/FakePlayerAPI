using HarmonyLib;

namespace FakePlayerAPI.Harmony
{
    [HarmonyPatch(typeof(MicroHID), nameof(MicroHID.UpdateServerside))]
    public class MicroHIDFix
    {
        public static bool Prefix(MicroHID __instance)
        {
            return !FakePlayer.Dictionary.ContainsKey(__instance.refHub.gameObject);
        }
    }
}