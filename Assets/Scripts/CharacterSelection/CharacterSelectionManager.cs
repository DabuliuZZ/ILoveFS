using System;
using Mirror;
using UnityEngine;

public class CharacterSelectionManager : NetworkBehaviour
{
    public override void OnStartServer()
    {
        CharacterSelectionSingleton.Instance.serverControlButtons.SetActive(true); // 服务器端激活控制按钮
    }
    
    public override void OnStartClient() // 每个客户端在进入场景后独立调用一次该方法，类似于 Start，本地调用，本地执行
    {
        CustomNetworkManager.instance.AddComponentsForAllPlayers(typeof(PlayerButton));
    }
}