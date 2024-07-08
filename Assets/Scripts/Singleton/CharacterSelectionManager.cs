using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class CharacterSelectionManager : NetworkBehaviour
{
    public static CharacterSelectionManager Instance;
    
    public GameObject serverControlButtons; // 服务器端的控制按钮组
    public GameObject buttonSet1; // 客户端1的按钮组
    public GameObject buttonSet2; // 客户端2的按钮组
    public Image character1;
    public Image character2;
    public Sprite[] characterSkins;
    public List<int> confirmedSkins = new List<int>();
    public int confirmCount;
    public TextMeshProUGUI playerLog;

    [SerializeField] private Button enterGameButton;
    [SerializeField] private TextMeshProUGUI serverLog;
    [SerializeField] private int maxPlayerCount;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this; 
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public override void OnStartServer()
    {
        serverControlButtons.SetActive(true); // 服务器端激活控制按钮
    }
    
    public override void OnStartClient() // 每个客户端在进入场景后独立调用一次该方法，类似于 Start，本地调用，本地执行
    {
        CustomNetworkManager.instance.AddComponentsForAllPlayers(typeof(PlayerButton));
    }

    public void OnPlayerConfirmed(int clientId)
    {
        confirmCount++;
        serverLog.text += "Player " + clientId + " Confirmed." + "\n";
        
        if (confirmCount >= maxPlayerCount) 
        {
            enterGameButton.interactable = true; 
        }
    }
}
