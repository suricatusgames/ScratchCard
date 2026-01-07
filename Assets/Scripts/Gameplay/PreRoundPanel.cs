using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay
{
    public class PreRoundPanel : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI scratchQuantityText;
        [SerializeField] private TextMeshProUGUI howToPlayText;
        [SerializeField] private Button startButton;
        [SerializeField] private GameObject panelContainer;
        
        public event Action OnStartButtonClicked;

        private void Start()
        {
            if (startButton != null)
                startButton.onClick.AddListener(HandleStartButtonClick);
        }

        public void Show()
        {
            if(panelContainer != null)
                panelContainer.SetActive(true);
        }

        private void Hide()
        {
            if (panelContainer != null)
                panelContainer.SetActive(false);
        }
        
        public void SetScratchQuantity(int quantity)
        {
            if (scratchQuantityText != null)
                scratchQuantityText.text = $"Você tem {quantity} raspadinhas disponíveis nessa rodada!";
        }

        public void HandleStartButtonClick()
        {
            OnStartButtonClicked?.Invoke();
            Hide();
        }

        private void OnDestroy()
        {
            if (startButton != null)
                startButton.onClick.RemoveListener(HandleStartButtonClick);
        }
    }
}