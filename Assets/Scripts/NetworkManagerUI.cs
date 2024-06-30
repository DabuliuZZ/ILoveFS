using UnityEngine;
using UnityEngine.UI;
using Mirror;
using TMPro;
using UnityEngine.Serialization;

public class NetworkManagerUI : MonoBehaviour
{
    public static NetworkManagerUI instance;

    public TMP_InputField userIDInput;
    public Toggle isHostToggle;
    public TMP_InputField ipInput;
    public Button startButton;
    public GameObject inputPanel;
    public GameObject chatPanel;
    public TMP_InputField messageInput;
    public TextMeshProUGUI chatDisplay;
    public TextMeshProUGUI statusDisplay; // 用于显示状态信息的UI元素

    private CustomNetworkManager customNetworkManager;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        // 获取自定义的 NetworkManager
        customNetworkManager = FindObjectOfType<CustomNetworkManager>();
        if (customNetworkManager == null)
        {
            Debug.LogError("CustomNetworkManager not found in the scene.");
        }
        else
        {
            customNetworkManager.statusDisplay = statusDisplay; // 设置状态显示
        }
    }

    private void Start()
    {
        if (userIDInput == null || isHostToggle == null || ipInput == null ||
            startButton == null || inputPanel == null || chatPanel == null ||
            messageInput == null || chatDisplay == null || statusDisplay == null)
        {
            Debug.LogError("One or more UI elements are not assigned in the Inspector.");
            return;
        }

        startButton.onClick.AddListener(StartGame);
        isHostToggle.onValueChanged.AddListener(delegate { ToggleIPInput(isHostToggle); });
        ipInput.gameObject.SetActive(false);
        chatPanel.SetActive(false);
    }

    private void ToggleIPInput(Toggle toggle)
    {
        ipInput.gameObject.SetActive(!toggle.isOn);
    }

    private void StartGame()
    {
        Debug.Log("Starting game...");
        string userID = userIDInput.text;
        Debug.Log("Player ID: " + userID);

        if (string.IsNullOrEmpty(userID))
        {
            Debug.LogError("Player ID cannot be empty.");
            return;
        }

        if (isHostToggle.isOn)
        {
            Debug.Log("Starting as host...");
            customNetworkManager.StartHost();
        }
        else
        {
            string ipAddress = ipInput.text;
            Debug.Log("IP Address: " + ipAddress);

            if (string.IsNullOrEmpty(ipAddress))
            {
                Debug.LogError("IP Address cannot be empty when not hosting.");
                return;
            }
            customNetworkManager.networkAddress = ipAddress;
            customNetworkManager.StartClient();
        }

        Debug.Log("Switching panels...");
        inputPanel.SetActive(false);
        chatPanel.SetActive(true);
    }
}
