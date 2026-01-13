using System;
using DG.Tweening;
using Tools.SoundManager.Services;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

namespace Gameplay.ScratchCard
{
    public class ScratchCard : MonoBehaviour, IPointerDownHandler, IDragHandler
    {
        [Header("References")]
        [SerializeField] private RawImage scratchLayer;
        [SerializeField] private Image prizeImage;
        [SerializeField] private GameObject activeBorder;
        [SerializeField] private Image progressRing;
        [SerializeField] private Image prizeFlashOverlay;

        [Header("Settings")]
        [SerializeField] private int textureWidth = 512;
        [SerializeField] private int textureHeight = 512;
        [SerializeField] private int brushSize = 30;
        [SerializeField] private float unlockThreshold = 0.5f;
        [SerializeField] private float revealThreshold = 0.75f;
        
        [Header("Border Settings")]
        [SerializeField] private Color borderColor = Color.yellow;
        [SerializeField] private float borderWidth = 10f;

        [Header("Prize Reveal VFX")]
        [SerializeField] private ParticleSystem prizeRevealParticles;
        [SerializeField] private float shakeStrength = 20f;
        [SerializeField] private float shakeDuration = 0.5f;
        [SerializeField] private float shakeRandomness = 90f;
        [SerializeField] private int shakeVibrato = 10;
        
        [Header("Spawn Animation")]
        [SerializeField] private float spawnDelay = 0f;
        [SerializeField] private float spawnDuration = 0.6f;
        [SerializeField] private float spawnRotation = 180f;
        
        [Header("Progress Ring VFX")]
        [SerializeField] private bool showProgressRing = true;
        [SerializeField] private Color progressColorStart = Color.red;
        [SerializeField] private Color progressColorEnd = Color.green;
        
        [Header("Prize Reveal Animation")]
        [SerializeField] private float prizeScaleDuration = 0.8f;
        [SerializeField] private float prizeScaleAmount = 1.3f;
        [SerializeField] private float prizeRotationAmount = 360f;
        [SerializeField] private Color flashColor = Color.white;

        [Header("Audio")]
        [SerializeField] private AudioClip scratchSound;

        [Inject] private ISoundManager _soundManager;
        
        private Texture2D _scratchTexture;
        private Color[] _pixels;
        private int _totalPixels;
        private int _scratchedPixels;
        private bool _isScratched;
        private bool _isLocked = true;
        private bool _hasUnlocked = false;
        private Sprite _assignedPrize;
        
        public event Action<Sprite> OnPrizeRevealed;
        public event Action<ScratchCard> OnCardStarted;
        public event Action<ScratchCard> OnCardUnlocked;

        public bool IsLocked => _isLocked;
        public bool IsScratched => _isScratched;
        public float ScratchPercentage => (float)_scratchedPixels / _totalPixels;
        
        private async void Awake()
        {
            if (activeBorder != null)
            {
                activeBorder.SetActive(false);
            }
            
            if (progressRing != null)
            {
                progressRing.fillAmount = 0f;
                progressRing.gameObject.SetActive(false);
            }
            
            if (prizeFlashOverlay != null)
            {
                prizeFlashOverlay.color = new Color(flashColor.r, flashColor.g, flashColor.b, 0f);
            }
            
            EnsureParticleSystemSetup();
        }
        
        private void EnsureParticleSystemSetup()
        {
            if (prizeRevealParticles != null && prizeRevealParticles.transform.parent == transform)
            {
                var parentCanvas = GetComponentInParent<Canvas>();
                if (parentCanvas != null)
                {
                    prizeRevealParticles.transform.SetParent(parentCanvas.transform, false);
                    prizeRevealParticles.transform.SetAsLastSibling();
                    
                    var rectTransform = prizeRevealParticles.GetComponent<RectTransform>();
                    if (rectTransform != null)
                    {
                        rectTransform.anchoredPosition = Vector2.zero;
                        rectTransform.localPosition = new Vector3(0, 0, -10);
                    }
                    
                    var canvas = prizeRevealParticles.GetComponent<Canvas>();
                    if (canvas == null)
                    {
                        canvas = prizeRevealParticles.gameObject.AddComponent<Canvas>();
                    }
                    canvas.overrideSorting = true;
                    canvas.sortingOrder = 999;
                    
                    var renderer = prizeRevealParticles.GetComponent<ParticleSystemRenderer>();
                    if (renderer != null)
                    {
                        renderer.sortingOrder = 999;
                    }
                }
            }
        }

        public void Initialize(Sprite prizeSprite)
        {
            _assignedPrize = prizeSprite;
            prizeImage.sprite = prizeSprite;
            CreateScratchTexture();
            PlaySpawnAnimation();
        }
        
        public void SetSpawnDelay(float delay)
        {
            spawnDelay = delay;
        }
        
        public void PlaySpawnAnimation()
        {
            var rectTransform = transform as RectTransform;
            if (rectTransform == null) return;
            
            rectTransform.localScale = Vector3.zero;
            rectTransform.localRotation = Quaternion.Euler(0f, spawnRotation, 0f);
            
            var spawnSequence = DOTween.Sequence();
            spawnSequence.AppendInterval(spawnDelay);
            spawnSequence.Append(rectTransform.DOScale(Vector3.one, spawnDuration).SetEase(Ease.OutBack));
            spawnSequence.Join(rectTransform.DOLocalRotate(Vector3.zero, spawnDuration).SetEase(Ease.OutQuad));
        }

        public void SetLocked(bool locked)
        {
            _isLocked = locked;
        }

        public void SetActive(bool active)
        {
            if (activeBorder != null)
            {
                activeBorder.SetActive(active);
            }
        }

        private void CreateScratchTexture()
        {
            _scratchTexture = new Texture2D(textureWidth, textureHeight);
            _pixels = new Color[textureWidth * textureHeight];

            for (int i = 0; i < _pixels.Length; i++)
            {
                _pixels[i] = Color.white;
            }
            
            _scratchTexture.SetPixels(_pixels);
            _scratchTexture.Apply();
            
            scratchLayer.texture = _scratchTexture;
            _totalPixels = _pixels.Length;
            _scratchedPixels = 0;
            _isScratched = false;
            _hasUnlocked = false;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (_isLocked || _isScratched)
            {
                return;
            }

            if (ScratchPercentage == 0)
            {
                OnCardStarted?.Invoke(this);
            }
            
            Scratch(eventData.position);
        }
        
        public void OnDrag(PointerEventData eventData)
        {
            if (_isLocked || _isScratched)
                return;
                
            Scratch(eventData.position);
        }

        private void Scratch(Vector2 screenPosition)
        {
            if (_isScratched)
                return;
            
            RectTransformUtility.ScreenPointToLocalPointInRectangle(scratchLayer.rectTransform, screenPosition, null, out var localPoint);
            
            _soundManager.PlaySfx(scratchSound, 0.5f);

            var normalizedPoint = new Vector2(
                (localPoint.x / scratchLayer.rectTransform.rect.width) + 0.5f,
                (localPoint.y / scratchLayer.rectTransform.rect.height) + 0.5f);
            
            var pixelX = (int)(normalizedPoint.x * textureWidth);
            var pixelY = (int)(normalizedPoint.y * textureHeight);
            
            ScratchAtPosition(pixelX, pixelY);
        }

        private void ScratchAtPosition(int centerX, int centerY)
        {
            for (int x = -brushSize; x <= brushSize; x++)
            {
                for (int y = -brushSize; y <= brushSize; y++)
                {
                    if (x * x + y * y <= brushSize * brushSize)
                    {
                        var pixelX = centerX + x;
                        var pixelY = centerY + y;

                        if (pixelX >= 0 && pixelX < textureWidth && pixelY >= 0 && pixelY < textureHeight)
                        {
                            var index = pixelY * textureWidth + pixelX;
                        
                            if (_pixels[index].a > 0)
                            {
                                _pixels[index] = Color.clear;
                                _scratchedPixels++;
                            }
                        }
                    }
                }
            }
            
            _scratchTexture.SetPixels(_pixels);
            _scratchTexture.Apply();

            CheckRevealProgress();
        }

        private void CheckRevealProgress()
        {
            var scratchPercentage = ScratchPercentage;
            
            UpdateProgressRing(scratchPercentage);

            if (scratchPercentage >= unlockThreshold && !_hasUnlocked)
            {
                _hasUnlocked = true;
                SetActive(false);
                OnCardUnlocked?.Invoke(this);
            }

            if (scratchPercentage >= revealThreshold && !_isScratched)
            {
                RevealPrize();
            }
        }
        
        private void UpdateProgressRing(float progress)
        {
            if (progressRing == null || !showProgressRing) return;
            
            if (progress > 0f && !progressRing.gameObject.activeSelf)
            {
                progressRing.gameObject.SetActive(true);
            }
            
            progressRing.DOFillAmount(progress, 0.2f).SetEase(Ease.OutQuad);
            
            var targetColor = Color.Lerp(progressColorStart, progressColorEnd, progress);
            progressRing.DOColor(targetColor, 0.2f);
        }

        private void RevealPrize()
        {
            _isScratched = true;
            scratchLayer.gameObject.SetActive(false);
            
            if (progressRing != null)
            {
                progressRing.gameObject.SetActive(false);
            }
            
            SetActive(false);
            PlayPrizeRevealAnimation();
            
            OnPrizeRevealed?.Invoke(_assignedPrize);
        }

        private void PlayPrizeRevealAnimation()
        {
            var revealSequence = DOTween.Sequence();
            
            if (prizeFlashOverlay != null)
            {
                revealSequence.Append(prizeFlashOverlay.DOFade(0.8f, 0.15f).SetEase(Ease.OutQuad));
                revealSequence.Append(prizeFlashOverlay.DOFade(0f, 0.35f).SetEase(Ease.InQuad));
            }
            
            if (prizeImage != null && prizeImage.rectTransform != null)
            {
                revealSequence.Join(prizeImage.rectTransform.DOScale(prizeScaleAmount, prizeScaleDuration * 0.5f).SetEase(Ease.OutBack));
                revealSequence.Append(prizeImage.rectTransform.DOScale(1f, prizeScaleDuration * 0.5f).SetEase(Ease.InOutQuad));
                
                revealSequence.Join(prizeImage.rectTransform.DOShakePosition(shakeDuration, shakeStrength, shakeVibrato, shakeRandomness, false, true).SetEase(Ease.OutQuad));
            }
            
            if (prizeRevealParticles != null)
            {
                prizeRevealParticles.Clear();
                prizeRevealParticles.Play();
            }
        }

        private void OnDestroy()
        {
            if (_scratchTexture != null)
                Destroy(_scratchTexture);
        }
    }
}
