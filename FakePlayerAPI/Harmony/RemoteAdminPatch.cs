using Exiled.API.Features;
using HarmonyLib;
using NorthwoodLib.Pools;
using RemoteAdmin;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using UnityEngine;
using static HarmonyLib.AccessTools;

namespace FakePlayerAPI.Harmony
{
    [HarmonyPatch(typeof(CommandProcessor), nameof(CommandProcessor.ProcessQuery))]
    internal class RemoteAdminPatch
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var newInstructions = ListPool<CodeInstruction>.Shared.Rent(instructions);


            const int prefixOffset = 3;
            int prefixIndex = newInstructions.FindIndex(inst => inst.opcode == OpCodes.Ldstr && inst.operand.ToString() == "[~] ") + prefixOffset;

            var skipStrLabel = generator.DefineLabel();

            Log.Debug($"prefixIndex: {prefixIndex}");

            newInstructions.InsertRange(prefixIndex, new[]
            {
                 new CodeInstruction(OpCodes.Call, PropertyGetter(typeof(ServerRoles), nameof(ServerRoles.gameObject))),
                 new CodeInstruction(OpCodes.Call, Method(typeof(Extensions), nameof(Extensions.IsFakePlayer), new System.Type[] { typeof(GameObject) })),
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

            Log.Debug($"loopStartIndex: {loopStartIndex}", Plugin.Instance.Config.VerboseOutput);

            if (loopStartIndex != -1)
            {
                const int endIndexOffset = 14;
                int endIndex = newInstructions.FindLastIndex(inst => inst.opcode == OpCodes.Ldstr && inst.operand.ToString() == ";") + endIndexOffset;
                Log.Debug($"endIndex: {endIndex}", Plugin.Instance.Config.VerboseOutput);

                if(endIndex != endIndexOffset - 1)
                {
                    var goVar = generator.DeclareLocal(typeof(GameObject));
                    var skipLabel2 = generator.DefineLabel();
                    var loopRetLabel = generator.DefineLabel();

                    newInstructions[endIndex] = newInstructions[endIndex].WithLabels(loopRetLabel);

                    newInstructions.InsertRange(loopStartIndex, new[]
                    {
                        new CodeInstruction(OpCodes.Stloc_S, goVar),
                        new CodeInstruction(OpCodes.Ldloc_S, goVar),
                        new CodeInstruction(OpCodes.Call, Method(typeof(Extensions), nameof(Extensions.IsFakePlayer), new System.Type[] { typeof(GameObject) })),
                        new CodeInstruction(OpCodes.Brfalse_S, skipLabel2),
                        new CodeInstruction(OpCodes.Ldloc_S, goVar),
                        new CodeInstruction(OpCodes.Call, Method(typeof(Extensions), nameof(Extensions.AsFakePlayer), new System.Type[] { typeof(GameObject) })),
                        new CodeInstruction(OpCodes.Callvirt, PropertyGetter(typeof(FakePlayer), nameof(FakePlayer.DisplayInRA))),
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

            for (int z = 0; z < newInstructions.Count; z++)
                yield return newInstructions[z];

            ListPool<CodeInstruction>.Shared.Return(newInstructions);
        }
    }
}