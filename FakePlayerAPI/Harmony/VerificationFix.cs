using HarmonyLib;

namespace FakePlayerAPI.Harmony
{
    [HarmonyPatch(typeof(CharacterClassManager), nameof(CharacterClassManager.Init))]
    internal class VerificationFix
    {
        private static bool Prefix(CharacterClassManager __instance)
        {
            if (!FakePlayer.Dictionary.ContainsKey(__instance.gameObject))
            {
                return true;
            }
            __instance.IsVerified = true;
            return false;
        }
    }
}