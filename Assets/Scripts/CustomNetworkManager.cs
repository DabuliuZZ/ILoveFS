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
        statusLog.text += "<color=yellow>Server started.</color>" + "\n";
        
        NetworkClient.RegisterPrefab(playerPrefab);
    }

    public override void OnServerConnect(NetworkConnectionToClient conn)
    {
        base.OnServerConnect(conn);
        statusLog.text += $"<color=yellow>Player connected: {conn.connectionId}</color>" + "\n";
    }

    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        statusLog.text += $"<color=yellow>Player disconnected: {conn.connectionId}</color>" + "\n";
        playerCount--;
        statusLog.text += $"<color=yellow>Current player count: {playerCount}</color>" + "\n";
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
        statusLog.text += $"<color=yellow>Player added: {conn.connectionId}</color>" + "\n";
        statusLog.text += $"<color=yellow>Current player count: {playerCount}</color>" + "\n";
    }

    public override void OnClientConnect()
    {
        base.OnClientConnect();
        statusLog.text += "<color=yellow>Connected to server.</color>" + "\n";
    }

    public override void OnClientDisconnect()
    {
        statusLog.text += "<color=yellow>Disconnected from server.</color>" + "\n";
        base.OnClientDisconnect();
    }
}
