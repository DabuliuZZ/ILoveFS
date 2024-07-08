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
    
    private Sprite[] characterSkins;
    private List<int> confirmedSkins;
    private TextMeshProUGUI playerLog;
    
    private Button switchButton;
    private Button confirmButton;
    private Image characterImage;
    private int currentSkinIndex;
    
    private void OnEnable()
    {
        player = GetComponent<Player>();
        clientId = player.clientId;
        
        characterSelectionManager = CharacterSelectionManager.Instance;
        
        characterSkins = characterSelectionManager.characterSkins;
        confirmedSkins = characterSelectionManager.confirmedSkins;
        playerLog = characterSelectionManager.playerLog;
        
        if (player.isLocalPlayer)
        {
            if (characterSelectionManager.playerComponetsDictionary.TryGetValue(clientId, out var playerComponents))
            {
                switchButton = playerComponents.Componets.SwitchButton;
                confirmButton = playerComponents.Componets.ConfirmButton;
                characterImage = playerComponents.Componets.CharacterImage;
                
                switchButton.gameObject.SetActive(true);
                confirmButton.gameObject.SetActive(true);
                
                // 进行按钮的初始化或其他设置
                switchButton.onClick.AddListener(OnSwitchSkin);
                confirmButton.onClick.AddListener(OnConfirmSkin);
            }
        }
    }

    private void OnSwitchSkin()
    {
        currentSkinIndex = (currentSkinIndex + 1) % characterSkins.Length;
        characterImage.sprite = characterSkins[currentSkinIndex];
        
        CmdSendSpriteChange(currentSkinIndex, clientId);
        
        foreach (var confirmedSkinIndex in confirmedSkins)
        {
            if (currentSkinIndex == confirmedSkinIndex)
            {
                confirmButton.interactable = false;
                characterImage.color = Color.gray;
                return;
            }
        }
        confirmButton.interactable = true;
        characterImage.color = Color.white;
    }
    
    [Command] public void CmdSendSpriteChange(int newSkinIndex, int clientId)
    {
        RpcUpdateSprite(newSkinIndex, clientId);
    }
    
    [ClientRpc] public void RpcUpdateSprite(int newSkinIndex, int clientId)
    {
        foreach (var playerComponet in characterSelectionManager.playerComponetsDictionary)
        {
            if (playerComponet.Key == clientId)
            {
                playerComponet.Value.Componets.CharacterImage.sprite = characterSkins[newSkinIndex];
                break;
            }
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

    [ClientRpc]
    public void RpcConfirmSkin(int confirmedSkinIndex)
    {
        characterSelectionManager.confirmedSkins.Add(confirmedSkinIndex);
    }
}
