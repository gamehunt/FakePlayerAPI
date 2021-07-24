using Exiled.API.Features;
using HarmonyLib;
using Mirror.LiteNetLib4Mirror;
using NorthwoodLib;
using NorthwoodLib.Pools;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using UnityEngine;

namespace FakePlayer.Runtime.Harmony
{
    [HarmonyPatch(typeof(ServerConsole), nameof(ServerConsole.RefreshServerData))]
    internal class PlayerListCountFix
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var newInstructions = ListPool<CodeInstruction>.Shared.Rent(instructions);

            const int offset = 1;

            int playerCountOffset1 = offset + newInstructions.FindIndex(inst => inst.opcode == OpCodes.Ldsfld && (FieldInfo)inst.operand == AccessTools.Field(typeof(ServerConsole), nameof(ServerConsole.PlayersAmount)));

            if(playerCountOffset1 - offset != -1)
            {
                Log.Debug($"playerCountOffset1: {playerCountOffset1}");

                newInstructions.InsertRange(playerCountOffset1, new[] {
                    new CodeInstruction(OpCodes.Call    , AccessTools.PropertyGetter(typeof(API.FakePlayer), nameof(API.FakePlayer.Dictionary))),
                    new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(Dictionary<GameObject, API.FakePlayer>), nameof(Dictionary<GameObject, API.FakePlayer>.Keys))),
                    new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(Dictionary<GameObject, API.FakePlayer>.KeyCollection), nameof(Dictionary<GameObject, API.FakePlayer>.KeyCollection.Count))),
                    new CodeInstruction(OpCodes.Sub),
                });
            }
            else
            {
                Log.Error("Failed to find playerCountOffset1! Go bonk gamehunt in discord");
            }

            int playerCountOffset2 = offset + newInstructions.FindLastIndex(inst => inst.opcode == OpCodes.Ldsfld && (FieldInfo)inst.operand == AccessTools.Field(typeof(ServerConsole), nameof(ServerConsole.PlayersAmount)));

            if (playerCountOffset2 - offset != -1)
            {
                Log.Debug($"playerCountOffset2: {playerCountOffset2}");

                newInstructions.InsertRange(playerCountOffset2, new[] {
                    new CodeInstruction(OpCodes.Call    , AccessTools.PropertyGetter(typeof(API.FakePlayer), nameof(API.FakePlayer.Dictionary))),
                    new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(Dictionary<GameObject, API.FakePlayer>), nameof(Dictionary<GameObject, API.FakePlayer>.Keys))),
                    new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(Dictionary<GameObject, API.FakePlayer>.KeyCollection), nameof(Dictionary<GameObject, API.FakePlayer>.KeyCollection.Count))),
                    new CodeInstruction(OpCodes.Sub),
                });
            }
            else
            {
                Log.Error("Failed to find playerCountOffset2! Go bonk gamehunt in discord");
            }

            for (int z = 0; z < newInstructions.Count; z++)
                yield return newInstructions[z];

            ListPool<CodeInstruction>.Shared.Return(newInstructions);
        }
    }
}