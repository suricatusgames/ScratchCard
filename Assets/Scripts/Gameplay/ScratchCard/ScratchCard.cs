using System;
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

        [Header("Settings")]
        [SerializeField] private int textureWidth = 512;
        [SerializeField] private int textureHeight = 512;
        [SerializeField] private int brushSize = 30;
        [SerializeField] private float unlockThreshold = 0.5f;
        [SerializeField] private float revealThreshold = 0.75f;
        
        [Header("Border Settings")]
        [SerializeField] private Color borderColor = Color.yellow;
        [SerializeField] private float borderWidth = 10f;
        
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
        }

        public void Initialize(Sprite prizeSprite)
        {
            _assignedPrize = prizeSprite;
            prizeImage.sprite = prizeSprite;
            CreateScratchTexture();
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

        private void RevealPrize()
        {
            _isScratched = true;
            scratchLayer.gameObject.SetActive(false);
            SetActive(false);
            OnPrizeRevealed?.Invoke(_assignedPrize);
        }

        private void OnDestroy()
        {
            if (_scratchTexture != null)
                Destroy(_scratchTexture);
        }
    }
}
