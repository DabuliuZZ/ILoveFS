using System;
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

    // 每个客户端在进入场景后独立调用一次该方法，
    // 如果当前客户端是服务端，那么把所有 Player 的当前所需脚本都打开，
    // 如果当前客户端不是服务端，那么只把自己的 Player 的当前所需脚本打开。
    public override void OnStartClient() 
    {
        CustomNetworkManager.instance.AddComponentsForPlayer(isServer,typeof(PlayerButton));
    }
    
    private void OffServerButton()
    {
        buttonSet1.SetActive(false);
        buttonSet2.SetActive(false);
    }
}