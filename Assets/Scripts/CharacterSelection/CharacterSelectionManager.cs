using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

[System.Serializable]
public class CharacterSeletionPlayerComponets
{
    public int clientId; // 组的键
    public CharacterSeletionComponets Componets; // 组内的元素列表
}

[System.Serializable]
public class CharacterSeletionComponets
{
    public Button switchButton;
    public Button confirmButton;
    public Image characterImage; 
    public Image avatarImage;
}

public class CharacterSelectionManager : NetworkBehaviour
{
    public static CharacterSelectionManager instance;
    
    // 使用SerializedField来公开组的列表
    [SerializeField] private List<CharacterSeletionPlayerComponets> playerComponets = new List<CharacterSeletionPlayerComponets>();

    // 创建一个字典来存储整数键与组的关联
    public Dictionary<int, CharacterSeletionPlayerComponets> playerComponetsDictionary = new Dictionary<int, CharacterSeletionPlayerComponets>();
    
    
    [SerializeField] private Button enterGameButton;
    [SerializeField] private TextMeshProUGUI serverLog;
    private int maxPlayerCount;
    
    public Sprite[] characterSkins;
    public Sprite[] avatarSkins;
    public List<int> confirmedSkins = new List<int>();
    public int confirmCount;
    public TextMeshProUGUI playerLog;

    
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
        
        // 将列表中的组添加到字典中
        foreach (var playerComponet in playerComponets)
        {
            playerComponetsDictionary.Add(playerComponet.clientId, playerComponet);
        }

        maxPlayerCount = playerComponetsDictionary.Count;
    }
    
    public override void OnStartServer()
    {
        serverLog.gameObject.SetActive(true); 
        enterGameButton.gameObject.SetActive(true);
        
        enterGameButton.onClick.AddListener(CodesignStart);
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
    
    [Server] private void CodesignStart()
    {
        // 跳场景
        NetworkManager.singleton.ServerChangeScene("CodesignScene");
        
        // 播下个场景的BGM
        // AudioManager.Instance.PlayBGM();
    }
}
