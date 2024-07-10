using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCodesign : NetworkBehaviour
{
    private Player player;
    private int clientId;
    private CodesignManager codesignManager;
    
    //——————————————————————————————————————————————
    
    private Sprite[] avatarSkins;
    private int skinIndex;
    private Image avatarImage;
    public Animator avatarAnimator;
    private Transform pos1;
    private Transform player1Obj;
    private Transform selfPos;
    private Transform selfPlayerObj;
    
    //————————————————————————————————————————————————
    
    private GameObject questionCard;
    private Animator questionCardAnimator;
    private Button questionCardButton;
    
    //————————————————————————————————————————————————
    
    private GameObject dice;
    private Animator diceAnimator;
    private string diceAnimName;
    private Button diceButton;
    private float diceAnimTime;
    
    //————————————————————————————————————————————————
    
    private Animator hostAnimator;
    private float hostThrowCardAnimTime;
    private float cardDeliverTime;
    
    private void OnEnable()
    {
        player = GetComponent<Player>();
        clientId = player.clientId;
        codesignManager = CodesignManager.instance;
        
        //————————————————————————————————————————————————
        
        avatarSkins = codesignManager.avatarSkins;
        skinIndex = player.skinIndex;
        pos1 = codesignManager.pos1;
        player1Obj = codesignManager.player1Obj;

        //————————————————————————————————————————————————
        
        dice = codesignManager.dice;
        diceAnimator = dice.GetComponent<Animator>();
        diceButton = dice.GetComponent<Button>();
        diceAnimTime = codesignManager.diceAnimTime;
        
        //——————————————————————————————————————————————————
        
        hostThrowCardAnimTime = codesignManager.hostThrowCardAnimTime;
        cardDeliverTime = codesignManager.cardDeliverTime;
        
        //————————————————————————————————————————————————————
        
        if (player.isLocalPlayer && clientId != 0)
        {
            if (codesignManager.playerComponetsDictionary.TryGetValue(clientId, out var playerComponents))
            {
                avatarImage = playerComponents.Componets.avatarImage;
                avatarAnimator = playerComponents.Componets.avatarAnimator;
                selfPos = playerComponents.Componets.selfPos;
                selfPlayerObj = playerComponents.Componets.selfPlayerObj;

                questionCard = playerComponents.Componets.qustionCard;
                
                diceAnimName = playerComponents.Componets.diceAnimName;
            }

            diceButton.onClick.AddListener(RollDiceAnim);
            questionCardAnimator = questionCard.GetComponent<Animator>();
            questionCardButton = questionCard.GetComponent<Button>();
            
            if (clientId != 1)
            {
                selfPlayerObj.position = pos1.position;
                player1Obj.position = selfPos.position;
                avatarImage.sprite = avatarSkins[skinIndex];
            }
            
            CmdAvatarSprite(skinIndex,clientId);
        }
    }
    
    [Command] public void CmdAvatarSprite(int skinIndex, int clientId)
    {
        RpcUpdateAvatarSprite(skinIndex, clientId);
    }
    
    [ClientRpc] public void RpcUpdateAvatarSprite(int skinIndex, int clientId)
    {
        if (codesignManager.playerComponetsDictionary.TryGetValue(clientId, out var playerComponents))
        {
            playerComponents.Componets.avatarImage.sprite = avatarSkins[skinIndex];
        }
    }

    void RollDiceAnim()
    {
        diceButton.interactable = false;
        diceButton.onClick.RemoveListener(RollDiceAnim);
        
        Debug.Log("diceAnimator play " + diceAnimName);
        Debug.Log("avatar play AvatarRollDice");
        //diceAnimator.Play(diceAnimName);
        //avatarAnimator.Play("AvatarRollDice");
        
        Invoke("HostThrowCardDeliver",diceAnimTime);
    }

    void HostThrowCardDeliver()
    {
        Debug.Log("hostAnimator play CardDeliver");
        //hostAnimator.Play("CardDeliver");
        
        Invoke("CardDeliver",hostThrowCardAnimTime);
    }

    void CardDeliver()
    {
        Debug.Log("questionCardAnimator play CardDrop");
        questionCardAnimator.Play("CardDrop");
        
        Invoke("QuestionCardAddListener",cardDeliverTime);
    }
    
    void QuestionCardAddListener()
    {
        questionCardButton.interactable = true;
        questionCardButton.onClick.AddListener(RemoveDice);
    }
    
    void RemoveDice()
    {
        questionCardButton.interactable = false;
        diceAnimator.Play("RemoveDice");
    }
}
