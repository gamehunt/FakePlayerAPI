using HarmonyLib;
using UnityEngine;

namespace FakePlayerAPI.Harmony
{
    [HarmonyPatch(typeof(NineTailedFoxAnnouncer), nameof(NineTailedFoxAnnouncer.Update))]
    public class Scp079RecontaimentPatch
    {
        private static bool Prefix(NineTailedFoxAnnouncer __instance)
        {
            if (NineTailedFoxAnnouncer.scpDeaths.Count <= 0)
            {
                return false;
            }
            __instance.scpListTimer += Time.deltaTime;
            if (__instance.scpListTimer <= 1f)
            {
                return false;
            }
            for (int i = 0; i < NineTailedFoxAnnouncer.scpDeaths.Count; i++)
            {
                string text = "";
                for (int j = 0; j < NineTailedFoxAnnouncer.scpDeaths[i].scpSubjects.Count; j++)
                {
                    string text2 = "";
                    string text3 = NineTailedFoxAnnouncer.scpDeaths[i].scpSubjects[j].fullName.Split(new char[]
                    {
                    '-'
                    })[1];
                    for (int k = 0; k < text3.Length; k++)
                    {
                        text2 = text2 + text3[k].ToString() + " ";
                    }
                    if (j == 0)
                    {
                        text = text + "SCP " + text2;
                    }
                    else
                    {
                        text = text + ". SCP " + text2;
                    }
                }
                NineTailedFoxAnnouncer.ScpDeath scpDeath = NineTailedFoxAnnouncer.scpDeaths[i];
                DamageTypes.DamageType damageType = scpDeath.hitInfo.GetDamageType();
                if (damageType == DamageTypes.Tesla)
                {
                    text += "SUCCESSFULLY TERMINATED BY AUTOMATIC SECURITY SYSTEM";
                }
                else if (damageType == DamageTypes.Nuke)
                {
                    text += "SUCCESSFULLY TERMINATED BY ALPHA WARHEAD";
                }
                else if (damageType == DamageTypes.Decont)
                {
                    text += "LOST IN DECONTAMINATION SEQUENCE";
                }
                else
                {
                    CharacterClassManager characterClassManager = null;
                    foreach (GameObject gameObject in PlayerManager.players)
                    {
                        CharacterClassManager component = gameObject.GetComponent<CharacterClassManager>();
                        if (FakePlayer.Dictionary.ContainsKey(gameObject) && !FakePlayer.Dictionary[gameObject].AffectEndConditions)
                        {
                            continue;
                        }
                        int playerId = gameObject.GetComponent<RemoteAdmin.QueryProcessor>().PlayerId;
                        scpDeath = NineTailedFoxAnnouncer.scpDeaths[i];
                        if (playerId == scpDeath.hitInfo.PlayerId)
                        {
                            characterClassManager = gameObject.GetComponent<CharacterClassManager>();
                        }
                    }
                    if (characterClassManager != null)
                    {
                        if (NineTailedFoxAnnouncer.scpDeaths[i].scpSubjects[0].roleId != RoleType.Scp106)
                        {
                            goto IL_207;
                        }
                        scpDeath = NineTailedFoxAnnouncer.scpDeaths[i];
                        if (scpDeath.hitInfo.GetDamageType() != DamageTypes.RagdollLess)
                        {
                            goto IL_207;
                        }
                        string text4 = "CONTAINEDSUCCESSFULLY";
                    IL_213:
                        string str = text4;
                        switch (characterClassManager.CurRole.team)
                        {
                            case Team.MTF:
                                {
                                    Respawning.NamingRules.UnitNamingRule unitNamingRule;
                                    string str2;
                                    if (!Respawning.NamingRules.UnitNamingRules.TryGetNamingRule(Respawning.SpawnableTeamType.NineTailedFox, out unitNamingRule))
                                    {
                                        str2 = "UNKNOWN";
                                    }
                                    else
                                    {
                                        str2 = unitNamingRule.GetCassieUnitName(characterClassManager.CurUnitName);
                                    }
                                    text = text + "CONTAINEDSUCCESSFULLY CONTAINMENTUNIT " + str2;
                                    goto IL_2BB;
                                }
                            case Team.CHI:
                                text = text + str + " BY CHAOSINSURGENCY";
                                goto IL_2BB;
                            case Team.RSC:
                                text = text + str + " BY SCIENCE PERSONNEL";
                                goto IL_2BB;
                            case Team.CDP:
                                text = text + str + " BY CLASSD PERSONNEL";
                                goto IL_2BB;
                            default:
                                text += "SUCCESSFULLY TERMINATED . CONTAINMENTUNIT UNKNOWN";
                                goto IL_2BB;
                        }
                    IL_207:
                        text4 = "TERMINATED";
                        goto IL_213;
                    }
                    text += "SUCCESSFULLY TERMINATED . TERMINATION CAUSE UNSPECIFIED";
                }
            IL_2BB:
                int num = 0;
                bool flag = false;
                foreach (GameObject gameObject2 in PlayerManager.players)
                {
                    CharacterClassManager component = gameObject2.GetComponent<CharacterClassManager>();
                    if (FakePlayer.Dictionary.ContainsKey(gameObject2) && !FakePlayer.Dictionary[gameObject2].AffectEndConditions)
                    {
                        continue;
                    }
                    if (component.CurClass == RoleType.Scp079)
                    {
                        flag = true;
                    }
                    if (component.CurRole.team == Team.SCP)
                    {
                        num++;
                    }
                }
                if (num == 1 && flag && Generator079.mainGenerator.totalVoltage <= 4 && !Generator079.mainGenerator.forcedOvercharge)
                {
                    Generator079.mainGenerator.forcedOvercharge = true;
                    Recontainer079.BeginContainment(true);
                    text += " . ALLSECURED . SCP 0 7 9 RECONTAINMENT SEQUENCE COMMENCING . FORCEOVERCHARGE";
                }
                float num2 = (AlphaWarheadController.Host.timeToDetonation <= 0f) ? 3.5f : 1f;
                __instance.ServerOnlyAddGlitchyPhrase(text, Random.Range(0.1f, 0.14f) * num2, Random.Range(0.07f, 0.08f) * num2);
            }
            __instance.scpListTimer = 0f;
            NineTailedFoxAnnouncer.scpDeaths.Clear();
            return false;
        }
    }
}