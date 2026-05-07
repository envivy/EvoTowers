using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace EvoTowers.Task1
{
    public class CommanderSelectionPanel : MonoBehaviour
    {
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Button flameWardenButton;
        [SerializeField] private Button rangerCaptainButton;
        [SerializeField] private Text flameWardenNameText;
        [SerializeField] private Text flameWardenDescText;
        [SerializeField] private Text rangerCaptainNameText;
        [SerializeField] private Text rangerCaptainDescText;

        private readonly Dictionary<CommanderId, CommanderDefinition> commanders =
            new Dictionary<CommanderId, CommanderDefinition>();

        private void Awake()
        {
            if (flameWardenButton != null)
            {
                flameWardenButton.onClick.AddListener(() => ChooseCommander(CommanderId.FlameWarden));
            }

            if (rangerCaptainButton != null)
            {
                rangerCaptainButton.onClick.AddListener(() => ChooseCommander(CommanderId.RangerCaptain));
            }
        }

        private void Start()
        {
            if (GameManager.Instance == null)
            {
                return;
            }

            foreach (CommanderDefinition commander in GameManager.Instance.GetCommanderDefinitions())
            {
                commanders[commander.id] = commander;
            }

            ApplyCommanderText(CommanderId.FlameWarden, flameWardenNameText, flameWardenDescText);
            ApplyCommanderText(CommanderId.RangerCaptain, rangerCaptainNameText, rangerCaptainDescText);

            if (panelRoot != null)
            {
                panelRoot.SetActive(GameManager.Instance.ActiveCommander == null);
            }
        }

        private void ChooseCommander(CommanderId commanderId)
        {
            GameManager.Instance?.SelectCommander(commanderId);
            if (panelRoot != null)
            {
                panelRoot.SetActive(false);
            }
        }

        private void ApplyCommanderText(CommanderId commanderId, Text nameText, Text descText)
        {
            if (!commanders.TryGetValue(commanderId, out CommanderDefinition commander))
            {
                return;
            }

            if (nameText != null)
            {
                nameText.text = commander.displayName;
            }

            if (descText != null)
            {
                descText.text = commander.description;
            }
        }
    }
}
