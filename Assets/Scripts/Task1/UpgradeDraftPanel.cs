using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace EvoTowers.Task1
{
    public class UpgradeDraftPanel : MonoBehaviour
    {
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Button optionButtonA;
        [SerializeField] private Button optionButtonB;
        [SerializeField] private Button optionButtonC;
        [SerializeField] private Text optionTitleA;
        [SerializeField] private Text optionTitleB;
        [SerializeField] private Text optionTitleC;
        [SerializeField] private Text optionDescA;
        [SerializeField] private Text optionDescB;
        [SerializeField] private Text optionDescC;
        [SerializeField] private Text feedbackText;

        private readonly List<UpgradeDefinition> currentOptions = new List<UpgradeDefinition>();
        private bool hasSelection;

        private void Awake()
        {
            BindButton(optionButtonA, 0);
            BindButton(optionButtonB, 1);
            BindButton(optionButtonC, 2);
        }

        private void OnEnable()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.UpgradeDraftReady += ShowDraft;
                GameManager.Instance.UpgradeApplied += HandleUpgradeApplied;
            }
        }

        private void OnDisable()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.UpgradeDraftReady -= ShowDraft;
                GameManager.Instance.UpgradeApplied -= HandleUpgradeApplied;
            }
        }

        private void BindButton(Button button, int index)
        {
            if (button == null)
            {
                return;
            }

            button.onClick.AddListener(() => Select(index));
        }

        private void ShowDraft(UpgradeDraftResult result)
        {
            currentOptions.Clear();
            hasSelection = false;

            if (!result.IsValid || result.Options == null || result.Options.Count == 0)
            {
                if (panelRoot != null)
                {
                    panelRoot.SetActive(false);
                }

                return;
            }

            currentOptions.AddRange(result.Options);
            ApplyOption(0, optionTitleA, optionDescA, optionButtonA);
            ApplyOption(1, optionTitleB, optionDescB, optionButtonB);
            ApplyOption(2, optionTitleC, optionDescC, optionButtonC);

            if (panelRoot != null)
            {
                panelRoot.SetActive(true);
            }
        }

        private void Select(int index)
        {
            if (hasSelection || index < 0 || index >= currentOptions.Count)
            {
                return;
            }

            hasSelection = true;
            GameManager.Instance?.ApplyUpgrade(currentOptions[index]);
        }

        private void HandleUpgradeApplied(UpgradeDefinition definition)
        {
            if (panelRoot != null)
            {
                panelRoot.SetActive(false);
            }

            if (feedbackText != null && definition != null)
            {
                feedbackText.text = $"{definition.displayName} Applied";
            }
        }

        private void ApplyOption(int index, Text titleText, Text descText, Button button)
        {
            bool hasOption = index < currentOptions.Count;
            if (button != null)
            {
                button.gameObject.SetActive(hasOption);
            }

            if (!hasOption)
            {
                return;
            }

            UpgradeDefinition option = currentOptions[index];
            if (titleText != null)
            {
                titleText.text = option.displayName;
            }

            if (descText != null)
            {
                descText.text = option.description;
            }
        }
    }
}
