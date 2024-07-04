using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class CharacterSelectionSingleton : NetworkBehaviour
{
    public static CharacterSelectionSingleton Instance;
    
    public GameObject serverControlButtons; // 服务器端的控制按钮组
    public GameObject buttonSet1; // 客户端1的按钮组
    public GameObject buttonSet2; // 客户端2的按钮组
    
    public Image character1;
    public Image character2;
    public Sprite[] characterSkins;
    public List<int> confirmedSkins = new List<int>();
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this; 
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
