using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectionSingleton : MonoBehaviour
{
    
    private static CharacterSelectionSingleton instance;
    public static CharacterSelectionSingleton Instance { get=>instance; }
    public GameObject buttonSet1; // 客户端1的按钮组
    public GameObject buttonSet2; // 客户端2的按钮组

    public Image character1;
    public Image character2;
    public Sprite[] characterSkins;
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this; 
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
