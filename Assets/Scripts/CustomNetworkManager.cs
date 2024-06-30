using UnityEngine;
using Mirror;
using TMPro;

public class CustomNetworkManager : NetworkManager
{
    public TextMeshProUGUI statusLog; // 用于显示状态信息的UI元素
    private int playerCount;

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
        Transform startPos = GetStartPosition();
        GameObject player = startPos != null
            ? Instantiate(playerPrefab, startPos.position, startPos.rotation)
            : Instantiate(playerPrefab);

        NetworkServer.AddPlayerForConnection(conn, player);
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
}
