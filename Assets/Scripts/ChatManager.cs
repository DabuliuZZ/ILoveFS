using UnityEngine;
using Mirror;
using TMPro;

public class ChatManager : NetworkBehaviour
{
    public TextMeshProUGUI chatDisplay;

    public override void OnStartServer()
    {
        NetworkServer.RegisterHandler<ChatMessage>(OnChatMessageReceived);
    }

    [Server]
    private void OnChatMessageReceived(NetworkConnection conn, ChatMessage message)
    {
        RpcDisplayMessage(message.userID, message.message);
    }

    [ClientRpc]
    private void RpcDisplayMessage(string uesrID, string message)
    {
        if (chatDisplay == null)
        {
            chatDisplay = NetworkManagerUI.instance.chatDisplay;
        }
        chatDisplay.text += $"<color=blue>{uesrID}</color>: <color=white>{message}</color>\n";
    }
}