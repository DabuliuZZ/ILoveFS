using Mirror;
using UnityEngine;

public class CharacterSelectionManager : NetworkBehaviour
{
    [SerializeField] private GameObject serverControlButtons; // 服务器端的控制按钮组
    [SerializeField] private GameObject buttonSet1; // 客户端1的按钮组
    [SerializeField] private GameObject buttonSet2; // 客户端2的按钮组
    
    public override void OnStartServer()
    {
        serverControlButtons.SetActive(true); // 服务器端激活控制按钮
        Invoke("OffServerButton",1.5f);
    }

    public override void OnStartClient()
    {
        if (isServer) return;
        CustomNetworkManager.instance.AddComponentsForPlayer(typeof(PlayerButton));
    }
    
    private void OffServerButton()
    {
        buttonSet1.SetActive(false);
        buttonSet2.SetActive(false);
    }
}