using System;
using TMPro;
using Tools.SoundManager.Services;
using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Zenject;

namespace Victory
{
    public class VictorySceneController : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image prizeDisplayImage;
        [SerializeField] private TextMeshProUGUI victoryMessageText;
        [SerializeField] private Button backButton;
        
        [Header("Prize Sprites")]
        [SerializeField] private Sprite[] allPrizeSprites;
        
        [Header("Defeat Settings")]
        [SerializeField] private Sprite defeatImage;

        [Header("Audio Settings")] 
        [SerializeField] private AudioClip victoryMusic;
        [SerializeField] private AudioClip defeatMusic;
        [SerializeField] private float musicVolume = 0.7f;
        
        [Header("Scene Settings")]
        [SerializeField] private string mainMenuSceneName = "MainMenuScene";
        
        [Inject] private ISoundManager _soundManager;
        
        private float _originalMusicVolume;
        
        private const string WinningPrizeKey = "WinningPrizeName";
        private const string PlayerNameKey = "PlayerName";
        private const string GameResultKey = "GameResult";

        private async void Start()
        {
            if (_soundManager != null)
            {
                _originalMusicVolume = _soundManager.GetMusicVolume();
                _soundManager.SetMusicVolume(musicVolume);
            }

            var gameResult = PlayerPrefs.GetString(GameResultKey, "Lose");

            if (gameResult == "Win")
            {
                DisplayVictory();

                if (_soundManager != null && victoryMusic != null)
                    await _soundManager.PlayMusic(victoryMusic, loop: true);
            }
            else
            {
                DisplayDefeat();

                if (_soundManager != null && defeatMusic != null)
                    await _soundManager.PlayMusic(defeatMusic, loop: true);
            }

            if (backButton != null)
                backButton.onClick.AddListener(OnBackButtonClicked);
        }

        private void OnBackButtonClicked()
        {
            LoadMainMenuScene();
        }

        private void LoadMainMenuScene()
        {
            if (_soundManager != null)
                _soundManager.SetMusicVolume(_originalMusicVolume);
            
            SceneManager.LoadScene(mainMenuSceneName);
        }

        private void DisplayVictory()
        {
            DisplayWinningPrize();
            DisplayVictoryMessage();
        }

        private void DisplayDefeat()
        {
            DisplayDefeatImage();
            DisplayDefeatMessage();
        }

        private void DisplayWinningPrize()
        {
            var winningPrizeName = PlayerPrefs.GetString(WinningPrizeKey, "");

            if (string.IsNullOrEmpty(winningPrizeName))
            {
                Debug.LogWarning("No winning prize name saved in PlayerPrefs!");
                return;
            }
            
            var winningSprite = FindSpriteByName(winningPrizeName);

            if (winningSprite != null && prizeDisplayImage != null)
            {
                prizeDisplayImage.sprite = winningSprite;
                prizeDisplayImage.enabled = true;
            }
            else
            {
                Debug.LogWarning($"Could not find sprite '{winningPrizeName}' or Image component is null");
            }
        }

        private void DisplayDefeatImage()
        {
            if (prizeDisplayImage != null && defeatImage != null)
            {
                prizeDisplayImage.sprite = defeatImage;
                prizeDisplayImage.enabled = true;
            }
        }
        
        private void DisplayVictoryMessage()
        {
            if (victoryMessageText == null)
                return;
            
            var playerName = PlayerPrefs.GetString(PlayerNameKey, "Jogador");
            victoryMessageText.text = $"Parabéns, {playerName}! Você ganhou:";
        }

        private void DisplayDefeatMessage()
        {
            if (victoryMessageText == null)
                return;
            
            var playerName = PlayerPrefs.GetString(PlayerNameKey, "Jogador");
            victoryMessageText.text = $"Não foi dessa vez, {playerName}!";
        }

        private Sprite FindSpriteByName(string spriteName)
        {
            foreach (var sprite in allPrizeSprites)
            {
                if (sprite.name == spriteName)
                {
                    return sprite;
                }
            }
            
            return null;
        }

        private void OnDestroy()
        {
            if (backButton != null)
                backButton.onClick.RemoveListener(OnBackButtonClicked);
        }
    }
}
