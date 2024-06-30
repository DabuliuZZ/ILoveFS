using UnityEngine;
using Mirror;
using TMPro;

public class PlayerChat : NetworkBehaviour
{
    private TMP_InputField messageInput;
    private TextMeshProUGUI chatDisplay;

    private void Start()
    {
        if (isLocalPlayer)
        {
            messageInput = NetworkManagerUI.instance.messageInput;
            chatDisplay = NetworkManagerUI.instance.chatDisplay;
            messageInput.onSubmit.AddListener(delegate { SendMessage(); });
        }
    }

    private void SendMessage()
    {
        if (!isLocalPlayer) return;

        string message = messageInput.text;
        if (!string.IsNullOrEmpty(message))
        {
            LogStatus("<color=yellow>Sending message...</color>");

            ChatMessage chatMessage = new ChatMessage
            {
                userID = NetworkClient.connection.identity.GetComponent<Player>().userID,
                message = message
            };

            NetworkClient.Send(chatMessage);

            LogStatus("<color=yellow>Message sent successfully.</color>");
            messageInput.text = string.Empty;
        }
    }

    private void LogStatus(string message)
    {
        Debug.Log(message);
        if (chatDisplay != null)
        {
            chatDisplay.text += message + "\n";
        }
    }
}