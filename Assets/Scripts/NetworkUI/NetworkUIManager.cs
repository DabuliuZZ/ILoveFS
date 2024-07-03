using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;

public class NetworkUIManager : MonoBehaviour
{
    public static NetworkUIManager instance;

    public TMP_InputField userIDInput;
    [SerializeField] private Toggle isHostToggle;
    [SerializeField] private TMP_InputField ipInput;
    [SerializeField] private Button connectButton;
    [SerializeField] private GameObject connectPanel;
    [SerializeField] private GameObject afterConnectPanel;
    public TMP_InputField messageInput;
    [SerializeField] private TextMeshProUGUI statusLog; // 用于显示状态信息的UI元素
    [SerializeField] private Button startGameButton;
    [SerializeField] private ChatManager chatManager;
    [SerializeField] private CustomNetworkManager customNetworkManager;

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
        
        customNetworkManager.statusLog = statusLog; // 设置状态显示
    }

    private void Start()
    {
        connectButton.onClick.AddListener(Connect);
        isHostToggle.onValueChanged.AddListener(delegate { ToggleIPInput(isHostToggle); });
        startGameButton.onClick.AddListener(StartGame);
    }

    private void ToggleIPInput(Toggle toggle)
    {
        ipInput.gameObject.SetActive(!toggle.isOn);
    }

    private void Connect()
    {
        if (isHostToggle.isOn)
        {
            Debug.Log("Starting as host...");
            startGameButton.gameObject.SetActive(true);
            customNetworkManager.StartHost();
        }
        else
        {
            string ipAddress = ipInput.text;
            Debug.Log("IP Address: " + ipAddress);

            customNetworkManager.networkAddress = ipAddress;
            customNetworkManager.StartClient();
        }
        
        connectPanel.SetActive(false);
        afterConnectPanel.SetActive(true);
    }
    
    [Server] private void StartGame()
    {
        // 跳场景
        NetworkManager.singleton.ServerChangeScene("CharacterSelectionScene");
    }
}
