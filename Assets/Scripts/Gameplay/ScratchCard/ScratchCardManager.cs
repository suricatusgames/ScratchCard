using System;
using System.Collections.Generic;
using Gameplay;
using TMPro;
using Tools.SoundManager.Services;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Utils;
using Zenject;

namespace ScratchCard
{
    public class ScratchCardManager : MonoBehaviour
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject scratchCardPrefab;
        [SerializeField] private GameObject preRoundPanelPrefab;
        [SerializeField] private GameObject postRoundPanelPrefab;
        
        [Header("Prizes")]
        [SerializeField] private Sprite[] prizeSprites;

        [Header("Settings")]
        [SerializeField] private Transform cardsContainer;
        [SerializeField] private int totalCardsToDisplay = 10;
        [SerializeField] private int minAvailableCards = 4;
        [SerializeField] private int maxAvailableCards = 10;
        [SerializeField] private GridLayoutGroup gridLayout;
        
        [Header("UI References")]
        [SerializeField] private TMP_Text availableCardsText;
        [SerializeField] private Canvas mainCanvas;
        
        [Header("Services")]
        [SerializeField] private GoogleSheetsService googleSheetsService;
        
        [Header("Audio Settings")]
        [SerializeField] private AudioClip gameplayMusic;
        [SerializeField] private float gameplayMusicVolume = 0.3f;
        
        [Header("Scene Settings")]
        [SerializeField] private string victorySceneName = "VictoryScene";
        
        private readonly List<Sprite> _revealedPrizes = new List<Sprite>();
        private readonly List<Gameplay.ScratchCard.ScratchCard> _allCards = new List<Gameplay.ScratchCard.ScratchCard>();
        private Gameplay.ScratchCard.ScratchCard _currentActiveCard;
        
        private int _maxAvailableScratches;
        private int _remainingScratches;
        private bool _hasWon = false;
        private bool _dataSent = false;
        private bool _gameStarted = false;
        
        private PreRoundPanel _preRoundPanel;
        private PostRoundPanel _postRoundPanel;
        
        private const string WinningPrizeKey = "WinningPrizeName";
        private const string GameResultKey = "GameResult";

        private float _originalMusicVolume;
        
        [Inject] private DiContainer _container;
        [Inject] private ISoundManager _soundManager;

        private async void Start()
        {
            if (mainCanvas == null)
            {
                mainCanvas = FindObjectOfType<Canvas>();
            }

            if (googleSheetsService == null)
            {
                googleSheetsService = FindObjectOfType<GoogleSheetsService>();
        
                if (googleSheetsService == null)
                {
                    var serviceObj = new GameObject("GoogleSheetsService");
                    googleSheetsService = serviceObj.AddComponent<GoogleSheetsService>();
                    DontDestroyOnLoad(serviceObj);
                }
            }
    
            if (_soundManager != null)
            {
                _originalMusicVolume = _soundManager.GetMusicVolume();
                _soundManager.SetMusicVolume(gameplayMusicVolume);
        
                if (gameplayMusic != null)
                {
                    await _soundManager.PlayMusic(gameplayMusic);
                }
            }
    
            _maxAvailableScratches = UnityEngine.Random.Range(minAvailableCards, maxAvailableCards + 1);
            _remainingScratches = _maxAvailableScratches;
    
            SetupGridLayout();
            InstantiateScratchCards();
            ShowPreRoundPanel();
    
            LockAllCards();
        }


        private void ShowPreRoundPanel()
        {
            if (preRoundPanelPrefab != null)
            {
                GameObject panelObj = Instantiate(preRoundPanelPrefab, mainCanvas.transform);
                _preRoundPanel = panelObj.GetComponent<PreRoundPanel>();

                if (_preRoundPanel != null)
                {
                    _preRoundPanel.SetScratchQuantity(_maxAvailableScratches);
                    _preRoundPanel.OnStartButtonClicked += OnGameStarted;
                    _preRoundPanel.Show();
                }
            }
            else
            {
                Debug.LogWarning("PreRoundPanel prefab not assigned!");
                OnGameStarted();
            }
        }

        private void ShowPostRoundPanel(bool isWin)
        {
            Debug.Log($"ScratchCardManager: ShowPostRoundPanel called with isWin={isWin}");
            
            if (postRoundPanelPrefab != null)
            {
                GameObject panelObj = Instantiate(postRoundPanelPrefab, mainCanvas.transform);
                _postRoundPanel = panelObj.GetComponent<PostRoundPanel>();

                if (_postRoundPanel != null)
                {
                    Debug.Log("PostRoundPanel component found");
                    _postRoundPanel.OnDisplayComplete += LoadVictoryScene;
                    
                    if (isWin)
                    {
                        _postRoundPanel.ShowWin();
                    }
                    else
                    {
                        _postRoundPanel.ShowLose();
                    }
                }
                else
                {
                    Debug.LogError("PostRoundPanel component not found on instantiated prefab!");
                }
            }
            else
            {
                Debug.LogWarning("PostRoundPanel prefab not assigned! Loading victory scene directly.");
                LoadVictoryScene();
            }
        }

        private void OnGameStarted()
        {
            _gameStarted = true;
            UnlockAllUnscratched();
            UpdateCardsCountDisplay();
        }

        private void SetupGridLayout()
        {
            if (gridLayout == null)
            {
                gridLayout = cardsContainer.GetComponent<GridLayoutGroup>();

                if (gridLayout == null)
                {
                    gridLayout = cardsContainer.gameObject.AddComponent<GridLayoutGroup>();
                }
            }
            
            gridLayout.cellSize = new Vector2(450, 300);
            gridLayout.spacing = new Vector2(40.19f, 39.49f);
            gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayout.constraintCount = 2;
        }
        
        private void InstantiateScratchCards()
        {
            float staggerDelay = 0.08f;
            
            for (int i = 0; i < totalCardsToDisplay; i++)
            {
                var cardObject = _container.InstantiatePrefab(scratchCardPrefab, cardsContainer);
                var card = cardObject.GetComponent<Gameplay.ScratchCard.ScratchCard>();

                card.SetSpawnDelay(i * staggerDelay);
                
                var randomPrize = GetRandomPrize();
                card.Initialize(randomPrize);
                card.OnPrizeRevealed += OnPrizeRevealed;
                card.OnCardStarted += OnCardStarted;
                card.OnCardUnlocked += OnCardUnlocked;
            
                card.SetLocked(true);
                _allCards.Add(card);
            }
        }

        private void OnCardStarted(Gameplay.ScratchCard.ScratchCard card)
        {
            if (!_gameStarted)
                return;

            if (_remainingScratches <= 0)
            {
                card.SetLocked(true);
                return;
            }

            if (_currentActiveCard != null && _currentActiveCard != card)
            {
                return;
            }

            _currentActiveCard = card;
            card.SetActive(true);
            LockAllCardsExcept(card);
        }

        private void OnCardUnlocked(Gameplay.ScratchCard.ScratchCard card)
        {
            if (_currentActiveCard == card)
            {
                _currentActiveCard = null;
                card.SetActive(false);
        
                if (_remainingScratches > 0)
                {
                    UnlockAllUnscratched();
                }
                else
                {
                    LockAllCards();
                }
            }
        }

        private void LockAllCardsExcept(Gameplay.ScratchCard.ScratchCard activeCard)
        {
            foreach (var card in _allCards)
            {
                if (card != activeCard && !card.IsScratched)
                {
                    card.SetLocked(true);
                }
            }
        }

        private void UnlockAllUnscratched()
        {
            foreach (var card in _allCards)
            {
                if (!card.IsScratched)
                {
                    card.SetLocked(false);
                }
            }
        }

        private void LockAllCards()
        {
            foreach (var card in _allCards)
            {
                if (!card.IsScratched)
                {
                    card.SetLocked(true);
                }
            }
        }

        private void OnPrizeRevealed(Sprite revealedSprite)
        {
            _remainingScratches--;
            _revealedPrizes.Add(revealedSprite);
            UpdateCardsCountDisplay();
            
            CheckForMatch(revealedSprite);
            
            if (_remainingScratches <= 0 && !_hasWon)
            {
                LockAllCards();
                OnDefeat();
            }
            else if (_remainingScratches <= 0)
            {
                LockAllCards();
            }
        }

        private void UpdateCardsCountDisplay()
        {
            if (availableCardsText != null)
            {
                availableCardsText.text = $"Quantidade de raspadinhas disponíveis: {_remainingScratches}/{_maxAvailableScratches}";
            }
        }

        private void CheckForMatch(Sprite newlyRevealed)
        {
            var matchCount = 0;
            foreach (var prize in _revealedPrizes)
            {
                if (prize == newlyRevealed)
                    matchCount++;
            }

            if (matchCount >= 3)
            {
                OnMatchFound(newlyRevealed);
            }
        }

        private void OnMatchFound(Sprite matchedSprite)
        {
            _hasWon = true;
            LockAllCards();
            
            PlayerPrefs.SetString(WinningPrizeKey, matchedSprite.name);
            PlayerPrefs.SetString(GameResultKey, "Win");
            PlayerPrefs.Save();
            
            SendGameResultToGoogleSheets("Win", matchedSprite.name);
            
            ShowPostRoundPanel(true);
        }

        private void OnDefeat()
        {
            PlayerPrefs.SetString(GameResultKey, "Lose");
            PlayerPrefs.Save();
            
            SendGameResultToGoogleSheets("Lose", "None");
            
            ShowPostRoundPanel(false);
        }

        private void SendGameResultToGoogleSheets(string result, string prizeName)
        {
            if (_dataSent)
                return;
            
            _dataSent = true;

            var playerName = PlayerPrefs.GetString("PlayerName", "Unknown");
            var playerEmail = PlayerPrefs.GetString("PlayerEmail", "Unknown");
            var playerPhone = PlayerPrefs.GetString("PlayerCellphone", "Unknown");

            if (googleSheetsService != null)
            {
                googleSheetsService.SendPlayerDataWithResult(
                    playerName, 
                    playerEmail, 
                    playerPhone, 
                    result, 
                    prizeName,
                    (success) =>
                    {
                        if (success)
                        {
                            Debug.Log($"Game result sent to Google Sheets: {result}");
                        }
                        else
                        {
                            Debug.LogWarning("Failed to send game result to Google Sheets");
                        }
                    }
                );
            }
            else
            {
                Debug.LogWarning("GoogleSheetsService not found!");
            }
        }

        private void LoadVictoryScene()
        {
            if (_soundManager != null)
                _soundManager.SetMusicVolume(_originalMusicVolume);
            SceneManager.LoadScene(victorySceneName);
        }

        private Sprite GetRandomPrize()
        {
            if (prizeSprites.Length == 0)
            {
                return null;
            }
            return prizeSprites[UnityEngine.Random.Range(0, prizeSprites.Length)];
        }
    }
}
