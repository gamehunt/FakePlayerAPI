using HarmonyLib;

namespace FakePlayer.Runtime.Harmony
{
    [HarmonyPatch(typeof(MicroHID), nameof(MicroHID.UpdateServerside))]
    public class MicroHIDFix
    {
        public static bool Prefix(MicroHID __instance)
        {
            return !API.FakePlayer.Dictionary.ContainsKey(__instance.refHub.gameObject);
        }
    }
}