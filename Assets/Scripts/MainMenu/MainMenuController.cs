using TMPro;
using Tools.B2B.PlayerRegistration.Models;
using Tools.B2B.PlayerRegistration.Services;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Utils;

namespace MainMenu
{
    public class MainMenuController : MonoBehaviour
    {
        [Header("Input Fields")]
        [SerializeField] private TMP_InputField nameInputField;
        [SerializeField] private TMP_InputField emailInputField;
        [SerializeField] private TMP_InputField cellphoneInputField;

        [Header("UI Elements")]
        [SerializeField] private Button startButton;
        [SerializeField] private TMP_Text errorMessageText;

        [Header("Keyboard Avoidance")]
        [SerializeField] private KeyboardAvoidance keyboardAvoidance;

        [Header("Scene Settings")]
        [SerializeField] private string gameplaySceneName = "GameplayScene";

        private const string ErrorMessageFillFields = "Por favor, preencha todos os campos.";
        private const string ErrorMessageInvalidEmail = "Por favor, insira um email válido com @.";
        private const string ErrorMessageInvalidPhone = "O telefone deve conter apenas números.";
        private IPlayerRegistrationService _playerRegistrationService;

        private GoogleSheetsService _googleSheetsService;

        private void Start()
        {
            SetupGoogleSheetsService();
            SetupPlayerRegistrationService();
            SetupInputFields();
            SetupKeyboardAvoidance();
        
            if (startButton != null)
            {
                startButton.onClick.AddListener(OnStartButtonClicked);
            }

            HideErrorMessage();
        }

        private void SetupGoogleSheetsService()
        {
            _googleSheetsService = FindObjectOfType<GoogleSheetsService>();
        
            if (_googleSheetsService == null)
            {
                var serviceObj = new GameObject("GoogleSheetsService");
                _googleSheetsService = serviceObj.AddComponent<GoogleSheetsService>();
                DontDestroyOnLoad(serviceObj);
            }
        }

        private void SetupPlayerRegistrationService()
        {
            _playerRegistrationService = new PlayerRegistrationService();
        }

        private void SetupInputFields()
        {
            if (cellphoneInputField != null)
            {
                cellphoneInputField.contentType = TMP_InputField.ContentType.IntegerNumber;
                cellphoneInputField.characterValidation = TMP_InputField.CharacterValidation.Integer;
            }

            if (emailInputField != null)
            {
                emailInputField.contentType = TMP_InputField.ContentType.EmailAddress;
            }
        }

        private void SetupKeyboardAvoidance()
        {
            if (keyboardAvoidance != null)
            {
                if (nameInputField != null)
                    keyboardAvoidance.RegisterInputField(nameInputField);
            
                if (emailInputField != null)
                    keyboardAvoidance.RegisterInputField(emailInputField);
            
                if (cellphoneInputField != null)
                    keyboardAvoidance.RegisterInputField(cellphoneInputField);
            }
        }

        private async void OnStartButtonClicked()
        {
            var playerName = nameInputField.text;
            var playerEmail = emailInputField.text;
            var playerCellphone = cellphoneInputField.text;

            var result = await _playerRegistrationService.RegisterPlayerAsync(
                playerName, 
                playerEmail,
                playerCellphone,
                consent: true);

            if (result.Success)
            {
                HideErrorMessage();
                SavePlayerData(result.PlayerData);
                LoadGameplayScene();
            }
            else
            {
                ShowErrorMessage(result.ErrorMessage);
            }
        }

        private void ShowErrorMessage(string message)
        {
            if (errorMessageText != null)
            {
                errorMessageText.text = message;
                errorMessageText.gameObject.SetActive(true);
            }
        }

        private void HideErrorMessage()
        {
            if (errorMessageText != null)
                errorMessageText.gameObject.SetActive(false);
            
        }

        private void SavePlayerData(PlayerRegistrationData playerData)
        {
            PlayerPrefs.SetString("PlayerName", playerData.Name);
            PlayerPrefs.SetString("PlayerEmail", playerData.Email);
            PlayerPrefs.SetString("PlayerCellphone", playerData.PhoneNumber);
            PlayerPrefs.Save();
        }

        private void LoadGameplayScene()
        {
            SceneManager.LoadScene(gameplaySceneName);
        }

        private void OnDestroy()
        {
            if (startButton != null)
            {
                startButton.onClick.RemoveListener(OnStartButtonClicked);
            }
        }
    }
}
