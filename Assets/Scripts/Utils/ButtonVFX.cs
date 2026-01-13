using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Utils
{
    [RequireComponent(typeof(Button))]
    public class ButtonVFX : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        [Header("Scale Animation")]
        [SerializeField] private bool enableScaleAnimation = true;
        [SerializeField] private float hoverScale = 1.1f;
        [SerializeField] private float pressScale = 0.95f;
        [SerializeField] private float scaleDuration = 0.2f;
        
        [Header("Pulse Animation")]
        [SerializeField] private bool enablePulse = false;
        [SerializeField] private float pulseScale = 1.05f;
        [SerializeField] private float pulseDuration = 1f;
        
        [Header("Color Animation")]
        [SerializeField] private bool enableColorAnimation = false;
        [SerializeField] private Color hoverColor = Color.white;
        [SerializeField] private Color pressColor = Color.gray;
        [SerializeField] private float colorDuration = 0.2f;
        
        [Header("Rotation Animation")]
        [SerializeField] private bool enableRotationOnClick = false;
        [SerializeField] private float rotationAmount = 10f;
        [SerializeField] private int rotationVibrato = 5;
        
        [Header("Particle Effects")]
        [SerializeField] private bool enableClickParticles = false;
        [SerializeField] private ParticleSystem clickParticles;
        
        private Button _button;
        private Image _buttonImage;
        private RectTransform _rectTransform;
        private Vector3 _originalScale;
        private Color _originalColor;
        private Tween _pulseTween;
        private bool _isHovering;
        private bool _isPressed;

        private void Awake()
        {
            _button = GetComponent<Button>();
            _buttonImage = GetComponent<Image>();
            _rectTransform = GetComponent<RectTransform>();
            _originalScale = _rectTransform.localScale;
            
            if (_buttonImage != null)
                _originalColor = _buttonImage.color;
        }

        private void Start()
        {
            if (enablePulse && _button.interactable)
            {
                StartPulse();
            }
        }

        private void OnEnable()
        {
            if (enablePulse && _button != null && _button.interactable)
            {
                StartPulse();
            }
        }

        private void OnDisable()
        {
            StopPulse();
            ResetButton();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!_button.interactable) return;
            
            _isHovering = true;
            StopPulse();
            
            if (enableScaleAnimation)
            {
                _rectTransform.DOScale(_originalScale * hoverScale, scaleDuration)
                    .SetEase(Ease.OutBack);
            }
            
            if (enableColorAnimation && _buttonImage != null)
            {
                _buttonImage.DOColor(hoverColor, colorDuration);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!_button.interactable) return;
            
            _isHovering = false;
            
            if (!_isPressed)
            {
                ResetButton();
                
                if (enablePulse)
                {
                    StartPulse();
                }
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!_button.interactable) return;
            
            _isPressed = true;
            
            if (enableScaleAnimation)
            {
                _rectTransform.DOScale(_originalScale * pressScale, scaleDuration * 0.5f)
                    .SetEase(Ease.OutQuad);
            }
            
            if (enableColorAnimation && _buttonImage != null)
            {
                _buttonImage.DOColor(pressColor, colorDuration * 0.5f);
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!_button.interactable) return;
            
            _isPressed = false;
            
            if (enableRotationOnClick)
            {
                _rectTransform.DOPunchRotation(
                    new Vector3(0, 0, rotationAmount), 
                    scaleDuration * 2f, 
                    rotationVibrato, 
                    0.5f
                );
            }
            
            if (enableClickParticles && clickParticles != null)
            {
                clickParticles.Clear();
                clickParticles.Play();
            }
            
            if (_isHovering)
            {
                if (enableScaleAnimation)
                {
                    _rectTransform.DOScale(_originalScale * hoverScale, scaleDuration)
                        .SetEase(Ease.OutBack);
                }
                
                if (enableColorAnimation && _buttonImage != null)
                {
                    _buttonImage.DOColor(hoverColor, colorDuration);
                }
            }
            else
            {
                ResetButton();
                
                if (enablePulse)
                {
                    StartPulse();
                }
            }
        }

        private void StartPulse()
        {
            StopPulse();
            
            _pulseTween = _rectTransform.DOScale(_originalScale * pulseScale, pulseDuration)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
        }

        private void StopPulse()
        {
            if (_pulseTween != null && _pulseTween.IsActive())
            {
                _pulseTween.Kill();
                _pulseTween = null;
            }
        }

        private void ResetButton()
        {
            if (enableScaleAnimation)
            {
                _rectTransform.DOScale(_originalScale, scaleDuration)
                    .SetEase(Ease.OutQuad);
            }
            
            if (enableColorAnimation && _buttonImage != null)
            {
                _buttonImage.DOColor(_originalColor, colorDuration);
            }
        }

        private void OnDestroy()
        {
            StopPulse();
            _rectTransform.DOKill();
            
            if (_buttonImage != null)
                _buttonImage.DOKill();
        }
    }
}
