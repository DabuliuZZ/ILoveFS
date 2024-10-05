using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class PlayerCodesign : NetworkBehaviour
{
    private Player player;
    private int clientId;
    private CodesignManager codesignManager;
    
    //——————————————————————————————————————————————
    
    private Sprite[] avatarSkins;
    private int skinIndex;
    private Image avatarImage;
    private Animator avatarAnimator;
    private Transform pos1;
    private Transform player1Obj;
    private Transform selfPos;
    private Transform selfPlayerObj;
    
    //————————————————————————————————————————————————
    
    private GameObject questionCardBack;
    private GameObject questionCardFace;
    private Animator questionCardAnimator;
    private Button questionCardButton;
    private Animator stickyNotesAnimator;
    private TMP_InputField stickyNote1InputField;
    private TMP_InputField stickyNote2InputField;
    private TextMeshProUGUI playerName;
    // private string[] playerNames;
    
    //————————————————————————————————————————————————
    
    private GameObject dice;
    private Animator diceAnimator;
    private string diceAnimName;
    private Button diceButton;
    private float diceAnimTime;
    
    //————————————————————————————————————————————————
    
    private Animator hostAnimator;
    private float hostThrowCardAnimTime;
    private float cardDeliverAnimTime;
    private float dropStickyAnimTime;
    
    //————————————————————————————————————————————————

    private Animator monkeyAnimator;
    
    //————————————————————————————————————————————————
    
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
        // playerNames = codesignManager.playerNames;

        //————————————————————————————————————————————————
        
        dice = codesignManager.dice;
        diceAnimator = dice.GetComponent<Animator>();
        diceButton = dice.GetComponent<Button>();
        diceAnimTime = codesignManager.diceAnimTime;
        
        //————————————————————————————————————————————————————
        
        hostThrowCardAnimTime = codesignManager.hostThrowCardAnimTime;
        cardDeliverAnimTime = codesignManager.cardDeliverAnimTime;
        dropStickyAnimTime = codesignManager.dropStickyNotesAnimTime;
        
        //————————————————————————————————————————————————————

        monkeyAnimator = codesignManager.monkeyAnimator;
        
        //————————————————————————————————————————————————————

        if (player.isLocalPlayer && clientId != 0)
        {
            if (codesignManager.playerComponetsDictionary.TryGetValue(clientId, out var playerComponents))
            {
                playerName = playerComponents.Componets.playerName;
                avatarImage = playerComponents.Componets.avatarImage;
                avatarAnimator = playerComponents.Componets.avatarAnimator;
                selfPos = playerComponents.Componets.selfPos;
                selfPlayerObj = playerComponents.Componets.selfPlayerObj;

                questionCardBack = playerComponents.Componets.questionCardBack;
                questionCardFace = playerComponents.Componets.questionCardFace;
                stickyNotesAnimator = playerComponents.Componets.stickyNotesAnimator;
                stickyNote1InputField = playerComponents.Componets.stickyNote1InputField;
                stickyNote2InputField = playerComponents.Componets.stickyNote2InputField;
                
                diceAnimName = playerComponents.Componets.diceAnimName;
            }

            diceButton.onClick.AddListener(RollDiceAnim);
            questionCardAnimator = questionCardBack.GetComponent<Animator>();
            questionCardButton = questionCardBack.GetComponent<Button>();
            
            if (clientId != 1)
            {
                selfPlayerObj.position = pos1.position;
                player1Obj.position = selfPos.position;
                avatarImage.sprite = avatarSkins[skinIndex];
                playerName.text = codesignManager.playerNames[skinIndex];
            }
            
            CmdAvatarSprite(skinIndex,clientId);
            CmdPlayerName(skinIndex,clientId);
        }
    }
    
    [Command] public void CmdAvatarSprite(int skinIndex, int clientId)
    {
        RpcUpdateAvatarSprite(skinIndex, clientId);
    }

    [Command]
    public void CmdPlayerName(int skinIndex, int clientId)
    {
        RpcUpdatePlayerName(skinIndex, clientId);
    }
    
    [ClientRpc] public void RpcUpdateAvatarSprite(int skinIndex, int clientId)
    {
        if (codesignManager.playerComponetsDictionary.TryGetValue(clientId, out var playerComponents))
        {
            playerComponents.Componets.avatarImage.sprite = avatarSkins[skinIndex];
        }
    }

    [ClientRpc] public void RpcUpdatePlayerName(int skinIndex, int clientId)
    {
        if (codesignManager.playerComponetsDictionary.TryGetValue(clientId, out var playerComponents))
        {
            playerComponents.Componets.playerName.text = codesignManager.playerNames[skinIndex];
        }
    }

    void RollDiceAnim()
    {
        // 玩家头像跳一下
        avatarImage.rectTransform.DOMoveY(avatarImage.rectTransform.position.y + 55f, 0.25f)
            .OnComplete(() =>
            {
                avatarImage.rectTransform.DOMoveY(avatarImage.rectTransform.position.y - 55f, 0.25f);
            });

        diceButton.interactable = false;
        diceButton.onClick.RemoveListener(RollDiceAnim);
        
        Debug.Log("diceAnimator play " + diceAnimName);
        Debug.Log("avatar play AvatarRollDice");

        // 播骰子动画，四选一
        diceAnimator.Play(diceAnimName);  
        
        Invoke("HostThrowCardDeliver",diceAnimTime);
    }

    void HostThrowCardDeliver()
    {
        Debug.Log("hostAnimator play CardDeliver");
        
        monkeyAnimator.Play("DrawCard1");
        
        Invoke("CardDeliver",hostThrowCardAnimTime);
    }

    void CardDeliver()
    {
        Debug.Log("questionCardAnimator play CardDrop");
        questionCardAnimator.Play("CardDrop");
        monkeyAnimator.Play("DrawCard2");
        
        Invoke("QuestionCardAddListener",cardDeliverAnimTime);
    }
    
    void QuestionCardAddListener()
    {
        questionCardButton.interactable = true;
        questionCardButton.onClick.AddListener(OnCardClick);
    }
    
    void OnCardClick()
    {
        questionCardButton.interactable = false;
        
        
        monkeyAnimator.Play("PlayHead");
        diceAnimator.Play("RemoveDice");
        
        // 第一阶段：将卡牌背面翻转至90°
        questionCardBack.transform.DORotate(new Vector3(0, 90, 0), 0.5f).OnComplete(() =>
        {
            // 完成第一阶段翻转后，背面失活，正面激活
            questionCardBack.SetActive(false);
            questionCardFace.SetActive(true);

            // 第二阶段：将卡牌正面从90°翻转至0°
            questionCardFace.transform.DORotate(new Vector3(0, 0, 0), 0.5f);
        });
        
        stickyNotesAnimator.Play("DropStickyNotes");
        
        Invoke("ActiveInputField",dropStickyAnimTime);
    }

    void ActiveInputField()
    {
        stickyNote1InputField.interactable = true;
        stickyNote2InputField.interactable = true;
    }
    
    public void InActiveInputField()
    {
        stickyNote1InputField.interactable = false;
        stickyNote2InputField.interactable = false;
    }

    public void OnStartPitchButtonClicked()
    {
        CmdStickyNotesInputSubmit(stickyNote1InputField.text,stickyNote2InputField.text);
    }

    [Command] public void CmdStickyNotesInputSubmit(string stickyNote1Text, string stickyNote2Text)
    {
        RpcStickyNotesInputSubmit(stickyNote1Text,stickyNote2Text);
    }
    
    [ClientRpc] public void RpcStickyNotesInputSubmit(string stickyNote1Text, string stickyNote2Text)
    {
        player.stickyNote1Text = stickyNote1Text;
        player.stickyNote2Text = stickyNote2Text;
    }

    public void DisableCards()
    {
        questionCardFace.SetActive(false);
        questionCardBack.SetActive(false);
        stickyNote1InputField.gameObject.SetActive(false);
        stickyNote2InputField.gameObject.SetActive(false);
    }
    public void EnableCards()
    {
        questionCardFace.SetActive(true);
        questionCardBack.SetActive(true);
        stickyNote1InputField.gameObject.SetActive(true);
        stickyNote2InputField.gameObject.SetActive(true);
    }
}
