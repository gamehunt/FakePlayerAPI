using Exiled.API.Features;
using HarmonyLib;
using Mirror;
using NorthwoodLib.Pools;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;

namespace FakePlayerAPI.Harmony
{
    [HarmonyPatch(typeof(NetworkBehaviour), nameof(NetworkBehaviour.SendTargetRPCInternal))]
    internal class MirrorPatch
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var newInstructions = ListPool<CodeInstruction>.Shared.Rent(instructions);

            const int ldarg1Offset = 2;
            int ldarg1Index = newInstructions.FindIndex(op => op.opcode == OpCodes.Ldarg_0) + ldarg1Offset;

            Log.Debug($"ldarg1Index: {ldarg1Index}");

            var skipLabel = generator.DefineLabel();

            newInstructions[ldarg1Index] = newInstructions[ldarg1Index].WithLabels(skipLabel);

            newInstructions.InsertRange(ldarg1Index, new[]
            {
                new CodeInstruction(OpCodes.Dup),
                new CodeInstruction(OpCodes.Brtrue_S, skipLabel),
                new CodeInstruction(OpCodes.Pop),
                new CodeInstruction(OpCodes.Ret)
            });

            for (int z = 0; z < newInstructions.Count; z++)
                yield return newInstructions[z];

            ListPool<CodeInstruction>.Shared.Return(newInstructions);
        }
    }
}