using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;


[System.Serializable]
public class CodesignPlayerComponets
{
    public int clientId; // 组的键
    public CodesignComponets Componets; // 组内的元素列表
}

[System.Serializable]
public class CodesignComponets
{
    public Transform selfPlayerObj;
    public Transform selfPos;
    public Image avatarImage;
    public Animator avatarAnimator;
    public string diceAnimName;
    public GameObject questionCardBack;
    public GameObject questionCardFace;
    public Animator StickyNotesAnimator;
    public TMP_InputField stickyNote1InputField;
    public TMP_InputField stickyNote2InputField;
}

public class CodesignManager : NetworkBehaviour
{
    public static CodesignManager instance;
    
    //——————————————————————————————————————————————————
    
    public Animator monkeyAnimator;
    
    // 白板素材引用
    // 角色演讲皮肤Animator引用

    //——————————————————————————————————————————————————
    
    // 使用SerializedField来公开组的列表
    [SerializeField] private List<CodesignPlayerComponets> playerComponets = new List<CodesignPlayerComponets>();
    // 创建一个字典来存储整数键与组的关联
    public Dictionary<int, CodesignPlayerComponets> playerComponetsDictionary = new Dictionary<int, CodesignPlayerComponets>();
    
    //——————————————————————————————————————————————————
    
    public Sprite[] avatarSkins;
    
    // 角色演讲皮肤动画名列表，string
    
    public Transform pos1;
    public Transform player1Obj;
    
    //————————————————————————————————————————————————————
    
    public GameObject dice;
    [SerializeField] private Button rollDiceButton;
    [SerializeField] private Button offDiceButton;
    [SerializeField] private Button startPitchButton;
    [SerializeField] private Button pitchButton;
    [SerializeField] private Button endButton;
    
    //——————————————————————————————————————————————————

    [SerializeField] private Transform[] pitchCardPos;
    [SerializeField] private Transform[] pitchNotesPos;
    private int currentPlayerIndex;

    //————————————————————————————————————————————————————
    
    public float diceAnimTime;
    public float hostThrowCardAnimTime;
    public float cardDeliverAnimTime;
    public float dropStickyNotesAnimTime;
    
    //————————————————————————————————————————————————————
    
    public Image giftPref;
    public DisplayingPlayer currentDisplayingPlayer;
    public StateType currentState;
    public Canvas canvas;
    public GameObject giftPanel;
    
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
        
        // 将列表中的组添加到字典中
        foreach (var playerComponet in playerComponets)
        {
            playerComponetsDictionary.Add(playerComponet.clientId, playerComponet);
        }
    }
    
    public override void OnStartServer()
    {
        rollDiceButton.gameObject.SetActive(true);
        offDiceButton.gameObject.SetActive(true);
        startPitchButton.gameObject.SetActive(true);
        pitchButton.gameObject.SetActive(true);
        endButton.gameObject.SetActive(true);
        
        rollDiceButton.onClick.AddListener(RollDiceStart);
        offDiceButton.onClick.AddListener(OffDice);
        startPitchButton.onClick.AddListener(StartPitch);
        pitchButton.onClick.AddListener(Pitch); 
        endButton.onClick.AddListener(End);
    }
    
    public void ChangeState(StateType stateType)
    {
        currentState = stateType;
    }
    
    public override void OnStartClient() // 每个客户端在进入场景后独立调用一次该方法，类似于 Start，本地调用，本地执行
    {
        CustomNetworkManager.instance.AddComponentsForAllPlayers(typeof(PlayerCodesign));

        foreach (var gift in AssetsLoader.instance.gifts)
        {
            gift.btn.onClick.AddListener(()=>
            {
                OnGiftClicked(gift.giftType);
                if (currentState == StateType.Pitching)
                {
                    foreach (var gift in AssetsLoader.instance.gifts)
                    {
                        gift.btn.interactable = false;
                    }
                }
            });
        }
    }

    [Command(requiresAuthority = false)]public void OnGiftClicked(GiftType giftType)
    {
        RpcOnGiftClicked(giftType);
    }
    
    [ClientRpc] public void RpcOnGiftClicked(GiftType id)
    {
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();

        // 在Canvas范围内生成随机位置
        float randomX = Random.Range( 0,canvasRect.rect.width);
        float randomY = Random.Range(0,canvasRect.rect.height / 2);
        
        // 生成物体在随机的世界坐标位置
        var image = Instantiate<Image>(giftPref, canvas.transform);
        
        // 设置新生成对象的RectTransform位置
        RectTransform rectTransform = image.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = new Vector2(randomX, randomY);

        image.sprite = AssetsLoader.instance.gifts.First(item => item.giftType == id).giftSprite;
        
        Sequence sequence = DOTween.Sequence();

        sequence.Append(image.DOColor(Color.clear, 2).OnComplete(() => Destroy(image.gameObject)));
        sequence.Join(image.rectTransform.DOMoveY(image.rectTransform.anchoredPosition.y + 20f, 1.5f));

        if (currentState == StateType.Pitching)
        {
            currentDisplayingPlayer.AddGift(id);
        }
        
    }
    
    [Command(requiresAuthority = false)] public void RollDiceStart()
    {
        RpcRollDiceStart();
    }
    
    [ClientRpc] public void RpcRollDiceStart()
    {
       dice.SetActive(true);
    }

    void OffDice()
    {
        dice.gameObject.SetActive(false);
    }
    
    [Command(requiresAuthority = false)] public void StartPitch()
    {
        RpcStartPitch();
    }
    
    [ClientRpc] public void RpcStartPitch()
    {
        Player[] players = FindObjectsOfType<Player>();
        foreach (var player in players)
        {
            if (player.isLocalPlayer && player.clientId != 0)
            {
                var playercod = player.GetComponent<PlayerCodesign>();
                playercod.OnStartPitchButtonClicked();
                playercod.DisableCards();
            }
        }
        
        TMP_InputField[] Inputs = FindObjectsOfType<TMP_InputField>();
        foreach (var Input in Inputs)
        {
            Input.interactable = false;
        }
        
        giftPanel.SetActive(true);
    }
    
    
    [Command(requiresAuthority = false)] public void Pitch()
    {
        if (currentPlayerIndex < playerComponets.Count)
        {
            RpcPitchButtonPressed(playerComponets[currentPlayerIndex].clientId, currentPlayerIndex);
            currentPlayerIndex++;
        }
    }

    private GameObject cardFaceCopy;
    private GameObject stickyNoteCopy;

    [ClientRpc] public void RpcPitchButtonPressed(int clientId, int index)
    {
        if (playerComponetsDictionary.TryGetValue(clientId, out var playerComponents))
        {
            // 获取玩家的特定对象
            var cardFace = playerComponents.Componets.questionCardFace;
            var stickyNotes = playerComponents.Componets.StickyNotesAnimator.gameObject;

            // 获取Canvas对象
            Canvas canvas = FindObjectOfType<Canvas>();
            
            //————————————————————————————————————————————————————————————————————
            // 如果上一轮有玩家展示（问题卡与答题卡不为空）

            // 问题卡与答题卡淡出后销毁（下方销毁代码需修改）
            if(cardFaceCopy!= null) Destroy(cardFaceCopy);
            if(stickyNoteCopy!= null) Destroy(stickyNoteCopy);
            
            // 角色演讲皮肤GameObject淡出
            
            //————————————————————————————————————————————————————————————————————
            // 如果上一轮没有玩家展示，第一次点击（问题卡与答题卡为空）
            
            // 猴子淡出后失活
            // 白板素材激活后淡入
            // 角色演讲皮肤GameObject激活
            
            //——————————————————————————————————————————————————————————————————————
            // 正常流程
            
            // 查找对应传入clientId的Player实例
            // 从player脚本处拿到skinIndex，获取角色演讲皮肤动画名
            // 角色演讲皮肤Animator播对应动画
            // 角色演讲皮肤GameObject淡入
            
            // 创建卡片和便签的实例
            cardFaceCopy = Instantiate(cardFace, pitchCardPos[index].position, Quaternion.identity, canvas.transform);
            stickyNoteCopy = Instantiate(stickyNotes, pitchNotesPos[index].position, Quaternion.identity,canvas.transform);

            stickyNoteCopy.GetComponent<Animator>().Play("Empty");

            // 问题卡与答题卡激活后淡入（下方激活代码需修改）
            cardFaceCopy.SetActive(true);
            stickyNoteCopy.SetActive(true);
            
            //——————————————————————————————————————————————————————————
            
            // 使用索引查找子级对象
            var stickyNote1InputField = stickyNoteCopy.transform.GetChild(0).GetComponent<TMP_InputField>();
            var stickyNote2InputField = stickyNoteCopy.transform.GetChild(1).GetComponent<TMP_InputField>();
                        
            // 查找对应传入clientId的Player实例
            Player[] players = FindObjectsOfType<Player>();
            foreach (var player in players)
            {
                if (player.clientId == clientId)
                {
                    stickyNote1InputField.text = player.stickyNote1Text;
                    stickyNote2InputField.text = player.stickyNote2Text;
                    
                    if (player.isLocalPlayer&&player.clientId != 0)
                    {
                        giftPanel.SetActive(false);
                        player.GetComponent<PlayerCodesign>().EnableCards();
                        player.GetComponent<PlayerCodesign>().InActiveInputField();
                    }
                }
                else
                {
                    if (player.isLocalPlayer&&player.clientId != 0)
                    {
                        giftPanel.SetActive(true);
                        player.GetComponent<PlayerCodesign>().DisableCards();
                    }
                }
            }
            
            foreach (var gift in AssetsLoader.instance.gifts)
            {
                gift.btn.interactable = true;
            }

            currentDisplayingPlayer = AssetsLoader.instance.GetPlayer(clientId);
            
            ChangeState(StateType.Pitching);
        }

    }
    [Command(requiresAuthority = false)]private void End()
    {
        var player=AssetsLoader.instance.displayingPlayers.OrderByDescending(player => player.score).FirstOrDefault();
        if(player!=null) RpcEnd(player.id);
    }

    [ClientRpc]
    private void RpcEnd(int clientId)
    {
        if (playerComponetsDictionary.TryGetValue(clientId, out var playerComponents))
        {
            // 获取玩家的特定对象
            var cardFace = playerComponents.Componets.questionCardFace;
            var stickyNotes = playerComponents.Componets.StickyNotesAnimator.gameObject;

            // 获取Canvas对象
            Canvas canvas = FindObjectOfType<Canvas>();
            
            if(cardFaceCopy!= null) Destroy(cardFaceCopy);
            if(stickyNoteCopy!= null) Destroy(stickyNoteCopy);
            
            // 创建卡片和便签的实例
            cardFaceCopy = Instantiate(cardFace, pitchCardPos[clientId-1].position, Quaternion.identity, canvas.transform);
            stickyNoteCopy = Instantiate(stickyNotes, pitchNotesPos[clientId-1].position, Quaternion.identity,canvas.transform);

            stickyNoteCopy.GetComponent<Animator>().Play("Empty");
            
            // 激活复制体
            cardFaceCopy.SetActive(true);
            stickyNoteCopy.SetActive(true);

            // 使用索引查找子级对象
            var stickyNote1InputField = stickyNoteCopy.transform.GetChild(0).GetComponent<TMP_InputField>();
            var stickyNote2InputField = stickyNoteCopy.transform.GetChild(1).GetComponent<TMP_InputField>();
            
            // 查找对应clientId的Player实例
            Player[] players = FindObjectsOfType<Player>();
            foreach (var player in players)
            {
                if (player.clientId == clientId)
                {
                    stickyNote1InputField.text = player.stickyNote1Text;
                    stickyNote2InputField.text = player.stickyNote2Text;
                    
                    if (player.isLocalPlayer&&player.clientId != 0)
                    {
                        giftPanel.SetActive(false);
                        player.GetComponent<PlayerCodesign>().EnableCards();
                    }
                    
                }
                else
                {

                    if (player.isLocalPlayer&&player.clientId != 0)
                    {
                        giftPanel.SetActive(true);
                        player.GetComponent<PlayerCodesign>().DisableCards();
                    }
                }
            }
            foreach (var gift in AssetsLoader.instance.gifts)
            {
                gift.btn.interactable = true;
            }
            ChangeState(StateType.End);
        }
    }

}

