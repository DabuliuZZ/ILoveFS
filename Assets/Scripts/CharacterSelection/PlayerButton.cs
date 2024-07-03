using System;
using System.Collections;
using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class PlayerButton : NetworkBehaviour
{
    [SerializeField] private GameObject buttonSet1; // 客户端1的按钮组
    [SerializeField] private GameObject buttonSet2; // 客户端2的按钮组
    private Button switchButton;
    private Button confirmButton;
    private Image currentCharacter;
    private Image characterImage1;
    private Image characterImage2;
    private Sprite[] characterSkins;
    private int currentSpriteIndex;

    private void OnEnable()
    {
        StartCoroutine(GetButtonAndCharacterAsync());
    }

    IEnumerator GetButtonAndCharacterAsync()
    {
        yield return null; //延迟一帧执行
        buttonSet1 = CharacterSelectionSingleton.Instance.buttonSet1;
        buttonSet2 = CharacterSelectionSingleton.Instance.buttonSet2;
        buttonSet1.SetActive(false);
        buttonSet2.SetActive(false);
        
        characterImage1 = CharacterSelectionSingleton.Instance.character1;
        characterImage2 = CharacterSelectionSingleton.Instance.character2;
        
        characterSkins = CharacterSelectionSingleton.Instance.characterSkins;
        
        var clientId = GetComponent<Player>().clientId;
        
        if (clientId == 1)
        {
            buttonSet1.SetActive(true);
            GetButtonAndCharacter(buttonSet1.transform);
            currentCharacter = characterImage1;
        }
        if (clientId == 2)
        {
            buttonSet2.SetActive(true);
            GetButtonAndCharacter(buttonSet2.transform);
            currentCharacter = characterImage2;
        }
    }

    private void GetButtonAndCharacter(Transform buttonSet)
    {
        foreach (Transform button in buttonSet)
        {
            if (button.name == "SwitchButton") { switchButton = button.GetComponent<Button>(); }
            if (button.name == "ConfirmButton") { confirmButton = button.GetComponent<Button>(); }
        }
        switchButton.onClick.AddListener(SwitchSkin);
        confirmButton.onClick.AddListener(ConfirmSkin);
    }

    private void SwitchSkin()
    {
        currentSpriteIndex = (currentSpriteIndex + 1) % characterSkins.Length;
        currentCharacter.sprite = characterSkins[currentSpriteIndex];

        var clientId = GetComponent<Player>().clientId;
        
        CmdSendSpriteChange(currentSpriteIndex,clientId);
    }
    
    [Command] public void CmdSendSpriteChange(int newSpriteIndex,int clientId)
    {
        RpcUpdateSprite(newSpriteIndex,clientId);
    }
    
    [ClientRpc] public void RpcUpdateSprite(int newSpriteIndex,int clientId)
    {
        if (clientId == 1)
        {
            characterImage1.sprite = characterSkins[newSpriteIndex];
        }
        if (clientId == 2)
        {
            characterImage2.sprite = characterSkins[newSpriteIndex];
        }
    }
    
    private void ConfirmSkin()
    {
        Debug.Log("Confirmed");
    }
}