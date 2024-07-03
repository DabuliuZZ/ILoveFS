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
        // 如果当前客户端是服务端，那么把所有 Player 的当前所需脚本都打开
        CustomNetworkManager.instance.AddComponentsForAllPlayers(typeof(PlayerButton));
        
        serverControlButtons.SetActive(true); // 服务器端激活控制按钮
        Invoke("OffServerButton",1f);
    }
    
    public override void OnStartClient() // 每个客户端在进入场景后独立调用一次该方法，类似于 Start，本地调用，本地执行
    {
        if (isServer) return; // 如果当前客户端不是服务端，那么只把当前客户端所对应的 Player 的当前所需脚本打开。
        CustomNetworkManager.instance.AddComponentsForLocalPlayer(typeof(PlayerButton));
    }
    
    private void OffServerButton()
    {
        buttonSet1.SetActive(false);
        buttonSet2.SetActive(false);
    }
}