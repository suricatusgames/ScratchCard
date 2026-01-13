using System;
using DG.Tweening;
using TMPro;
using Tools.SoundManager.Services;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Gameplay
{
    public class PostRoundPanel : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TMP_Text gameOverText;
        [SerializeField] private TMP_Text messageText;
        [SerializeField] private GameObject panelContainer;
        [SerializeField] private Image panelBackground;
        [SerializeField] private CanvasGroup panelCanvasGroup;
    
        [Header("Win Text")]
        [SerializeField] private string winTitleText = "PARABÉNS!";
        [SerializeField] private string winMessageText = "Você encontrou 3 prêmios iguais!";
    
        [Header("Lose Text")]
        [SerializeField] private string loseTitleText = "GAME OVER";
        [SerializeField] private string loseMessageText = "Você usou todas as raspadinhas disponíveis!";
    
        [Header("Auto Transition")]
        [SerializeField] private float displayDuration = 1.5f;
        
        [Header("Audio Settings")]
        [SerializeField] private AudioClip winSound;
        [SerializeField] private AudioClip loseSound;
        
        [Header("Win VFX")]
        [SerializeField] private ParticleSystem winParticles;
        [SerializeField] private Color winFlashColor = new Color(1f, 0.84f, 0f, 0.3f);
        [SerializeField] private float winScaleAmount = 1.2f;
        
        [Header("Lose VFX")]
        [SerializeField] private ParticleSystem loseParticles;
        [SerializeField] private Color loseFlashColor = new Color(0.3f, 0.3f, 0.3f, 0.3f);
        [SerializeField] private float loseShakeStrength = 20f;
        
        [Header("Animation Settings")]
        [SerializeField] private float entranceAnimationDuration = 0.8f;
        [SerializeField] private float textAnimationDelay = 0.3f;
        [SerializeField] private float flashDuration = 0.5f;
        
        [Inject] private ISoundManager _soundManager;
        
        public event Action OnDisplayComplete;

        private bool _isShowing = false;

        public void ShowWin()
        {
            SetTexts(winTitleText, winMessageText);
            PlayWinAnimation();
            _soundManager.PlaySfx(winSound);
        }

        public void ShowLose()
        {
            SetTexts(loseTitleText, loseMessageText);
            PlayLoseAnimation();
            _soundManager.PlaySfx(loseSound);
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
        
        private void PlayWinAnimation()
        {
            if (panelContainer == null) return;
            
            _isShowing = true;
            panelContainer.SetActive(true);
            
            RectTransform panelRect = panelContainer.GetComponent<RectTransform>();
            if (panelRect == null) return;
            
            panelRect.localScale = Vector3.zero;
            if (panelCanvasGroup != null) panelCanvasGroup.alpha = 0f;
            
            if (gameOverText != null) gameOverText.alpha = 0f;
            if (messageText != null) messageText.alpha = 0f;
            
            var winSequence = DOTween.Sequence();
            
            if (panelCanvasGroup != null)
            {
                winSequence.Append(panelCanvasGroup.DOFade(1f, entranceAnimationDuration * 0.3f));
            }
            
            winSequence.Append(panelRect.DOScale(winScaleAmount, entranceAnimationDuration * 0.5f)
                .SetEase(Ease.OutBack));
            winSequence.Append(panelRect.DOScale(1f, entranceAnimationDuration * 0.3f)
                .SetEase(Ease.InOutQuad));
            
            if (panelBackground != null)
            {
                var originalColor = panelBackground.color;
                winSequence.Join(panelBackground.DOColor(winFlashColor, flashDuration * 0.5f)
                    .SetLoops(2, LoopType.Yoyo));
            }
            
            winSequence.Join(panelRect.DOPunchRotation(new Vector3(0, 0, 5), entranceAnimationDuration, 5, 0.5f));
            
            if (gameOverText != null)
            {
                winSequence.Insert(textAnimationDelay, gameOverText.DOFade(1f, 0.5f).SetEase(Ease.OutQuad));
                winSequence.Insert(textAnimationDelay, gameOverText.rectTransform.DOPunchScale(Vector3.one * 0.3f, 0.5f, 5, 0.5f));
            }
            
            if (messageText != null)
            {
                winSequence.Insert(textAnimationDelay + 0.2f, messageText.DOFade(1f, 0.5f).SetEase(Ease.OutQuad));
            }
            
            if (winParticles != null)
            {
                winSequence.InsertCallback(entranceAnimationDuration * 0.5f, () =>
                {
                    winParticles.Clear();
                    winParticles.Play();
                });
            }
            
            winSequence.OnComplete(() =>
            {
                Invoke(nameof(OnComplete), displayDuration);
            });
        }
        
        private void PlayLoseAnimation()
        {
            if (panelContainer == null) return;
            
            _isShowing = true;
            panelContainer.SetActive(true);
            
            var panelRect = panelContainer.GetComponent<RectTransform>();
            if (panelRect == null) return;
            
            panelRect.localScale = Vector3.one;
            if (panelCanvasGroup != null) panelCanvasGroup.alpha = 0f;
            
            if (gameOverText != null) gameOverText.alpha = 0f;
            if (messageText != null) messageText.alpha = 0f;
            
            var loseSequence = DOTween.Sequence();
            
            if (panelCanvasGroup != null)
            {
                loseSequence.Append(panelCanvasGroup.DOFade(1f, entranceAnimationDuration));
            }
            
            loseSequence.Join(panelRect.DOShakePosition(entranceAnimationDuration * 0.6f, loseShakeStrength, 10, 45, false, true)
                .SetEase(Ease.OutQuad));
            
            if (panelBackground != null)
            { 
                var originalColor = panelBackground.color;
                loseSequence.Join(panelBackground.DOColor(loseFlashColor, flashDuration)
                    .SetEase(Ease.InOutQuad));
            }
            
            if (gameOverText != null)
            {
                loseSequence.Insert(textAnimationDelay, gameOverText.DOFade(1f, 0.6f).SetEase(Ease.OutQuad));
                loseSequence.Insert(textAnimationDelay, gameOverText.rectTransform.DOShakeRotation(0.5f, new Vector3(0, 0, 10), 5, 45));
            }
            
            if (messageText != null)
            {
                loseSequence.Insert(textAnimationDelay + 0.3f, messageText.DOFade(1f, 0.5f).SetEase(Ease.OutQuad));
            }
            
            if (loseParticles != null)
            {
                loseSequence.InsertCallback(entranceAnimationDuration * 0.3f, () =>
                {
                    loseParticles.Clear();
                    loseParticles.Play();
                });
            }
            
            loseSequence.OnComplete(() =>
            {
                Invoke(nameof(OnComplete), displayDuration);
            });
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
