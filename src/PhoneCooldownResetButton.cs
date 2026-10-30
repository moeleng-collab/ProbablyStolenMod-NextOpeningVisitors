using System;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NextOpeningVisitors
{
    [HarmonyPatch(typeof(PhoneUIManager), nameof(PhoneUIManager.OpenUI))]
    internal static class PhoneCooldownResetButton
    {
        private const string ButtonObjectName = "NextOpeningVisitors.ResetCooldownButton";

        private static void Postfix(PhoneUIManager __instance)
        {
            try
            {
                if (__instance.phoneBookUIManager == null || __instance.numberDisplay == null)
                {
                    NextOpeningVisitorsMod.Log?.Warning("Phone cooldown button unavailable: phone UI references are missing.");
                    return;
                }

                if (!__instance.phoneBookUIManager.gameObject.activeSelf) __instance.phoneBookUIManager.OpenUI();
                AddButton(__instance, __instance.phoneBookUIManager);
            }
            catch (Exception exception)
            {
                NextOpeningVisitorsMod.Log?.Warning("Could not create the phone cooldown button. " + exception);
            }
        }

        private static void AddButton(PhoneUIManager phone, PhoneBookUIManager book)
        {
            RectTransform displayRect = phone.numberDisplay.rectTransform;
            Transform? parent = displayRect.parent;
            if (parent == null) throw new InvalidOperationException("The number display has no parent transform.");

            Transform? oldButton = parent.Find(ButtonObjectName);
            if (oldButton != null) UnityEngine.Object.Destroy(oldButton.gameObject);

            GameObject instance = new GameObject(ButtonObjectName, typeof(RectTransform));
            instance.transform.SetParent(parent, false);
            RectTransform? rect = instance.GetComponent<RectTransform>();
            TextMeshProUGUI? label = instance.AddComponent<TextMeshProUGUI>();
            if (rect == null || label == null)
            {
                UnityEngine.Object.Destroy(instance);
                throw new InvalidOperationException("Could not create the cooldown button text components.");
            }

            rect.anchorMin = displayRect.anchorMin;
            rect.anchorMax = displayRect.anchorMax;
            rect.pivot = displayRect.pivot;
            rect.anchoredPosition = displayRect.anchoredPosition;
            rect.sizeDelta = displayRect.sizeDelta;
            rect.localRotation = displayRect.localRotation;
            rect.localScale = displayRect.localScale;
            instance.transform.SetAsLastSibling();

            instance.SetActive(true);
            label.enabled = true;
            if (phone.statusDisplay != null && phone.statusDisplay.font != null)
            {
                label.font = phone.statusDisplay.font;
                label.fontSharedMaterial = phone.statusDisplay.fontSharedMaterial;
            }
            label.text = ModLocalization.ResetButton(LocHelper.GetCurrentLocaleCode());
            label.alignment = TextAlignmentOptions.Center;
            label.enableAutoSizing = false;
            label.fontSize = Mathf.Max(1f, (phone.numberDisplay.fontSize - 2f) * 0.65f);
            Color labelColor = phone.numberDisplay.color;
            labelColor.a = 1f;
            label.color = labelColor;
            label.raycastTarget = true;
            label.ForceMeshUpdate();

            Button button = instance.AddComponent<Button>();
            button.targetGraphic = label;
            button.transition = Selectable.Transition.None;
            Navigation navigation = button.navigation;
            navigation.mode = Navigation.Mode.None;
            button.navigation = navigation;
            button.onClick.AddListener(() => ResetCooldowns(phone, book));

            phone.gameObject.AddComponent<PhoneCooldownResetButtonVisibility>().Configure(instance, phone.numberDisplay);
            NextOpeningVisitorsMod.Log?.Log($"Cooldown button UI: active={instance.activeInHierarchy}, text='{label.text}', chars={label.textInfo.characterCount}, font={label.font?.name}, color={label.color}, rect={rect.rect}.");
        }

        private static void ResetCooldowns(PhoneUIManager phone, PhoneBookUIManager book)
        {
            try
            {
                CooldownResetResult result = CooldownResetRules.ResetCooldowns(
                    PlayerStore.Instance.PhoneClientDict,
                    PlayerStore.Instance.futurStoreClientIdQueue,
                    client => client.phoneState == StorePhoneClient.PhoneState.Cooldown,
                    client =>
                    {
                        client.currentCooldown = 0;
                        client.phoneState = StorePhoneClient.PhoneState.Regular;
                    });

                ShowDialogMessage(ModLocalization.ResetStatus(
                    LocHelper.GetCurrentLocaleCode(), result.ResetCount, result.SkippedCount));
                NextOpeningVisitorsMod.Log?.Log($"Phone cooldown reset: reset={result.ResetCount}, pendingSkipped={result.SkippedCount}.");
                book.OpenUI();
            }
            catch (Exception exception)
            {
                NextOpeningVisitorsMod.Log?.Error("Phone cooldown reset failed. " + exception);
                ShowDialogMessage(ModLocalization.ResetFailure(LocHelper.GetCurrentLocaleCode()));
            }
        }

        private static void ShowDialogMessage(string message)
        {
            DialoguePhoneUIManager? dialogue = DialoguePhoneUIManager.Instance;
            if (dialogue == null)
            {
                NextOpeningVisitorsMod.Log?.Warning("Could not show the cooldown result in the phone dialogue UI.");
                return;
            }

            dialogue.StartDialogue(new Dialogue().SetText("NextOpeningVisitors", message));
        }
    }

    internal sealed class PhoneCooldownResetButtonVisibility : MonoBehaviour
    {
        private GameObject? button;
        private TextMeshProUGUI? numberDisplay;

        internal void Configure(GameObject target, TextMeshProUGUI display)
        {
            button = target;
            numberDisplay = display;
        }

        private void LateUpdate()
        {
            if (button == null || numberDisplay == null)
            {
                Destroy(this);
                return;
            }

            DialoguePhoneUIManager? dialogue = DialoguePhoneUIManager.Instance;
            Dialogue? currentDialog = dialogue == null ? null : dialogue.currentDialog;
            bool isConversationActive = currentDialog != null && currentDialog.title != "NextOpeningVisitors";
            bool shouldShow = string.IsNullOrEmpty(numberDisplay.text) && !isConversationActive;
            if (button.activeSelf != shouldShow) button.SetActive(shouldShow);
        }
    }
}
