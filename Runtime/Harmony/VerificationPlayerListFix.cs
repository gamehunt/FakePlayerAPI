using Exiled.API.Features;
using HarmonyLib;
using Mirror.LiteNetLib4Mirror;
using NorthwoodLib;
using NorthwoodLib.Pools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using UnityEngine;

namespace FakePlayer.Runtime.Harmony
{
    [HarmonyPatch(typeof(ServerConsole), nameof(ServerConsole.FixedUpdate))]
    internal class VerificationPlayerListFix
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var newInstructions = ListPool<CodeInstruction>.Shared.Rent(instructions);

            int ccmGetOffset = newInstructions.FindIndex(inst => inst.opcode == OpCodes.Callvirt && (MethodInfo)inst.operand == AccessTools.Method(typeof(GameObject), nameof(GameObject.GetComponent), null, new Type[] { typeof(CharacterClassManager) }));

            if(ccmGetOffset != -1)
            {
                Log.Debug($"ccmGetOffset: {ccmGetOffset}", Plugin.Instance.Config.VerboseOutput);

                const int continueLabelOffset = -5;
                int continueLabelIndex = newInstructions.FindLastIndex(inst => inst.opcode == OpCodes.Constrained) + continueLabelOffset;

                if (continueLabelIndex - continueLabelOffset != -1)
                {
                    Log.Debug($"continueLabelIndex: {continueLabelIndex}", Plugin.Instance.Config.VerboseOutput);

                    var continueLabel = newInstructions[continueLabelIndex].labels.FirstOrDefault();

                    newInstructions.InsertRange(ccmGetOffset, new[]
                    {
                        new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(FakePlayer.Extensions.Extensions), nameof(FakePlayer.Extensions.Extensions.IsFakePlayer), new Type[] { typeof(GameObject) })),
                        new CodeInstruction(OpCodes.Brtrue_S, continueLabel),
                        new CodeInstruction(OpCodes.Ldloc_3, continueLabel),
                    });
                }
                else
                {
                    Log.Error("Failed to find continueLabelIndex, go bonk gamehunt in discord!");
                }
            }
            else
            {
                Log.Error("Failed to find ccmGetOffset, go bonk gamehunt in discord!");
            }

            for (int z = 0; z < newInstructions.Count; z++)
                yield return newInstructions[z];

            ListPool<CodeInstruction>.Shared.Return(newInstructions);
        }
    }
}