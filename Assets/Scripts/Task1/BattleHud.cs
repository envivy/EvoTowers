using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace EvoTowers.Task1
{
    public class BattleHud : MonoBehaviour
    {
        [SerializeField] private Text goldText;
        [SerializeField] private Text livesText;
        [SerializeField] private Text waveText;
        [SerializeField] private Text enemyCountText;
        [SerializeField] private Text selectedTowerText;
        [SerializeField] private Text commanderInfoText;
        [SerializeField] private Text resultText;
        [SerializeField] private Text feedbackText;
        [SerializeField] private Button startWaveButton;
        [SerializeField] private Button arrowTowerButton;
        [SerializeField] private Button flameTowerButton;
        [SerializeField] private Button magicTowerButton;
        [SerializeField] private GameObject resultPanel;
        [SerializeField] private Text resultTitleText;
        [SerializeField] private Text resultSummaryText;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button backButton;
        [SerializeField] private GameObject commanderPanel;
        [SerializeField] private Button flameWardenButton;
        [SerializeField] private Button rangerCaptainButton;
        [SerializeField] private Text flameWardenNameText;
        [SerializeField] private Text flameWardenDescText;
        [SerializeField] private Text rangerCaptainNameText;
        [SerializeField] private Text rangerCaptainDescText;
        [SerializeField] private GameObject upgradePanel;
        [SerializeField] private Button optionButtonA;
        [SerializeField] private Button optionButtonB;
        [SerializeField] private Button optionButtonC;
        [SerializeField] private Text optionTitleA;
        [SerializeField] private Text optionTitleB;
        [SerializeField] private Text optionTitleC;
        [SerializeField] private Text optionDescA;
        [SerializeField] private Text optionDescB;
        [SerializeField] private Text optionDescC;
        [SerializeField] private WaveManager waveManager;
        [SerializeField] private Week2BattleFlow week2BattleFlow;
        [SerializeField] private bool enableFallbackHud = true;

        private readonly StringBuilder builder = new StringBuilder(64);
        private readonly List<UpgradeDefinition> currentDraftOptions = new List<UpgradeDefinition>();
        private readonly HashSet<string> shownHints = new HashSet<string>();
        private bool hasUpgradeSelection;
        private Coroutine feedbackCoroutine;
        private bool gameEventsBound;
        private bool buildEventsBound;
        private bool selectionEventsBound;
        private TowerEvolutionDefinition selectedEvolution;
        private bool isPaused;

        private void Awake()
        {
            BindButton(startWaveButton, StartWave);
            BindButton(arrowTowerButton, () => SelectTower(TowerType.Arrow));
            BindButton(flameTowerButton, () => SelectTower(TowerType.Flame));
            BindButton(magicTowerButton, () => SelectTower(TowerType.Magic));
            BindButton(restartButton, RestartBattle);
            BindButton(backButton, BackToEntry);
            BindButton(flameWardenButton, () => SelectCommander(CommanderId.FlameWarden));
            BindButton(rangerCaptainButton, () => SelectCommander(CommanderId.RangerCaptain));
            BindButton(optionButtonA, () => SelectUpgrade(0));
            BindButton(optionButtonB, () => SelectUpgrade(1));
            BindButton(optionButtonC, () => SelectUpgrade(2));
            ShowHint("Build your first tower, then choose a commander to begin.");
        }

        private void Start()
        {
            TryBindEvents();
            PopulateCommanderTexts();
            SyncDraftFromGameState();
            Refresh();
        }

        private void OnEnable()
        {
            TryBindEvents();
            SyncDraftFromGameState();
        }

        private void OnDisable()
        {
            UnbindEvents();
        }

        private void Update()
        {
            if (!gameEventsBound || !buildEventsBound)
            {
                TryBindEvents();
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                TogglePause();
            }
        }

        public void Refresh()
        {
            GameManager game = GameManager.Instance;
            BuildManager build = BuildManager.Instance;

            SyncDraftFromGameState();

            if (game != null)
            {
                SetText(goldText, $"Gold: {game.Gold}");
                SetText(livesText, $"Lives: {game.Lives}");
                SetText(commanderInfoText, game.GetCommanderSummary());
            }

            if (waveManager != null)
            {
                SetText(waveText, $"Wave: {waveManager.CurrentWaveNumber}/{waveManager.TotalWaves}");
                SetText(enemyCountText, $"Enemies: {waveManager.LivingEnemies}");

                if (startWaveButton != null)
                {
                    bool canStart = game != null &&
                        game.ActiveCommander != null &&
                        game.FlowState == GameFlowState.Prepare &&
                        waveManager.HasMoreWaves &&
                        !waveManager.IsWaveRunning;
                    startWaveButton.interactable = canStart;
                }
            }

            if (build != null && game != null)
            {
                TowerConfig config = game.GetTowerConfig(build.SelectedTowerType);
                int cost = config != null ? game.GetBuildCost(config) : 0;
                builder.Clear();
                builder.Append("Build: ");
                builder.Append(build.SelectedTowerType);
                if (config != null)
                {
                    builder.Append(" (");
                    builder.Append(cost);
                    builder.Append(")");
                }

                SetText(selectedTowerText, builder.ToString());
            }

            if (commanderPanel != null && game != null)
            {
                commanderPanel.SetActive(game.ActiveCommander == null && !game.IsGameOver);
            }

            if (upgradePanel != null && game != null)
            {
                upgradePanel.SetActive(game.FlowState == GameFlowState.Upgrade && currentDraftOptions.Count > 0);
            }

            if (resultPanel != null && game != null)
            {
                resultPanel.SetActive(game.IsGameOver);
            }
        }

        private void StartWave()
        {
            GameAudio.Instance?.PlayButton();
            if (GameManager.Instance == null || waveManager == null)
            {
                return;
            }

            if (GameManager.Instance.ActiveCommander == null || GameManager.Instance.FlowState != GameFlowState.Prepare)
            {
                return;
            }

            waveManager.StartNextWave();
            if (waveManager.CurrentWaveNumber == 1)
            {
                ShowHint("Enemies follow the road. Protect the goal.");
            }

            if (waveManager.CurrentWaveNumber == 10)
            {
                ShowHint("Boss incoming. Spend gold and evolve your key towers.");
            }

            week2BattleFlow?.NotifyWaveStarted();
            Refresh();
        }

        private void SelectTower(TowerType type)
        {
            GameAudio.Instance?.PlayButton();
            BuildManager.Instance?.SelectTower((int)type);
            Refresh();
        }

        private void SelectCommander(CommanderId commanderId)
        {
            GameAudio.Instance?.PlayButton();
            GameManager.Instance?.SelectCommander(commanderId);
            Refresh();
        }

        private void SelectUpgrade(int index)
        {
            if (hasUpgradeSelection || index < 0 || index >= currentDraftOptions.Count)
            {
                return;
            }

            hasUpgradeSelection = true;
            GameAudio.Instance?.PlayButton();
            GameManager.Instance?.ApplyUpgrade(currentDraftOptions[index]);
        }

        private void HandleCommanderSelected(CommanderDefinition commander)
        {
            ShowFeedback($"{commander.displayName} selected");
            Refresh();
        }

        private void HandleUpgradeDraftReady(UpgradeDraftResult draft)
        {
            hasUpgradeSelection = false;
            currentDraftOptions.Clear();
            if (draft.Options != null)
            {
                currentDraftOptions.AddRange(draft.Options);
            }

            ApplyDraftOption(0, optionButtonA, optionTitleA, optionDescA);
            ApplyDraftOption(1, optionButtonB, optionTitleB, optionDescB);
            ApplyDraftOption(2, optionButtonC, optionTitleC, optionDescC);
            Refresh();
        }

        private void HandleUpgradeApplied(UpgradeDefinition definition)
        {
            currentDraftOptions.Clear();
            if (upgradePanel != null)
            {
                upgradePanel.SetActive(false);
            }

            ShowFeedback($"{definition.displayName} applied");
            Refresh();
        }

        private void SyncDraftFromGameState()
        {
            GameManager game = GameManager.Instance;
            if (game == null || game.FlowState != GameFlowState.Upgrade || currentDraftOptions.Count > 0)
            {
                return;
            }

            UpgradeDraftResult draft = game.CurrentUpgradeDraft;
            if (!draft.IsValid || draft.Options == null || draft.Options.Count == 0)
            {
                return;
            }

            HandleUpgradeDraftReady(draft);
        }

        private void HandleGameOver()
        {
            SetText(resultText, "Failure");
            SetText(resultTitleText, "Failure");
            SetText(resultSummaryText, GameManager.Instance != null ? GameManager.Instance.BuildSettlementSummary() : "Your lives reached zero.");
            Refresh();
        }

        private void HandleVictory()
        {
            SetText(resultText, "Victory");
            SetText(resultTitleText, "Victory");
            SetText(resultSummaryText, GameManager.Instance != null ? GameManager.Instance.BuildSettlementSummary() : "All waves have been cleared.");
            Refresh();
        }

        private void RestartBattle()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        private void BackToEntry()
        {
            Time.timeScale = 1f;
            string targetScene = Application.CanStreamedLevelBeLoaded("SampleScene") ? "SampleScene" : SceneManager.GetActiveScene().name;
            SceneManager.LoadScene(targetScene);
        }

        private void TogglePause()
        {
            if (GameManager.Instance != null && GameManager.Instance.FlowState == GameFlowState.Upgrade)
            {
                return;
            }

            SetPaused(!isPaused);
        }

        private void SetPaused(bool paused)
        {
            if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
            {
                paused = false;
            }

            isPaused = paused;
            Time.timeScale = isPaused ? 0f : 1f;
            Refresh();
        }

        private void ShowHint(string message)
        {
            if (!shownHints.Add(message))
            {
                return;
            }

            ShowFeedback(message);
        }

        private void ApplyDraftOption(int index, Button button, Text titleText, Text descText)
        {
            bool hasOption = index < currentDraftOptions.Count;
            if (button != null)
            {
                button.gameObject.SetActive(hasOption);
                button.interactable = hasOption && !hasUpgradeSelection;
            }

            if (!hasOption)
            {
                SetText(titleText, string.Empty);
                SetText(descText, string.Empty);
                return;
            }

            UpgradeDefinition option = currentDraftOptions[index];
            SetText(titleText, option.displayName);
            SetText(descText, option.description);
        }

        private void PopulateCommanderTexts()
        {
            if (GameManager.Instance == null)
            {
                return;
            }

            CommanderDefinition flame = GameManager.Instance.GetCommander(CommanderId.FlameWarden);
            CommanderDefinition ranger = GameManager.Instance.GetCommander(CommanderId.RangerCaptain);

            if (flame != null)
            {
                SetText(flameWardenNameText, flame.displayName);
                SetText(flameWardenDescText, flame.description);
            }

            if (ranger != null)
            {
                SetText(rangerCaptainNameText, ranger.displayName);
                SetText(rangerCaptainDescText, ranger.description);
            }
        }

        private void ShowFeedback(string message)
        {
            SetText(feedbackText, message);
            if (feedbackCoroutine != null)
            {
                StopCoroutine(feedbackCoroutine);
            }

            feedbackCoroutine = StartCoroutine(ClearFeedbackAfterDelay(2f));
        }

        private IEnumerator ClearFeedbackAfterDelay(float delay)
        {
            yield return new WaitForSecondsRealtime(delay);
            SetText(feedbackText, string.Empty);
            feedbackCoroutine = null;
        }

        private void TryBindEvents()
        {
            if (!gameEventsBound && GameManager.Instance != null)
            {
                GameManager.Instance.StateChanged += Refresh;
                GameManager.Instance.GameOver += HandleGameOver;
                GameManager.Instance.Victory += HandleVictory;
                GameManager.Instance.CommanderSelected += HandleCommanderSelected;
                GameManager.Instance.UpgradeDraftReady += HandleUpgradeDraftReady;
                GameManager.Instance.UpgradeApplied += HandleUpgradeApplied;
                gameEventsBound = true;
            }

            if (!buildEventsBound && BuildManager.Instance != null)
            {
                BuildManager.Instance.SelectedTowerChanged += Refresh;
                buildEventsBound = true;
            }

            if (!selectionEventsBound && TowerSelectionManager.Instance != null)
            {
                TowerSelectionManager.Instance.SelectedTowerChanged += HandleSelectedTowerChanged;
                selectionEventsBound = true;
            }
        }

        private void UnbindEvents()
        {
            if (gameEventsBound && GameManager.Instance != null)
            {
                GameManager.Instance.StateChanged -= Refresh;
                GameManager.Instance.GameOver -= HandleGameOver;
                GameManager.Instance.Victory -= HandleVictory;
                GameManager.Instance.CommanderSelected -= HandleCommanderSelected;
                GameManager.Instance.UpgradeDraftReady -= HandleUpgradeDraftReady;
                GameManager.Instance.UpgradeApplied -= HandleUpgradeApplied;
            }

            if (buildEventsBound && BuildManager.Instance != null)
            {
                BuildManager.Instance.SelectedTowerChanged -= Refresh;
            }

            if (selectionEventsBound && TowerSelectionManager.Instance != null)
            {
                TowerSelectionManager.Instance.SelectedTowerChanged -= HandleSelectedTowerChanged;
            }

            gameEventsBound = false;
            buildEventsBound = false;
            selectionEventsBound = false;
        }

        private void OnGUI()
        {
            if (!enableFallbackHud)
            {
                DrawSelectedTowerFallback(GameManager.Instance, 12f);
                return;
            }

            GameManager game = GameManager.Instance;
            BuildManager build = BuildManager.Instance;

            const float margin = 12f;
            GUI.Box(new Rect(margin, margin, 250f, 180f), string.Empty);
            GUI.Label(new Rect(margin + 12f, margin + 10f, 220f, 24f), game != null ? $"Gold: {game.Gold}" : "Gold: -");
            GUI.Label(new Rect(margin + 12f, margin + 36f, 220f, 24f), game != null ? $"Lives: {game.Lives}" : "Lives: -");
            GUI.Label(new Rect(margin + 12f, margin + 62f, 220f, 24f), waveManager != null ? $"Wave: {waveManager.CurrentWaveNumber}/{waveManager.TotalWaves}" : "Wave: -");
            GUI.Label(new Rect(margin + 12f, margin + 88f, 220f, 24f), waveManager != null ? $"Enemies: {waveManager.LivingEnemies}" : "Enemies: -");
            GUI.Label(new Rect(margin + 12f, margin + 114f, 220f, 24f), build != null ? $"Tower: {build.SelectedTowerType}" : "Tower: -");
            GUI.Label(new Rect(margin + 12f, margin + 140f, 220f, 32f), game != null ? game.GetCommanderSummary() : "Commander: None");
            DrawSelectedTowerFallback(game, margin);

            float right = Screen.width - 172f;
            if (game != null && game.ActiveCommander == null)
            {
                if (GUI.Button(new Rect(right, margin, 160f, 36f), "Flame Warden"))
                {
                    SelectCommander(CommanderId.FlameWarden);
                }

                if (GUI.Button(new Rect(right, margin + 42f, 160f, 36f), "Ranger Captain"))
                {
                    SelectCommander(CommanderId.RangerCaptain);
                }
            }
            else if (game != null && game.FlowState == GameFlowState.Prepare && waveManager != null && waveManager.HasMoreWaves)
            {
                if (GUI.Button(new Rect(right, margin, 160f, 36f), "Start Game"))
                {
                    StartWave();
                }
            }

            if (GUI.Button(new Rect(right, margin + 88f, 76f, 34f), "Arrow"))
            {
                SelectTower(TowerType.Arrow);
            }

            if (GUI.Button(new Rect(right + 84f, margin + 88f, 76f, 34f), "Flame"))
            {
                SelectTower(TowerType.Flame);
            }

            if (GUI.Button(new Rect(right, margin + 128f, 160f, 34f), "Magic"))
            {
                SelectTower(TowerType.Magic);
            }

            if (game != null && game.FlowState == GameFlowState.Upgrade && currentDraftOptions.Count > 0)
            {
                GUI.Box(new Rect(Screen.width * 0.5f - 260f, Screen.height * 0.5f - 110f, 520f, 220f), "Choose Upgrade");
                for (int i = 0; i < currentDraftOptions.Count; i++)
                {
                    UpgradeDefinition option = currentDraftOptions[i];
                    Rect buttonRect = new Rect(Screen.width * 0.5f - 240f + i * 160f, Screen.height * 0.5f - 60f, 140f, 120f);
                    if (GUI.Button(buttonRect, $"{option.displayName}\n{option.description}"))
                    {
                        SelectUpgrade(i);
                    }
                }
            }

            if (isPaused)
            {
                DrawPauseMenu();
            }

            if (game != null && game.IsGameOver)
            {
                GUI.Box(new Rect(Screen.width * 0.5f - 210f, Screen.height * 0.5f - 150f, 420f, 300f), string.Empty);
                GUI.Label(new Rect(Screen.width * 0.5f - 180f, Screen.height * 0.5f - 120f, 360f, 180f), game.BuildSettlementSummary());
                if (GUI.Button(new Rect(Screen.width * 0.5f - 120f, Screen.height * 0.5f + 90f, 110f, 34f), "Restart"))
                {
                    RestartBattle();
                }

                if (GUI.Button(new Rect(Screen.width * 0.5f + 10f, Screen.height * 0.5f + 90f, 110f, 34f), "Back"))
                {
                    BackToEntry();
                }
            }
        }

        private void DrawPauseMenu()
        {
            GUI.Box(new Rect(Screen.width * 0.5f - 150f, Screen.height * 0.5f - 130f, 300f, 260f), "Paused");
            if (GUI.Button(new Rect(Screen.width * 0.5f - 100f, Screen.height * 0.5f - 82f, 200f, 34f), "Continue"))
            {
                GameAudio.Instance?.PlayButton();
                SetPaused(false);
            }

            GUI.Label(new Rect(Screen.width * 0.5f - 100f, Screen.height * 0.5f - 36f, 200f, 24f), "Volume");
            if (GameAudio.Instance != null)
            {
                GameAudio.Instance.Volume = GUI.HorizontalSlider(new Rect(Screen.width * 0.5f - 100f, Screen.height * 0.5f - 8f, 200f, 20f), GameAudio.Instance.Volume, 0f, 1f);
            }

            if (GUI.Button(new Rect(Screen.width * 0.5f - 100f, Screen.height * 0.5f + 28f, 200f, 34f), "Restart"))
            {
                RestartBattle();
            }

            if (GUI.Button(new Rect(Screen.width * 0.5f - 100f, Screen.height * 0.5f + 72f, 200f, 34f), "Back"))
            {
                BackToEntry();
            }
        }

        private static void BindButton(Button button, UnityEngine.Events.UnityAction action)
        {
            if (button != null)
            {
                button.onClick.AddListener(action);
            }
        }

        private static void SetText(Text text, string value)
        {
            if (text != null)
            {
                text.text = value;
            }
        }

        private void HandleSelectedTowerChanged(TowerController _)
        {
            selectedEvolution = null;
            Refresh();
        }

        private void DrawSelectedTowerFallback(GameManager game, float margin)
        {
            TowerController tower = TowerSelectionManager.Instance != null ? TowerSelectionManager.Instance.SelectedTower : null;
            if (game == null || tower == null)
            {
                return;
            }

            Rect panel = new Rect(margin, margin + 204f, 330f, 210f);
            GUI.Box(panel, "Tower Detail");
            GUI.Label(new Rect(panel.x + 12f, panel.y + 28f, 300f, 24f), $"{tower.DisplayName} Lv{tower.Level} XP {tower.Experience}/100");
            GUI.Label(new Rect(panel.x + 12f, panel.y + 54f, 300f, 24f), tower.IsEvolved ? $"Branch: {tower.BranchId}" : "Branch: Base");
            GUI.Label(new Rect(panel.x + 12f, panel.y + 80f, 300f, 24f), $"DMG {tower.RuntimeStats.Damage:0}  RNG {tower.RuntimeStats.Range:0.0}  INT {tower.RuntimeStats.AttackInterval:0.00}");

            if (tower.IsEvolved)
            {
                GUI.Label(new Rect(panel.x + 12f, panel.y + 112f, 300f, 48f), tower.Evolution != null ? tower.Evolution.description : "Evolved tower");
                return;
            }

            IReadOnlyList<TowerEvolutionDefinition> evolutions = game.GetEvolutionsFor(tower.Type);
            for (int i = 0; i < evolutions.Count && i < 2; i++)
            {
                TowerEvolutionDefinition option = evolutions[i];
                Rect buttonRect = new Rect(panel.x + 12f + i * 154f, panel.y + 112f, 146f, 38f);
                bool selected = selectedEvolution == option;
                if (GUI.Button(buttonRect, selected ? $"> {option.displayName}" : option.displayName))
                {
                    selectedEvolution = option;
                }
            }

            if (selectedEvolution != null)
            {
                ShowHint("Choose a branch evolution to change this tower's role.");
                string reason;
                bool canEvolve = tower.CanEvolve(selectedEvolution, out reason);
                GUI.Label(new Rect(panel.x + 12f, panel.y + 154f, 200f, 24f), $"{selectedEvolution.gameplayTags} Cost {selectedEvolution.evolveCost}");
                GUI.enabled = canEvolve;
                if (GUI.Button(new Rect(panel.x + 218f, panel.y + 154f, 90f, 30f), "Evolve"))
                {
                    if (tower.TryEvolve(selectedEvolution))
                    {
                        ShowFeedback($"{tower.DisplayName} evolved");
                        selectedEvolution = null;
                    }
                }

                GUI.enabled = true;
                if (!canEvolve)
                {
                    GUI.Label(new Rect(panel.x + 12f, panel.y + 182f, 300f, 24f), reason);
                }
            }
        }
    }
}
