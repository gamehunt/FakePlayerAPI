﻿using Exiled.Events.EventArgs;
using Exiled.Events.Handlers;
using GameCore;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace FakePlayerAPI.Harmony
{
    [HarmonyPatch(typeof(RoundSummary), nameof(RoundSummary.Start))]
    internal class RoundSummaryFix
    {
        private static readonly MethodInfo CustomProcess = SymbolExtensions.GetMethodInfo(() => Process(null));

        private static IEnumerator<float> Process(RoundSummary instance)
        {
            RoundSummary roundSummary = instance;
            while (roundSummary != null)
            {
                int count = PlayerManager.players.Count - FakePlayer.Dictionary.Keys.Count;
                while (RoundSummary.RoundLock || !RoundSummary.RoundInProgress() || ((roundSummary._keepRoundOnOne && count < 2)))
                {
                    if (count == 0 && FakePlayer.Dictionary.Keys.Count > 0)
                    {
                        foreach (FakePlayer n in FakePlayer.List)
                        {
                            n.Kill(false);
                        }
                    }
                    count = PlayerManager.players.Count - FakePlayer.Dictionary.Keys.Count;
                    yield return 0.0f;
                }
                yield return 0.0f;
                RoundSummary.SumInfo_ClassList newList = default;
                foreach (GameObject player in PlayerManager.players)
                {
                    if (!(player == null))
                    {
                        FakePlayer npc = FakePlayer.Get(player);
                        if (npc == null || npc.AffectEndConditions)
                        {
                            CharacterClassManager component = player.GetComponent<CharacterClassManager>();
                            if (component.Classes.CheckBounds(component.CurClass))
                            {
                                switch (component.Classes.SafeGet(component.CurClass).team)
                                {
                                    case Team.SCP:
                                        if (component.CurClass == RoleType.Scp0492)
                                        {
                                            ++newList.zombies;
                                            continue;
                                        }

                                        ++newList.scps_except_zombies;
                                        continue;
                                    case Team.MTF:
                                        ++newList.mtf_and_guards;
                                        continue;
                                    case Team.CHI:
                                        ++newList.chaos_insurgents;
                                        continue;
                                    case Team.RSC:
                                        ++newList.scientists;
                                        continue;
                                    case Team.CDP:
                                        ++newList.class_ds;
                                        continue;
                                    default:
                                        continue;
                                }
                            }
                        }
                    }
                }

                newList.warhead_kills = AlphaWarheadController.Host.detonated ? AlphaWarheadController.Host.warheadKills : -1;
                yield return float.NegativeInfinity;
                newList.time = (int)Time.realtimeSinceStartup;
                yield return float.NegativeInfinity;
                RoundSummary.roundTime = newList.time - roundSummary.classlistStart.time;
                int num1 = newList.mtf_and_guards + newList.scientists;
                int num2 = newList.chaos_insurgents + newList.class_ds;
                int num3 = newList.scps_except_zombies + newList.zombies;
                float num4 = roundSummary.classlistStart.class_ds == 0 ? 0.0f : (RoundSummary.escaped_ds + newList.class_ds) / roundSummary.classlistStart.class_ds;
                float num5 = roundSummary.classlistStart.scientists == 0 ? 1f : (RoundSummary.escaped_scientists + newList.scientists) / roundSummary.classlistStart.scientists;

                if (newList.class_ds == 0 && num1 == 0)
                {
                    roundSummary._roundEnded = true;
                }
                else
                {
                    int num6 = 0;
                    if (num1 > 0)
                        ++num6;
                    if (num2 > 0)
                        ++num6;
                    if (num3 > 0)
                        ++num6;
                    if (num6 <= 1)
                    {
                        roundSummary._roundEnded = true;
                    }
                }

                var endingRoundEventArgs = new EndingRoundEventArgs(Exiled.API.Enums.LeadingTeam.Draw, newList, roundSummary._roundEnded);

                if (num1 > 0)
                {
                    if (RoundSummary.escaped_ds == 0 && RoundSummary.escaped_scientists != 0)
                        endingRoundEventArgs.LeadingTeam = Exiled.API.Enums.LeadingTeam.FacilityForces;
                }
                else
                {
                    endingRoundEventArgs.LeadingTeam = RoundSummary.escaped_ds != 0 ? Exiled.API.Enums.LeadingTeam.ChaosInsurgency : Exiled.API.Enums.LeadingTeam.Anomalies;
                }

                Server.OnEndingRound(endingRoundEventArgs);

                roundSummary._roundEnded = endingRoundEventArgs.IsRoundEnded && endingRoundEventArgs.IsAllowed;

                if (roundSummary._roundEnded)
                {
                    FriendlyFireConfig.PauseDetector = true;
                    string str = "Round finished! Anomalies: " + num3 + " | Chaos: " + num2 + " | Facility Forces: " + num1 + " | D escaped percentage: " + num4 + " | S escaped percentage: : " + num5;
                    GameCore.Console.AddLog(str, Color.gray, false);
                    ServerLogs.AddLog(ServerLogs.Modules.Logger, str, ServerLogs.ServerLogType.GameEvent);
                    byte i1;
                    for (i1 = 0; i1 < 75; ++i1)
                        yield return 0.0f;
                    int timeToRoundRestart = Mathf.Clamp(ConfigFile.ServerConfig.GetInt("auto_round_restart_time", 10), 5, 1000);

                    if (roundSummary != null)
                    {
                        newList.scps_except_zombies -= newList.zombies;

                        var roundEndedEventArgs = new RoundEndedEventArgs(endingRoundEventArgs.LeadingTeam, newList, timeToRoundRestart);

                        Server.OnRoundEnded(roundEndedEventArgs);

                        roundSummary.RpcShowRoundSummary(roundSummary.classlistStart, roundEndedEventArgs.ClassList, (RoundSummary.LeadingTeam)roundEndedEventArgs.LeadingTeam, RoundSummary.escaped_ds, RoundSummary.escaped_scientists, RoundSummary.kills_by_scp, roundEndedEventArgs.TimeToRestart);
                    }

                    for (int i2 = 0; i2 < 50 * (timeToRoundRestart - 1); ++i2)
                        yield return 0.0f;
                    roundSummary.RpcDimScreen();
                    for (i1 = 0; i1 < 50; ++i1)
                        yield return 0.0f;
                    PlayerManager.localPlayer.GetComponent<PlayerStats>().Roundrestart();
                    yield break;
                }
            }
        }

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.opcode == OpCodes.Call)
                {
                    if (instruction.operand != null
                        && instruction.operand is MethodBase methodBase
                        && methodBase.Name != nameof(RoundSummary._ProcessServerSideCode))
                    {
                        yield return instruction;
                    }
                    else
                    {
                        yield return new CodeInstruction(OpCodes.Call, CustomProcess);
                    }
                }
                else
                {
                    yield return instruction;
                }
            }
        }
    }
}