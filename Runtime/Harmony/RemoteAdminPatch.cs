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
            int prefixIndex = newInstructions.FindIndex(inst => inst.opcode == OpCodes.Ldstr && inst.operand.ToString() == "[~] ") + prefixOffset;

            var skipStrLabel = generator.DefineLabel();

            Log.Debug($"prefixIndex: {prefixIndex}");

            //Prefix patch
            newInstructions.InsertRange(prefixIndex, new[]
            {
                 new CodeInstruction(OpCodes.Call, PropertyGetter(typeof(ServerRoles), nameof(ServerRoles.gameObject))),
                 new CodeInstruction(OpCodes.Call, Method(typeof(Extensions.Extensions), nameof(Extensions.Extensions.IsFakePlayer), new System.Type[] { typeof(GameObject) })),
                 new CodeInstruction(OpCodes.Brfalse_S, skipStrLabel),
                 new CodeInstruction(OpCodes.Ldstr, "[FAKE] "),
                 new CodeInstruction(OpCodes.Stloc_S, 120),
                 new CodeInstruction(OpCodes.Ldloc_S, 122).WithLabels(skipStrLabel),
            });

            int loopStartIndex = -1;

            for(int i = 0; i < newInstructions.Count - 5; i++)
            {
                if(newInstructions[i].opcode == OpCodes.Call && newInstructions[i + 1].opcode == OpCodes.Callvirt && newInstructions[i + 5].opcode == OpCodes.Ldsfld)
                {
                     loopStartIndex = i + 1;
                     break;
                }
            }

            if (loopStartIndex != -1)
            {
                Log.Debug($"loopStartIndex: {loopStartIndex}", Plugin.Instance.Config.VerboseOutput);

                const int endIndexOffset = 14;
                int endIndex = newInstructions.FindLastIndex(inst => inst.opcode == OpCodes.Ldstr && inst.operand.ToString() == ";") + endIndexOffset;

                if(endIndex != endIndexOffset - 1)
                {
                    Log.Debug($"endIndex: {endIndex}", Plugin.Instance.Config.VerboseOutput);

                    var goVar = generator.DeclareLocal(typeof(GameObject));
                    var skipLabel2 = generator.DefineLabel();
                    var loopRetLabel = generator.DefineLabel();

                    newInstructions[endIndex] = newInstructions[endIndex].WithLabels(loopRetLabel);

                    //Patch for hiding from RA
                    newInstructions.InsertRange(loopStartIndex, new[]
                    {
                        new CodeInstruction(OpCodes.Stloc_S, goVar),
                        new CodeInstruction(OpCodes.Ldloc_S, goVar),
                        new CodeInstruction(OpCodes.Call, Method(typeof(Extensions.Extensions), nameof(Extensions.Extensions.IsFakePlayer), new System.Type[] { typeof(GameObject) })),
                        new CodeInstruction(OpCodes.Brfalse_S, skipLabel2),
                        new CodeInstruction(OpCodes.Ldloc_S, goVar),
                        new CodeInstruction(OpCodes.Call, Method(typeof(Extensions.Extensions), nameof(Extensions.Extensions.AsFakePlayer), new System.Type[] { typeof(GameObject) })),
                        new CodeInstruction(OpCodes.Callvirt, PropertyGetter(typeof(API.FakePlayer), nameof(API.FakePlayer.DisplayInRA))),
                        new CodeInstruction(OpCodes.Brfalse_S, loopRetLabel),
                        new CodeInstruction(OpCodes.Ldloc_S, goVar).WithLabels(skipLabel2),
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

            const int query_offset = -3;
            const int retryLabelOffset = -7;
            
            int shortQueryIndex = newInstructions.FindLastIndex(inst => inst.opcode == OpCodes.Ldstr && inst.operand.ToString() == ":PLAYER#Player with id ") + query_offset;
            
            var shortQueryRetryLabel = generator.DefineLabel();
            newInstructions[shortQueryIndex + retryLabelOffset] = newInstructions[shortQueryIndex + retryLabelOffset].WithLabels(shortQueryRetryLabel);

            Log.Debug($"shortQueryIndex: {shortQueryIndex}");
            Log.Debug($"shortQueryRetryLabel: {shortQueryIndex + retryLabelOffset}");

            var tmpQueryStorage = generator.DeclareLocal(typeof(string[]));

            newInstructions.InsertRange(shortQueryIndex, new[]
            {
                new CodeInstruction(OpCodes.Stloc_S, tmpQueryStorage),
                new CodeInstruction(OpCodes.Ldloc_S, tmpQueryStorage),
                new CodeInstruction(OpCodes.Call, Method(typeof(RemoteAdminPatch), nameof(FindFakePlayer))),
                new CodeInstruction(OpCodes.Stloc_S, 140),
                new CodeInstruction(OpCodes.Ldloc_S, 140),
                new CodeInstruction(OpCodes.Brtrue_S, shortQueryRetryLabel),
                new CodeInstruction(OpCodes.Ldloc_S, tmpQueryStorage)
            });

            var raListRetryLabel = generator.DefineLabel();

            int raListIndex = newInstructions.FindIndex(inst => inst.opcode == OpCodes.Ldstr && inst.operand.ToString() == ":PLAYER#Player with id ") + query_offset;
            newInstructions[raListIndex + retryLabelOffset] = newInstructions[raListIndex + retryLabelOffset].WithLabels(raListRetryLabel);

            Log.Debug($"raListIndex: {raListIndex}");
            Log.Debug($"raListRetryLabel: {raListIndex + retryLabelOffset}");

            newInstructions.InsertRange(raListIndex, new[]
            {
                new CodeInstruction(OpCodes.Stloc_S, tmpQueryStorage),
                new CodeInstruction(OpCodes.Ldloc_S, tmpQueryStorage),
                new CodeInstruction(OpCodes.Call, Method(typeof(RemoteAdminPatch), nameof(FindFakePlayer))),
                new CodeInstruction(OpCodes.Stloc_S, 124),
                new CodeInstruction(OpCodes.Ldloc_S, 124),
                new CodeInstruction(OpCodes.Brtrue_S, raListRetryLabel),
                new CodeInstruction(OpCodes.Ldloc_S, tmpQueryStorage)
            });

            // Fake player note patch
            int ldstrColorIndex = newInstructions.FindIndex(inst => inst.opcode == OpCodes.Ldstr && inst.operand.ToString() == "</color>");
            Log.Debug($"ldstrColorIndex: {ldstrColorIndex}");

            var noteSkipLabel = generator.DefineLabel();

            newInstructions.InsertRange(ldstrColorIndex, new[]
            {
                new CodeInstruction(OpCodes.Ldloc_S, 124),
                new CodeInstruction(OpCodes.Call, Method(typeof(Extensions.Extensions), nameof(Extensions.Extensions.IsFakePlayer), new Type[]{ typeof(GameObject) })),
                new CodeInstruction(OpCodes.Brfalse_S, noteSkipLabel),
                new CodeInstruction(OpCodes.Ldstr, "\n<color=#FEAF04>This is fake player instance created with FakePlayerAPI</color>"),
                new CodeInstruction(OpCodes.Callvirt, Method(typeof(StringBuilder), nameof(StringBuilder.Append), new Type[]{ typeof(string)})),
                new CodeInstruction(OpCodes.Pop),
                new CodeInstruction(OpCodes.Ldloc_S, 135),
                new CodeInstruction(OpCodes.Ldstr, "\n<color=#FEAF04>Debug identifier: "),
                new CodeInstruction(OpCodes.Callvirt, Method(typeof(StringBuilder), nameof(StringBuilder.Append), new Type[]{ typeof(string)})),
                new CodeInstruction(OpCodes.Pop),
                new CodeInstruction(OpCodes.Ldloc_S, 135),
                new CodeInstruction(OpCodes.Ldloc_S, 124),
                new CodeInstruction(OpCodes.Call, Method(typeof(Extensions.Extensions), nameof(Extensions.Extensions.AsFakePlayer), new Type[]{ typeof(GameObject) })),
                new CodeInstruction(OpCodes.Callvirt, Method(typeof(API.FakePlayer), nameof(API.FakePlayer.GetIdentifier))),
                new CodeInstruction(OpCodes.Callvirt, Method(typeof(StringBuilder), nameof(StringBuilder.Append), new Type[]{ typeof(string)})),
                new CodeInstruction(OpCodes.Pop),
                new CodeInstruction(OpCodes.Ldloc_S, 135),
                new CodeInstruction(OpCodes.Ldstr, "</color>"),
                new CodeInstruction(OpCodes.Callvirt, Method(typeof(StringBuilder), nameof(StringBuilder.Append), new Type[]{ typeof(string)})),
                new CodeInstruction(OpCodes.Pop),
                new CodeInstruction(OpCodes.Ldloc_S, 135).WithLabels(noteSkipLabel),
            });

            for (int z = 0; z < newInstructions.Count; z++)
                yield return newInstructions[z];

            ListPool<CodeInstruction>.Shared.Return(newInstructions);
        }
    }
}