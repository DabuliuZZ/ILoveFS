using Mirror;
using TMPro;

public class ChatManager : NetworkBehaviour
{
    public TextMeshProUGUI chatLog;

    public override void OnStartServer()
    {
        NetworkServer.RegisterHandler<ChatMessage>(OnChatMessageReceived);
    }

    [Server]
    private void OnChatMessageReceived(NetworkConnection conn, ChatMessage message)
    {
        RpcDisplayMessage(message.UserID, message.Message);
    }

    [ClientRpc]
    private void RpcDisplayMessage(string userID, string message)
    {
        chatLog.text += "<color=blue>" + userID + ":</color> <color=white>" + message + "</color>" + "\n";
    }
}