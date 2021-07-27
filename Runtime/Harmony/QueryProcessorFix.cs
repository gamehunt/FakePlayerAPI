using Exiled.API.Features;
using HarmonyLib;
using Mirror;
using NorthwoodLib.Pools;
using RemoteAdmin;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace FakePlayers.Runtime.Harmony
{
    [HarmonyPatch(typeof(QueryProcessor), nameof(QueryProcessor.Start))]
    internal class QueryProcessorFix
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var newInstructions = ListPool<CodeInstruction>.Shared.Rent(instructions);

            int ipAddressIndex = newInstructions.FindIndex(inst => inst.opcode == OpCodes.Stfld && (FieldInfo)inst.operand == AccessTools.Field(typeof(QueryProcessor), nameof(QueryProcessor._ipAddress)));

            var skipLabel = generator.DefineLabel();

            if(ipAddressIndex != -1)
            {
                Log.Debug($"ipAddressIndex: {ipAddressIndex}");

                const int offset = -1;

                newInstructions[ipAddressIndex] = newInstructions[ipAddressIndex].WithLabels(skipLabel);

                newInstructions.InsertRange(ipAddressIndex + offset, new[] {
                    new CodeInstruction(OpCodes.Dup),
                    new CodeInstruction(OpCodes.Ldnull),
                    new CodeInstruction(OpCodes.Ceq),
                    new CodeInstruction(OpCodes.Brtrue_S, skipLabel),
                });
            }
            else
            {
                Log.Error("Failed to find ipAddressIndex, go bonk gamehunt in discord");
            }

            for (int z = 0; z < newInstructions.Count; z++)
                yield return newInstructions[z];

            ListPool<CodeInstruction>.Shared.Return(newInstructions);
        }
    }
}