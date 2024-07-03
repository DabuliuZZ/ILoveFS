using UnityEngine;
using Mirror;
using TMPro;
using System.Collections;

public class CustomNetworkManager : NetworkManager
{
    public static CustomNetworkManager instance;
    public TextMeshProUGUI statusLog; // 用于显示状态信息的UI元素
    private int playerCount;

    public override void Awake()
    {
        base.Awake();

        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public override void OnStartServer()
    {
        base.OnStartServer();
        statusLog.text += "Server started." + "\n";
        NetworkClient.RegisterPrefab(playerPrefab);
    }

    public override void OnServerConnect(NetworkConnectionToClient conn)
    {
        base.OnServerConnect(conn);
        statusLog.text += "Player connected" + conn.connectionId + "\n";
    }

    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        statusLog.text += "Player disconnected: " + conn.connectionId + "\n";
        playerCount--;
        statusLog.text += "Current player count: " + playerCount + "\n";
        base.OnServerDisconnect(conn);
    }

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        GameObject player = Instantiate(playerPrefab);
        NetworkServer.AddPlayerForConnection(conn, player);
        
        player.GetComponent<Player>().clientId = conn.connectionId;
        
        playerCount++;
        statusLog.text += "Player added: " + conn.connectionId + "\n";
        statusLog.text += "Current player count: " + playerCount + "\n";
    }

    public override void OnClientConnect()
    {
        base.OnClientConnect();
        statusLog.text += "Connected to server." + "\n";
    }

    public override void OnClientDisconnect()
    {
        statusLog.text += "Disconnected from server." + "\n";
        base.OnClientDisconnect();
    }
    
    
    
    
    
    // 服务端给所有Player添加脚本的方法
    public void AddComponentsForAllPlayers(params System.Type[] componentTypes)
    {
        StartCoroutine(AddComponentsForAllPlayersCoroutine(componentTypes));
    }
    private IEnumerator AddComponentsForAllPlayersCoroutine(System.Type[] componentTypes)
    {
        yield return new WaitForSeconds(0.5f);

        foreach (var player in FindObjectsOfType<Player>())
        {
            Debug.Log(player + " " + player.isLocalPlayer);
            foreach (var componentType in componentTypes)
            {
                if (player.gameObject.GetComponent(componentType) is NetworkBehaviour networkBehaviour )
                {
                    networkBehaviour.enabled = true;
                }
            }
        }
    }
    
    // 客户端给自己的Player添加脚本的方法
    public void AddComponentsForLocalPlayer(params System.Type[] componentTypes)
    {
        StartCoroutine(AddComponentsForLocalPlayerCoroutine(componentTypes));
    }
    private IEnumerator AddComponentsForLocalPlayerCoroutine(System.Type[] componentTypes)
    {
        yield return new WaitForSeconds(0.5f);

        foreach (var player in FindObjectsOfType<Player>())
        {
            if (player.isLocalPlayer) // 重点
            {
                Debug.Log(player + " " + player.isLocalPlayer);
                foreach (var componentType in componentTypes)
                {
                    if (player.gameObject.GetComponent(componentType) is NetworkBehaviour networkBehaviour )
                    {
                        networkBehaviour.enabled = true;
                    }
                }
            }
        }
    }
}
