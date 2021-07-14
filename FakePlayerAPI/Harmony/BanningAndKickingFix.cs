using Exiled.Events.EventArgs;
using Exiled.Events.Handlers;
using GameCore;
using HarmonyLib;
using Mirror;
using System;
using UnityEngine;

namespace FakePlayerAPI.Harmony
{
    [HarmonyPatch(typeof(BanPlayer), nameof(BanPlayer.BanUser), new[] { typeof(GameObject), typeof(int), typeof(string), typeof(string), typeof(bool) })]
    internal class BanningAndKickingFix
    {
        private static bool Prefix(BanPlayer __instance, GameObject user, int duration, string reason, string issuer, bool isGlobalBan)
        {
            try
            {
                Exiled.API.Features.Player issuerPlayer = Exiled.API.Features.Player.Get(issuer) ?? Exiled.API.Features.Server.Host;

                if (FakePlayer.Dictionary.ContainsKey(user))
                {
                    issuerPlayer.RemoteAdminMessage("Dont ban fake players", false, Plugin.Instance.Name);
                    issuerPlayer.ClearBroadcasts();
                    issuerPlayer.Broadcast(2, "Dont ban fake players");
                    return false;
                }

                return true;
            }
            catch (Exception e)
            {
                Exiled.API.Features.Log.Error($"Error occured in BanningAndKickingFix: {e}\n{e.StackTrace}");

                return true;
            }
        }
    }
}