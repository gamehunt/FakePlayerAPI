using HarmonyLib;
using UnityEngine;

namespace FakePlayerAPI.Harmony
{
    [HarmonyPatch(typeof(NineTailedFoxAnnouncer), nameof(NineTailedFoxAnnouncer.CheckForZombies))]
    internal class Scp079RecontaimentPatch2
    {
        private static bool Prefix(GameObject zombie)
        {
            int num = 0;
            foreach (GameObject gameObject in PlayerManager.players)
            {
                if (FakePlayer.Dictionary.ContainsKey(gameObject) && !FakePlayer.Dictionary[gameObject].AffectEndConditions)
                {
                    continue;
                }
                if (!(gameObject == zombie))
                {
                    ReferenceHub hub = ReferenceHub.GetHub(gameObject);
                    if (hub.characterClassManager.CurClass != RoleType.Scp079 && hub.characterClassManager.CurRole.team == Team.SCP)
                    {
                        num++;
                    }
                }
            }
            if (num <= 0 && Generator079.mainGenerator.totalVoltage <= 4 && !Generator079.mainGenerator.forcedOvercharge)
            {
                Generator079.mainGenerator.forcedOvercharge = true;
                Recontainer079.BeginContainment(true);
                NineTailedFoxAnnouncer.singleton.ServerOnlyAddGlitchyPhrase("ALLSECURED . SCP 0 7 9 RECONTAINMENT SEQUENCE COMMENCING . FORCEOVERCHARGE", 0.1f, 0.07f);
            }
            return false;
        }
    }
}