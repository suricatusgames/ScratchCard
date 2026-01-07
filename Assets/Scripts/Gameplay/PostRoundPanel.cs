using System;
using TMPro;
using UnityEngine;

namespace Gameplay
{
    public class PostRoundPanel : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TMP_Text gameOverText;
        [SerializeField] private TMP_Text messageText;
        [SerializeField] private GameObject panelContainer;
    
        [Header("Win Text")]
        [SerializeField] private string winTitleText = "PARABÉNS!";
        [SerializeField] private string winMessageText = "Você encontrou 3 prêmios iguais!";
    
        [Header("Lose Text")]
        [SerializeField] private string loseTitleText = "GAME OVER";
        [SerializeField] private string loseMessageText = "Você usou todas as raspadinhas disponíveis!";
    
        [Header("Auto Transition")]
        [SerializeField] private float displayDuration = 1.5f;

        public event Action OnDisplayComplete;

        private bool _isShowing = false;

        public void ShowWin()
        {
            Debug.Log("PostRoundPanel: Showing WIN panel");
            SetTexts(winTitleText, winMessageText);
            Show();
        }

        public void ShowLose()
        {
            Debug.Log("PostRoundPanel: Showing LOSE panel");
            SetTexts(loseTitleText, loseMessageText);
            Show();
        }

        public void Show()
        {
            _isShowing = true;
            
            if (panelContainer != null)
            {
                panelContainer.SetActive(true);
            }

            Invoke(nameof(OnComplete), displayDuration);
        }

        public void Hide()
        {
            if (panelContainer != null)
            {
                panelContainer.SetActive(false);
            }
        }

        private void SetTexts(string title, string message)
        {
            if (gameOverText != null)
            {
                gameOverText.text = title;
            }

            if (messageText != null)
            {
                messageText.text = message;
            }
        }

        public void SetCustomTexts(string title, string message)
        {
            SetTexts(title, message);
        }

        public void SetDisplayDuration(float duration)
        {
            displayDuration = duration;
        }

        private void OnComplete()
        {
            OnDisplayComplete?.Invoke();
        }
    }
}
