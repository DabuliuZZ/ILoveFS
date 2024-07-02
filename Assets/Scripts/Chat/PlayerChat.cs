using Mirror;
using TMPro;

public class PlayerChat : NetworkBehaviour
{
    private TMP_InputField messageInput;

    private void Start()
    {
        messageInput = NetworkUIManager.instance.messageInput;
        messageInput.onSubmit.AddListener(delegate { SendMessage(); });
    }

    private void SendMessage()
    {
        string message = messageInput.text;
        
        ChatMessage chatMessage = new ChatMessage
        {
            UserID = GetComponent<Player>().userID,
            Message = message
        };

        NetworkClient.Send(chatMessage);
        messageInput.text = string.Empty;
    }
}