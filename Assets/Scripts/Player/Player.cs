using Mirror;

public class Player : NetworkBehaviour
{
    [SyncVar] public string userID;
    [SyncVar] public int clientId;
    
    public override void OnStartLocalPlayer()
    {
        userID = NetworkUIManager.instance.userIDInput.text;
    }
}

public struct ChatMessage : NetworkMessage
{
    public string UserID;
    public string Message;
}