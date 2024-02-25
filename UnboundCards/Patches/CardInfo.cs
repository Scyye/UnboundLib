using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using Unbound.Cards;
using Unbound.Cards.Utils;
using Unbound.Core;
using Unbound.Cards.Utils;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Unbound.Cards.Patches
{
    [Serializable]
    [HarmonyPatch(typeof(CardInfo), "Awake")]
    public class CardInfoPatch
    {
        static Exception Finalizer(Exception __exception)
        {
            return __exception is NullReferenceException ? null : __exception;
        }

        static void Finalizer(CardInfo __instance)
        {
            GameObject cardObject = __instance.gameObject;
            MenuCard menuCard = cardObject.GetComponent<MenuCard>();
            if (menuCard == null) return;

            GameObject cardFrontObject = FindObjectInChildren(cardObject, "Front");
            if (cardFrontObject == null) return;

            GameObject back = FindObjectInChildren(cardObject, "Back");
            Object.Destroy(back);

            GameObject damagable = FindObjectInChildren(cardObject, "Damagable");
            Object.Destroy(damagable);

            foreach (CardVisuals cardVisuals in cardObject.GetComponentsInChildren<CardVisuals>())
            {
                cardVisuals.firstValueToSet = true;
            }

            FindObjectInChildren(cardObject, "BlockFront")?.SetActive(false);

            var canvasGroups = cardObject.GetComponentsInChildren<CanvasGroup>();
            foreach (var canvasGroup in canvasGroups)
            {
                canvasGroup.alpha = 1;
            }

            // // Creates problems if it's not in the game scene and also is the main cause of lag
            GameObject uiParticleObject = FindObjectInChildren(cardFrontObject.gameObject, "UI_ParticleSystem");
            if (uiParticleObject != null)
            {
                Object.Destroy(uiParticleObject);
            }

            if (__instance.cardArt != null)
            {
                var artObject = FindObjectInChildren(cardFrontObject.gameObject, "Art");
                if (artObject != null)
                {
                    var cardAnimationHandler = cardObject.AddComponent<CardAnimationHandler>();
                    cardAnimationHandler.ToggleAnimation(false);
                }
            }

            var backgroundObj = FindObjectInChildren(cardFrontObject.gameObject, "Background");
            if (backgroundObj == null) return;

            backgroundObj.transform.localScale = new Vector3(1, 1, 1);
            var rectTransform = backgroundObj.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.sizeDelta = new Vector2(1500f, 1500f);

            var imageComponent = backgroundObj.gameObject.GetComponentInChildren<Image>(true);
            if (imageComponent != null)
            {
                imageComponent.preserveAspect = true;
                imageComponent.color = new Color(0.16f, 0.16f, 0.16f, 1f);
            }

            var maskComponent = backgroundObj.gameObject.GetComponentInChildren<Mask>(true);
            if (maskComponent != null)
            {
                maskComponent.showMaskGraphic = true;
            }

            RectTransform rect = cardObject.GetOrAddComponent<RectTransform>();
            rect.localScale = 8f * Vector3.one;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.pivot = new Vector2(0.5f, 0.5f);

            ToggleCardsMenuHandler.UpdateCardColumnAmountMenus();
        }

        private static IEnumerable<GameObject> FindObjectsInChildren(GameObject gameObject, string gameObjectName)
        {
            Transform[] children = gameObject.GetComponentsInChildren<Transform>(true);
            return (from item in children where item.name == gameObjectName select item.gameObject).ToList();
        }

        private static GameObject FindObjectInChildren(GameObject gameObject, string gameObjectName)
        {
            Transform[] children = gameObject.GetComponentsInChildren<Transform>(true);
            return (from item in children where item.name == gameObjectName select item.gameObject).FirstOrDefault();
        }
    }

    [Serializable]
    [HarmonyPatch(typeof(CardInfo))]
    internal class CardInfoFix
    {

        [HarmonyPatch("CardDescription", MethodType.Getter)]
        internal static void CardName(ref string __result, CardInfo __instance)
        {
            if (__result == __instance.name && (string) __instance.GetFieldValue("cardName") != "")
            {
                __result = (string) __instance.GetFieldValue("cardName");
            }
        }

        [HarmonyPatch("CardName", MethodType.Getter)]
        internal static void CardDescription(ref string __result, CardInfo __instance)
        {
            if (__result == __instance.name && (string) __instance.GetFieldValue("cardDestription") != "")
            {
                __result = (string) __instance.GetFieldValue("cardDestription");
            }
        }
    }
}