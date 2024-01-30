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
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using CalculateScrapForQuota.Scripts;
using CalculateScrapForQuota.Utils;
using Object = UnityEngine.Object;
using P = CalculateScrapForQuota.Plugin;
using DU = CalculateScrapForQuota.Utils.DebugUtil;
// ReSharper disable PossibleNullReferenceException

namespace CalculateScrapForQuota.Patches
{
    [HarmonyPatch]
    [SuppressMessage("Method Declaration", "Harmony003:Harmony non-ref patch parameters modified")]
    public class HudManagerPatch
    {
        private static GameObject shipGO => GameObject.Find("/Environment/HangarShip");
        private static GameObject valueCounterGO => GameObject.Find("/Systems/UI/Canvas/IngamePlayerHUD/BottomMiddle/ValueCounter");
        private static int unmetQuota => TimeOfDay.Instance.profitQuota - TimeOfDay.Instance.quotaFulfilled;
        private static bool isAtCompany => StartOfRound.Instance.currentLevel.levelID == 3;
        private static bool isInShip => GameNetworkManager.Instance.localPlayerController.isInHangarShipRoom;
        private static bool isScanning(HUDManager instance) => (float)typeof(HUDManager)
            .GetField("playerPingingScan", BindingFlags.NonPublic | BindingFlags.Instance)
            .GetValue(instance) > -1.0;
        private static bool canPlayerScan(HUDManager instance) => (bool)typeof(HUDManager)
            .GetMethod("CanPlayerScan", BindingFlags.NonPublic | BindingFlags.Instance)
            .Invoke(instance, null);

        public static List<GameObject> CurrentHighlight = new();
        
        [HarmonyPrefix]
        [HarmonyPatch(typeof(HUDManager), "PingScan_performed")]
        private static void OnScan(HUDManager __instance, InputAction.CallbackContext context)
        {
            P.Log("OnScan() called.");
            
            // Guard Clause
            if (!context.performed
                || isScanning(__instance)
                || !canPlayerScan(__instance)
                || GameNetworkManager.Instance.localPlayerController == null
                ) return;
            
            P.Log("OnScan() is valid.");

            List<GrabbableObject> sellableGrabbables;
            if (isInShip && !isAtCompany)
                sellableGrabbables = GetSellableGrabbablesInChildren(shipGO);
            else if (isAtCompany)
                sellableGrabbables = GetAllSellableObjects();
            else
                return;
            
            var optimalGrabbables = MathUtil.FindBestCombination(sellableGrabbables, unmetQuota, grabbable => grabbable.scrapValue);

            if (optimalGrabbables.totalValue >= unmetQuota)
            {
                SetupText(optimalGrabbables.totalValue);
                CurrentHighlight = optimalGrabbables.combination.Select(g => g.gameObject).ToList();
                Display();
            }
        }
        
        private static Coroutine displayCoroutine = null;
        private static void Display(float duration = 5f)
        {
            if (displayCoroutine != null)
                GameNetworkManager.Instance.StopCoroutine(displayCoroutine);
            MaterialSwapper.SwapOn(CurrentHighlight);
            displayCoroutine = GameNetworkManager.Instance.StartCoroutine(DisplayRoutine(duration));
        }
        private static IEnumerator DisplayRoutine(float duration)
        {
            _textGO.SetActive(true);

            yield return new WaitForSeconds(duration);

            _textGO.SetActive(false);
            MaterialSwapper.SwapOff();
            //MaterialSwapper.Clear();
        }

        private static List<GrabbableObject> GetAllSellableObjects()
        {
            var grabbables = GameObject.FindObjectsOfType<GrabbableObject>();
            var sellableGrabbables = grabbables.Where(IsGrabbableSellable).ToList();
            return sellableGrabbables;
            
        }
        
        private static List<GrabbableObject> GetSellableGrabbablesInChildren(GameObject GO)
        {
            var grabbables = GO.GetComponentsInChildren<GrabbableObject>();
            var sellableGrabbables = grabbables.Where(IsGrabbableSellable).ToList();
            return sellableGrabbables;
        }
        
        private static bool IsGrabbableSellable(GrabbableObject grabbable)
        {
            return grabbable.itemProperties.isScrap
                   && !grabbable.isPocketed
                   && grabbable.scrapValue > 0
                   && grabbable.name != "ClipboardManual"
                   && grabbable.name != "StickyNoteItem"
                   && grabbable.name != "Gift"
                   && grabbable.name != "Shotgun"
                   && grabbable.name != "Ammo";
        }
        
        private static GameObject _textGO;
        private static TextMeshProUGUI _textMesh => _textGO.GetComponentInChildren<TextMeshProUGUI>();
        
        private static void SetupText(int totalValue)
        {
            // Text GameObject instantiation and caching
            if (!_textGO)
            {
                _textGO = Object.Instantiate(valueCounterGO.gameObject, valueCounterGO.transform.parent, false);
                _textGO.transform.Translate(0f, 1f, 0f);
                var pos = _textGO.transform.localPosition;
                _textGO.transform.localPosition = new(pos.x + 50f, -100f, pos.z);
                _textMesh.fontSize = 12;
            }
            
            _textMesh.text = $"Optimal: {totalValue}";
        }
    }
}
