using Exiled.API.Features;
using HarmonyLib;
using NorthwoodLib.Pools;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;

namespace FakePlayer.Runtime.Harmony
{
    [HarmonyPatch(typeof(NineTailedFoxAnnouncer), nameof(NineTailedFoxAnnouncer.Update))]
    public class Scp079RecontaimentPatch
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var newInstructions = ListPool<CodeInstruction>.Shared.Rent(instructions);

            int loopStartIndex1 = newInstructions.FindIndex(op => op.opcode == OpCodes.Ldstr && op.operand.ToString().Equals("LOST IN DECONTAMINATION SEQUENCE"));

            if (loopStartIndex1 != -1)
            {
                const int loopStartIndexOffset1 = 14;

                var skipLabel1 = generator.DefineLabel();
                var skipLabel2 = generator.DefineLabel();

                Log.Debug($"loopStartIndex1: {loopStartIndex1 + loopStartIndexOffset1}", Plugin.Instance.Config.VerboseOutput);

                int skipLabel2Index = newInstructions.FindIndex(op => op.opcode == OpCodes.Constrained);

                if (skipLabel2Index != -1)
                {
                    const int skipLabel2Offset = -5;
                    Log.Debug($"skipLabel2Index: {skipLabel2Index + skipLabel2Offset}", Plugin.Instance.Config.VerboseOutput);
                    newInstructions[skipLabel2Index + skipLabel2Offset] = newInstructions[skipLabel2Index + skipLabel2Offset].WithLabels(skipLabel2);
                    newInstructions.InsertRange(loopStartIndex1 + loopStartIndexOffset1, new[]
                    {
                            new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Extensions.Extensions), nameof(Extensions.Extensions.IsFakePlayer), new System.Type[]{ typeof(GameObject) })),
                            new CodeInstruction(OpCodes.Brfalse_S, skipLabel1),
                            new CodeInstruction(OpCodes.Ldloc_S, 14),
                            new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Extensions.Extensions), nameof(Extensions.Extensions.AsFakePlayer), new System.Type[]{ typeof(GameObject) })),
                            new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(API.FakePlayer), nameof(API.FakePlayer.AffectEndConditions))),
                            new CodeInstruction(OpCodes.Brtrue_S, skipLabel2),
                            new CodeInstruction(OpCodes.Ldloc_S, 14).WithLabels(skipLabel1),
                    });

                    int loopStartIndex2 = newInstructions.FindIndex(op => op.opcode == OpCodes.Ldstr && op.operand.ToString().Equals("SUCCESSFULLY TERMINATED . TERMINATION CAUSE UNSPECIFIED"));

                    if(loopStartIndex2 != -1)
                    {
                        const int loopStartIndexOffset2 = 13;
                        Log.Debug($"loopStartIndex2: {loopStartIndex2 + loopStartIndexOffset2}", Plugin.Instance.Config.VerboseOutput);

                        int skipLabel4Index = newInstructions.FindLastIndex(op => op.opcode == OpCodes.Constrained);

                        if (skipLabel4Index != -1)
                        {
                            const int skipLabel4Offset = -5;

                            var skipLabel3 = generator.DefineLabel();
                            var skipLabel4 = generator.DefineLabel();

                            newInstructions[skipLabel4Index + skipLabel4Offset] = newInstructions[skipLabel4Index + skipLabel4Offset].WithLabels(skipLabel4);
                            newInstructions.InsertRange(loopStartIndex2 + loopStartIndexOffset2, new[]
                            {
                                new CodeInstruction(OpCodes.Stloc_S, 14),
                                new CodeInstruction(OpCodes.Ldloc_S, 14),
                                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Extensions.Extensions), nameof(Extensions.Extensions.IsFakePlayer), new System.Type[]{ typeof(GameObject) })),
                                new CodeInstruction(OpCodes.Brfalse_S, skipLabel3),
                                new CodeInstruction(OpCodes.Ldloc_S, 14),
                                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Extensions.Extensions), nameof(Extensions.Extensions.AsFakePlayer), new System.Type[]{ typeof(GameObject) })),
                                new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(API.FakePlayer), nameof(API.FakePlayer.AffectEndConditions))),
                                new CodeInstruction(OpCodes.Brtrue_S, skipLabel4),
                                new CodeInstruction(OpCodes.Ldloc_S, 14).WithLabels(skipLabel3),
                            });
                        }
                        else
                        {
                            Log.Error("skipLabel4Index not found, go bonk gamehunt in discord!");
                        }
                    }
                    else
                    {
                        Log.Error("Failed to find loopStartIndex2, go bonk gamehunt in discord!");
                    }
                }
                else
                {
                    Log.Error("skipLabel2Index not found, go bonk gamehunt in discord!");
                }
            }
            else
            {
                Log.Error("Failed to find loopStartIndex1, go bonk gamehunt in discord!");
            }

            for (int z = 0; z < newInstructions.Count; z++)
                yield return newInstructions[z];

            ListPool<CodeInstruction>.Shared.Return(newInstructions);
        }
    }
}