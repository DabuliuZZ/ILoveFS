using System;
using Mirror;

public class Player : NetworkBehaviour
{
    [SyncVar] public string userID;
    [SyncVar] public int clientId;
    [SyncVar] public int skinIndex;

    private void Start()
    {
        DontDestroyOnLoad(this);
    }

    public override void OnStartLocalPlayer()
    {
        userID = NetworkUIManager.instance.userIDInput.text;
    }
}