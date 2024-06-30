using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NetworkUIManager : MonoBehaviour
{
    public static NetworkUIManager instance;

    public TMP_InputField userIDInput;
    public Toggle isHostToggle;
    public TMP_InputField ipInput;
    public Button connectButton;
    public GameObject connectPanel;
    public GameObject afterConnectPanel;
    public TMP_InputField messageInput;
    public TextMeshProUGUI chatLog;
    public TextMeshProUGUI statusLog; // 用于显示状态信息的UI元素

    [SerializeField] private CustomNetworkManager customNetworkManager;

    private void Awake()
    {
        if (instance == null) { instance = this; }
        else { Destroy(gameObject); }
        
        customNetworkManager.statusLog = statusLog; // 设置状态显示
    }

    private void Start()
    {
        connectButton.onClick.AddListener(StartGame);
        isHostToggle.onValueChanged.AddListener(delegate { ToggleIPInput(isHostToggle); });
        ipInput.gameObject.SetActive(false);
        afterConnectPanel.SetActive(false);
    }

    private void ToggleIPInput(Toggle toggle)
    {
        ipInput.gameObject.SetActive(!toggle.isOn);
    }

    private void StartGame()
    {
        if (isHostToggle.isOn)
        {
            Debug.Log("Starting as host...");
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
}
