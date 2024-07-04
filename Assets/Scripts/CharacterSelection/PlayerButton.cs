using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerButton : NetworkBehaviour
{
    private Player player;
    private int clientId;
    private CharacterSelectionSingleton characterSelectionSingleton;
    
    private GameObject buttonSet1; // 客户端1的按钮组
    private GameObject buttonSet2; // 客户端2的按钮组
    private Image characterImage1;
    private Image characterImage2;
    private Sprite[] characterSkins;
    private Button switchButton;
    private Button confirmButton;
    private TextMeshProUGUI playerLog;
    
    private Image currentCharacter;
    private int currentSpriteIndex;
    private List<int> confirmedSkins;
    
    private void OnEnable()
    {
        player = GetComponent<Player>();
        clientId = player.clientId;
        characterSelectionSingleton = CharacterSelectionSingleton.Instance;
        
        buttonSet1 = characterSelectionSingleton.buttonSet1;
        buttonSet2 = characterSelectionSingleton.buttonSet2;
        characterImage1 = characterSelectionSingleton.character1;
        characterImage2 = characterSelectionSingleton.character2;
        characterSkins = characterSelectionSingleton.characterSkins;
        confirmedSkins = characterSelectionSingleton.confirmedSkins;
        playerLog = characterSelectionSingleton.playerLog;
        
        if (player.isLocalPlayer)
        {
            if (clientId == 1)
            {
                buttonSet1.SetActive(true);
                currentCharacter = characterImage1;
                GetButtonAndCharacter(buttonSet1.transform);
            }
            if (clientId == 2)
            {
                buttonSet2.SetActive(true);
                currentCharacter = characterImage2;
                GetButtonAndCharacter(buttonSet2.transform);
            }
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
        switchButton.gameObject.SetActive(true);
        confirmButton.gameObject.SetActive(true);
    }

    private void SwitchSkin()
    {
        currentSpriteIndex = (currentSpriteIndex + 1) % characterSkins.Length;
        currentCharacter.sprite = characterSkins[currentSpriteIndex];
        
        CmdSendSpriteChange(currentSpriteIndex, clientId);
        
        foreach (var confirmedSkinIndex in confirmedSkins)
        {
            if (currentSpriteIndex == confirmedSkinIndex)
            {
                confirmButton.interactable = false;
                currentCharacter.color = Color.gray;
                return;
            }
        }
        confirmButton.interactable = true;
        currentCharacter.color = Color.white;
    }
    
    [Command] public void CmdSendSpriteChange(int newSpriteIndex, int clientId)
    {
        RpcUpdateSprite(newSpriteIndex, clientId);
    }
    
    [ClientRpc] public void RpcUpdateSprite(int newSpriteIndex, int clientId)
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
        foreach (var confirmedSkinIndex in confirmedSkins)
        {
            if (currentSpriteIndex == confirmedSkinIndex)
            {
                Debug.Log("该皮肤已被选择，换一个吧！");
                playerLog.text += "This skin has been chosen,change one!" + "\n";
                return;
            }
        }
        currentCharacter.color = Color.white;
        switchButton.interactable = false;
        confirmButton.interactable = false;
        CmdConfirmSkin(currentSpriteIndex,clientId);
    }

    [Command] public void CmdConfirmSkin(int confirmedSkinIndex,int clientId)
    {
        RpcConfirmSkin(confirmedSkinIndex);
        characterSelectionSingleton.OnPlayerConfirmed(clientId);
    }

    [ClientRpc] public void RpcConfirmSkin(int confirmedSkinIndex)
    { 
        characterSelectionSingleton.confirmedSkins.Add(confirmedSkinIndex);
    }
}
