using Exiled.API.Features;
using FakePlayers.Extensions;
using HarmonyLib;

namespace FakePlayers.Runtime.Harmony
{
    [HarmonyPatch(typeof(CharacterClassManager), nameof(CharacterClassManager.SetClassIDAdv))]
    internal class SetClassIDAdvFix
    {
        public static void Postfix(CharacterClassManager __instance)
        {
            if (__instance.gameObject.IsFakePlayer())
            {
                foreach(Player p in Player.List)
                {
                    if (!p.IsFakePlayer())
                    {
                        p.ReferenceHub.playerStats.TargetSyncHp(p.Connection, p.Health);
                    }
                }
            }
        }
    }
}
