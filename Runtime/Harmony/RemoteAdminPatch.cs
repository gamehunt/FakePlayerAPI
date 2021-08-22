using Exiled.API.Features;
using HarmonyLib;
using NorthwoodLib.Pools;
using RemoteAdmin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using UnityEngine;
using static HarmonyLib.AccessTools;

namespace FakePlayers.Runtime.Harmony
{
    [HarmonyPatch(typeof(CommandProcessor), nameof(CommandProcessor.ProcessQuery))]
    internal class RemoteAdminPatch
    {
        public static GameObject FindFakePlayer(string[] query)
        {
            try
            {
                return string.IsNullOrEmpty(query[2]) ? null : Player.Get(int.Parse(query[2]))?.GameObject;
            }catch(Exception)
            {
                return null;
            }
        }

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var newInstructions = ListPool<CodeInstruction>.Shared.Rent(instructions);


            const int prefixOffset = 3;
            int prefixIndex = newInstructions.FindIndex(inst => inst.opcode == OpCodes.Ldstr && inst.operand.ToString() == "<link=RA_RaEverywhere><color=white>[<color=#EFC01A></color><color=white>]</color></link> ") + prefixOffset;

            var skipStrLabel = generator.DefineLabel();

            Log.Debug($"prefixIndex: {prefixIndex}", Plugin.Instance.Config.VerboseOutput);

            //Prefix patch
            newInstructions.InsertRange(prefixIndex, new[]
            {
                 new CodeInstruction(OpCodes.Call, PropertyGetter(typeof(ServerRoles), nameof(ServerRoles.gameObject))),
                 new CodeInstruction(OpCodes.Call, Method(typeof(Extensions.Extensions), nameof(Extensions.Extensions.IsFakePlayer), new System.Type[] { typeof(GameObject) })),
                 new CodeInstruction(OpCodes.Brfalse_S, skipStrLabel),
                 new CodeInstruction(OpCodes.Ldstr, "[FAKE] "),
                 new CodeInstruction(OpCodes.Stloc_S, 18),
                 new CodeInstruction(OpCodes.Ldloc_S, 20).WithLabels(skipStrLabel),
            });

            const int loopStartOffset = 1;
            int loopStartIndex = newInstructions.FindIndex(inst => inst.opcode == OpCodes.Ldloc_S && inst.operand.ToString().Equals("ReferenceHub (16)")) + loopStartOffset;

            if (loopStartIndex != -1)
            {
                Log.Debug($"loopStartIndex: {loopStartIndex}", Plugin.Instance.Config.VerboseOutput);

                const int endIndexOffset = 3;
                int endIndex = newInstructions.FindLastIndex(inst => inst.opcode == OpCodes.Ldstr && inst.operand.ToString() == "\n") + endIndexOffset;

                if(endIndex != endIndexOffset - 1)
                {
                    Log.Debug($"endIndex: {endIndex}", Plugin.Instance.Config.VerboseOutput);

                    var skipLabel2 = generator.DefineLabel();
                    var loopRetLabel = newInstructions[endIndex].labels.First();

                    //Patch for hiding from RA
                    newInstructions.InsertRange(loopStartIndex, new[]
                    {
                        new CodeInstruction(OpCodes.Call, Method(typeof(Extensions.Extensions), nameof(Extensions.Extensions.IsFakePlayer), new System.Type[] { typeof(ReferenceHub) })),
                        new CodeInstruction(OpCodes.Brfalse_S, skipLabel2),
                        new CodeInstruction(OpCodes.Ldloc_S, 16),
                        new CodeInstruction(OpCodes.Call, Method(typeof(Extensions.Extensions), nameof(Extensions.Extensions.AsFakePlayer), new System.Type[] { typeof(ReferenceHub) })),
                        new CodeInstruction(OpCodes.Callvirt, PropertyGetter(typeof(API.FakePlayer), nameof(API.FakePlayer.DisplayInRA))),
                        new CodeInstruction(OpCodes.Brfalse_S, loopRetLabel),
                        new CodeInstruction(OpCodes.Ldloc_S, 16).WithLabels(skipLabel2),
                    });
                }
                else
                {
                    Log.Error("Failed to find endIndexOffset, go bonk gamehunt in discord!");
                }

            }
            else
            {
                Log.Error("Failed to find loopStartOffset, go bonk gamehunt in discord!");
            }

            //Auth token && RA list patch 2
            //FIXME crashes server rn
            const int query_offset = -3;
            const int retryLabelOffset = -7;
            
            int shortQueryIndex = newInstructions.FindLastIndex(inst => inst.opcode == OpCodes.Ldstr && inst.operand.ToString() == ":PLAYER#Player with id ") + query_offset;
            
            var shortQueryRetryLabel = generator.DefineLabel();
            newInstructions[shortQueryIndex + retryLabelOffset] = newInstructions[shortQueryIndex + retryLabelOffset].WithLabels(shortQueryRetryLabel);

            Log.Debug($"shortQueryIndex: {shortQueryIndex}", Plugin.Instance.Config.VerboseOutput);
            Log.Debug($"shortQueryRetryLabel: {shortQueryIndex + retryLabelOffset}", Plugin.Instance.Config.VerboseOutput);

            var tmpQueryStorage = generator.DeclareLocal(typeof(string[]));

            newInstructions.InsertRange(shortQueryIndex, new[]
            {
                new CodeInstruction(OpCodes.Stloc_S, tmpQueryStorage),
                new CodeInstruction(OpCodes.Ldloc_S, tmpQueryStorage),
                new CodeInstruction(OpCodes.Call, Method(typeof(RemoteAdminPatch), nameof(FindFakePlayer))),
                new CodeInstruction(OpCodes.Stloc_S, 38),
                new CodeInstruction(OpCodes.Ldloc_S, 38),
                new CodeInstruction(OpCodes.Brtrue_S, shortQueryRetryLabel),
                new CodeInstruction(OpCodes.Ldloc_S, tmpQueryStorage)
            });

            var raListRetryLabel = generator.DefineLabel();

            int raListIndex = newInstructions.FindIndex(inst => inst.opcode == OpCodes.Ldstr && inst.operand.ToString() == ":PLAYER#Player with id ") + query_offset;
            newInstructions[raListIndex + retryLabelOffset] = newInstructions[raListIndex + retryLabelOffset].WithLabels(raListRetryLabel);

            Log.Debug($"raListIndex: {raListIndex}", Plugin.Instance.Config.VerboseOutput);
            Log.Debug($"raListRetryLabel: {raListIndex + retryLabelOffset}", Plugin.Instance.Config.VerboseOutput);

            newInstructions.InsertRange(raListIndex, new[]
            {
                new CodeInstruction(OpCodes.Stloc_S, tmpQueryStorage),
                new CodeInstruction(OpCodes.Ldloc_S, tmpQueryStorage),
                new CodeInstruction(OpCodes.Call, Method(typeof(RemoteAdminPatch), nameof(FindFakePlayer))),
                new CodeInstruction(OpCodes.Stloc_S, 22),
                new CodeInstruction(OpCodes.Ldloc_S, 22),
                new CodeInstruction(OpCodes.Brtrue_S, raListRetryLabel),
                new CodeInstruction(OpCodes.Ldloc_S, tmpQueryStorage)
            });

            // Fake player note patch
            const int colorIndexOffset = 4;
            int ldstrColorIndex = newInstructions.FindLastIndex(inst => inst.opcode == OpCodes.Ldstr && inst.operand.ToString() == "\n<color=#D4AF37>* GameplayData permission required</color>") + colorIndexOffset;
            Log.Debug($"ldstrColorIndex: {ldstrColorIndex}", Plugin.Instance.Config.VerboseOutput);

            var noteSkipLabel = generator.DefineLabel();

            newInstructions.InsertRange(ldstrColorIndex, new[]
            {
                new CodeInstruction(OpCodes.Ldloc_S, 22),
                new CodeInstruction(OpCodes.Call, Method(typeof(Extensions.Extensions), nameof(Extensions.Extensions.IsFakePlayer), new Type[]{ typeof(GameObject) })),
                new CodeInstruction(OpCodes.Brfalse_S, noteSkipLabel),
                new CodeInstruction(OpCodes.Ldstr, "\n<color=#FEAF04>This is fake player instance created with FakePlayerAPI</color>"),
                new CodeInstruction(OpCodes.Callvirt, Method(typeof(StringBuilder), nameof(StringBuilder.Append), new Type[]{ typeof(string)})),
                new CodeInstruction(OpCodes.Pop),
                new CodeInstruction(OpCodes.Ldloc_S, 33),
                new CodeInstruction(OpCodes.Ldstr, "\n<color=#FEAF04>Debug identifier: "),
                new CodeInstruction(OpCodes.Callvirt, Method(typeof(StringBuilder), nameof(StringBuilder.Append), new Type[]{ typeof(string)})),
                new CodeInstruction(OpCodes.Pop),
                new CodeInstruction(OpCodes.Ldloc_S, 33),
                new CodeInstruction(OpCodes.Ldloc_S, 22),
                new CodeInstruction(OpCodes.Call, Method(typeof(Extensions.Extensions), nameof(Extensions.Extensions.AsFakePlayer), new Type[]{ typeof(GameObject) })),
                new CodeInstruction(OpCodes.Callvirt, Method(typeof(API.FakePlayer), nameof(API.FakePlayer.GetIdentifier))),
                new CodeInstruction(OpCodes.Callvirt, Method(typeof(StringBuilder), nameof(StringBuilder.Append), new Type[]{ typeof(string)})),
                new CodeInstruction(OpCodes.Pop),
                new CodeInstruction(OpCodes.Ldloc_S, 33),
                new CodeInstruction(OpCodes.Ldstr, "</color>"),
                new CodeInstruction(OpCodes.Callvirt, Method(typeof(StringBuilder), nameof(StringBuilder.Append), new Type[]{ typeof(string)})),
                new CodeInstruction(OpCodes.Pop),
                new CodeInstruction(OpCodes.Ldloc_S, 33).WithLabels(noteSkipLabel),
            });

            for (int z = 0; z < newInstructions.Count; z++)
            {
                //Log.Debug(newInstructions[z].ToString());
                yield return newInstructions[z];
            }

            ListPool<CodeInstruction>.Shared.Return(newInstructions);
        }
    }
}