using Mirror;

public class Player : NetworkBehaviour
{
    [SyncVar]
    public string userID;

    public override void OnStartLocalPlayer()
    {
        userID = NetworkManagerUI.instance.userIDInput.text;
    }
}
public struct ChatMessage : NetworkMessage
{
    public string userID;
    public string message;
}