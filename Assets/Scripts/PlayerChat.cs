using Mirror;
using TMPro;

public class PlayerChat : NetworkBehaviour
{
    private TMP_InputField messageInput;

    private void Start()
    {
        if (isLocalPlayer)
        {
            messageInput = NetworkUIManager.instance.messageInput;
            messageInput.onSubmit.AddListener(delegate { SendMessage(); });
        }
    }

    private void SendMessage()
    {
        if (!isLocalPlayer) return;

        string message = messageInput.text;
        
        ChatMessage chatMessage = new ChatMessage
        {
            UserID = NetworkClient.connection.identity.GetComponent<Player>().userID,
            Message = message
        };

        NetworkClient.Send(chatMessage);
        messageInput.text = string.Empty;
    }
}