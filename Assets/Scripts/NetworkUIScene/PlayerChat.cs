using System;
using Mirror;
using TMPro;

public class PlayerChat : NetworkBehaviour
{
    private TMP_InputField messageInput;
    private TextMeshProUGUI chatLog;

    private void Start()
    {
        chatLog = NetworkUIManager.instance.Log;
        messageInput = NetworkUIManager.instance.messageInput;
        
        var player = GetComponent<Player>();
        if(player.isLocalPlayer)
        {
            messageInput.onSubmit.AddListener(delegate { SendMessage(); });
        }            
    }

    private void SendMessage()
    {
        string message = messageInput.text;
        string userID = GetComponent<Player>().userID;
        
        CmdSendMessage(userID,message);
        
        messageInput.text = string.Empty;
    }
    
    [Command] public void CmdSendMessage(string userID,string message)
    {
        RpcDisplayMessage(userID,message);
    }
    
    [ClientRpc] public void RpcDisplayMessage(string userID, string message)
    {
        chatLog.text += "<color=blue>" + userID + ":</color> <color=white>" + message + "</color>" + "\n";
    }
}