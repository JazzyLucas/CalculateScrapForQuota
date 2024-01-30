using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Threading.Tasks;
using BepInEx;
using HarmonyLib;
using GameNetcodeStuff;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using CalculateScrapForQuota.Scripts;
using CalculateScrapForQuota.Utils;
using Unity.Netcode;
using Object = UnityEngine.Object;
using P = CalculateScrapForQuota.Plugin;
using DU = CalculateScrapForQuota.Utils.DebugUtil;
// ReSharper disable PossibleNullReferenceException

namespace CalculateScrapForQuota.Patches
{
    [HarmonyPatch]
    [SuppressMessage("Method Declaration", "Harmony003:Harmony non-ref patch parameters modified")]
    public class GrabbableObjectPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(GrabbableObject), "EnableItemMeshes")]
        private static void EnableItemMeshes(GrabbableObject __instance, bool enable)
        {
            P.Log("EnableItemMeshes() called.");

            var grabbableGO = __instance.gameObject;
            if (HudManagerPatch.CurrentHighlight.Contains(grabbableGO) && enable)
            {
                List<GameObject> gos = new();
                gos.Add(grabbableGO);
                MaterialSwapper.SwapOn(gos);
            }
        }
    }
}