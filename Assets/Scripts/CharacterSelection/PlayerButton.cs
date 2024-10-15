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
    private Sprite[] avatarSkins;
    private List<int> confirmedSkins;
    private TextMeshProUGUI playerLog;
    
    private Button switchButton;
    private Button confirmButton;
    private Image characterImage;
    private Image avatarImage;
    private int currentSkinIndex;
    
    private void OnEnable()
    {
        player = GetComponent<Player>();
        clientId = player.clientId;
        
        characterSelectionManager = CharacterSelectionManager.instance;
        
        characterSkins = characterSelectionManager.characterSkins;
        avatarSkins = characterSelectionManager.avatarSkins;
        confirmedSkins = characterSelectionManager.confirmedSkins;
        playerLog = characterSelectionManager.playerLog;
        
        if (player.isLocalPlayer)
        {
            if (characterSelectionManager.playerComponetsDictionary.TryGetValue(clientId, out var playerComponents))
            {
                switchButton = playerComponents.Componets.switchButton;
                confirmButton = playerComponents.Componets.confirmButton;
                characterImage = playerComponents.Componets.characterImage;
                avatarImage = playerComponents.Componets.avatarImage;
                
                switchButton.gameObject.SetActive(true);
                confirmButton.gameObject.SetActive(true);
                avatarImage.color = Color.white;
                
                // 进行按钮的初始化或其他设置
                switchButton.onClick.AddListener(OnSwitchSkin);
                confirmButton.onClick.AddListener(OnConfirmSkin);
                
                // 播BGM
                // AudioManager.Instance.PlayBGMLocal();
            }
        }
    }

    private void OnSwitchSkin()
    {
        currentSkinIndex = (currentSkinIndex + 1) % characterSkins.Length;
        
        characterImage.sprite = characterSkins[currentSkinIndex];
        avatarImage.sprite = avatarSkins[currentSkinIndex];
        
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
        
        // 播切换按钮音效
        // AudioManager.Instance.PlayAudioClipLocal();
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
                playerComponet.Value.Componets.characterImage.sprite = characterSkins[newSkinIndex];
                playerComponet.Value.Componets.avatarImage.sprite = avatarSkins[newSkinIndex];
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
        
        CmdConfirmSkin(currentSkinIndex,clientId);
        
        // 播锁定按钮音效
        // AudioManager.Instance.PlayAudioClipLocal();
    }

    [Command] public void CmdConfirmSkin(int confirmedSkinIndex,int clientId)
    {
        RpcConfirmSkin(confirmedSkinIndex);
        characterSelectionManager.OnPlayerConfirmed(clientId);
    }

    [ClientRpc] public void RpcConfirmSkin(int confirmedSkinIndex)
    {
        characterSelectionManager.confirmedSkins.Add(confirmedSkinIndex);
        player.skinIndex = confirmedSkinIndex;
    }
}
