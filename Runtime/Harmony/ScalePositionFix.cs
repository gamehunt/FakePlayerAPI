﻿using HarmonyLib;
using UnityEngine;

namespace FakePlayer.Runtime.Harmony
{
    [HarmonyPatch(typeof(PlayerMovementSync), nameof(PlayerMovementSync.OverridePosition))]
    internal class ScalePositionFix
    {
        private static bool Prefix(PlayerMovementSync __instance, Vector3 pos, float rot, bool forceGround)
        {
            if (!API.FakePlayer.Dictionary.ContainsKey(__instance.gameObject))
            {
                return true;
            }
            RaycastHit raycastHit;
            if (forceGround && Physics.Raycast(pos, Vector3.down, out raycastHit, 100f, __instance.CollidableSurfaces))
            {
                pos = raycastHit.point + Vector3.up * Plugin.Instance.Config.FakePlayerSizePositionMultiplier;
                pos = new Vector3(pos.x, pos.y - (1f - __instance._hub.transform.localScale.y) * Plugin.Instance.Config.FakePlayerSizePositionMultiplier, pos.z);
            }
            __instance.ForcePosition(pos);
            __instance.PlayScp173SoundIfTeleported();
            return false;
        }
    }
}