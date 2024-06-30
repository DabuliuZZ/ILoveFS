using UnityEngine;
using Mirror;
using TMPro;

public class CustomNetworkManager : NetworkManager
{
    public TextMeshProUGUI statusDisplay; // 用于显示状态信息的UI元素
    private int playerCount = 0;

    public override void OnStartServer()
    {
        base.OnStartServer();
        LogStatus("<color=yellow>Server started.</color>");
        if (playerPrefab != null)
        {
            NetworkClient.RegisterPrefab(playerPrefab);
        }
        else
        {
            Debug.LogError("Player Prefab is not assigned in the NetworkManager.");
        }
    }

    public override void OnServerConnect(NetworkConnectionToClient conn)
    {
        base.OnServerConnect(conn);
        LogStatus($"<color=yellow>Player connected: {conn.connectionId}</color>");
    }

    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        LogStatus($"<color=yellow>Player disconnected: {conn.connectionId}</color>");
        playerCount--;
        LogStatus($"<color=yellow>Current player count: {playerCount}</color>");
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
        LogStatus($"<color=yellow>Player added: {conn.connectionId}</color>");
        LogStatus($"<color=yellow>Current player count: {playerCount}</color>");
    }

    public override void OnClientConnect()
    {
        base.OnClientConnect();
        LogStatus("<color=yellow>Connected to server.</color>");
    }

    public override void OnClientDisconnect()
    {
        LogStatus("<color=yellow>Disconnected from server.</color>");
        base.OnClientDisconnect();
    }

    private void LogStatus(string message)
    {
        Debug.Log(message);
        if (statusDisplay != null)
        {
            statusDisplay.text += message + "\n";
        }
    }
}
