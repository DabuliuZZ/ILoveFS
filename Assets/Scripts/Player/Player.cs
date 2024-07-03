using System;
using Mirror;

public class Player : NetworkBehaviour
{
    [SyncVar] public string userID;
    [SyncVar] public int clientId;

    private void Start()
    {
        DontDestroyOnLoad(this);
    }

    public override void OnStartLocalPlayer()
    {
        userID = NetworkUIManager.instance.userIDInput.text;
    }
    
    // public void EnableComponent<T>() where T: NetworkBehaviour
    // {
    //     GetComponent<T>().enabled = true;
    // }
}

public struct ChatMessage : NetworkMessage
{
    public string UserID;
    public string Message;
}