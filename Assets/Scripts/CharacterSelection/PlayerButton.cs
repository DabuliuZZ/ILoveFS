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
    private CharacterSelectionManager characterSelectionManager;
    
    private GameObject buttonSet1; // 客户端1的按钮组
    private GameObject buttonSet2; // 客户端2的按钮组
    private Image characterImage1;
    private Image characterImage2;
    private Sprite[] characterSkins;
    private List<int> confirmedSkins;
    private TextMeshProUGUI playerLog;
    
    private Button switchButton;
    private Button confirmButton;
    private Image currentCharacter;
    private int currentSkinIndex;

    
    private void OnEnable()
    {
        player = GetComponent<Player>();
        clientId = player.clientId;
        characterSelectionManager = CharacterSelectionManager.Instance;
        
        buttonSet1 = characterSelectionManager.buttonSet1;
        buttonSet2 = characterSelectionManager.buttonSet2;
        characterImage1 = characterSelectionManager.character1;
        characterImage2 = characterSelectionManager.character2;
        characterSkins = characterSelectionManager.characterSkins;
        confirmedSkins = characterSelectionManager.confirmedSkins;
        playerLog = characterSelectionManager.playerLog;
        
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
        switchButton.onClick.AddListener(OnSwitchSkin);
        confirmButton.onClick.AddListener(OnConfirmSkin);
        switchButton.gameObject.SetActive(true);
        confirmButton.gameObject.SetActive(true);
    }

    private void OnSwitchSkin()
    {
        currentSkinIndex = (currentSkinIndex + 1) % characterSkins.Length;
        currentCharacter.sprite = characterSkins[currentSkinIndex];
        
        CmdSendSpriteChange(currentSkinIndex, clientId);
        
        foreach (var confirmedSkinIndex in confirmedSkins)
        {
            if (currentSkinIndex == confirmedSkinIndex)
            {
                confirmButton.interactable = false;
                currentCharacter.color = Color.gray;
                return;
            }
        }
        confirmButton.interactable = true;
        currentCharacter.color = Color.white;
    }
    
    [Command] public void CmdSendSpriteChange(int newSkinIndex, int clientId)
    {
        RpcUpdateSprite(newSkinIndex, clientId);
    }
    
    [ClientRpc] public void RpcUpdateSprite(int newSkinIndex, int clientId)
    {
        if (clientId == 1)
        {
            characterImage1.sprite = characterSkins[newSkinIndex];
        }
        if (clientId == 2)
        {
            characterImage2.sprite = characterSkins[newSkinIndex];
        }
    }

    private void OnConfirmSkin()
    {
        foreach (var confirmedSkinIndex in confirmedSkins)
        {
            if (currentSkinIndex == confirmedSkinIndex)
            {
                playerLog.text += "This skin has been chosen,change one!" + "\n";
                return;
            }
        }

        switchButton.interactable = false;
        confirmButton.interactable = false;
        
        player.skinIndex = currentSkinIndex;
        
        CmdConfirmSkin(currentSkinIndex,clientId);
    }

    [Command] public void CmdConfirmSkin(int confirmedSkinIndex,int clientId)
    {
        RpcConfirmSkin(confirmedSkinIndex);
        characterSelectionManager.OnPlayerConfirmed(clientId);
    }

    [ClientRpc] public void RpcConfirmSkin(int confirmedSkinIndex)
    { 
        characterSelectionManager.confirmedSkins.Add(confirmedSkinIndex);
    }
}
