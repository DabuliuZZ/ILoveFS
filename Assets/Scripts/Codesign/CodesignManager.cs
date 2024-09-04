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
    
    // 使用SerializedField来公开组的列表
    [SerializeField] private List<CodesignPlayerComponets> playerComponets = new List<CodesignPlayerComponets>();
    // 创建一个字典来存储整数键与组的关联
    public Dictionary<int, CodesignPlayerComponets> playerComponetsDictionary = new Dictionary<int, CodesignPlayerComponets>();
    
    //——————————————————————————————————————————————————
    
    public Sprite[] avatarSkins;
    public Transform pos1;
    public Transform player1Obj;
    
    //——————————————————————————————————————————————————
    
    public GameObject dice;
    [SerializeField] private Button rollDiceButton;
    [SerializeField] private Button offDiceButton;
    [SerializeField] private Button startPitchButton;
    [SerializeField] private Button pitchButton;
    
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
    
    //————————————————————————————————————————————————————
    
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
        
        rollDiceButton.onClick.AddListener(RollDiceStart);
        offDiceButton.onClick.AddListener(OffDice);
        startPitchButton.onClick.AddListener(StartPitch);
        pitchButton.onClick.AddListener(Pitch); 
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
        sequence.Join(image.rectTransform.DOMoveY(image.rectTransform.anchoredPosition.y + 5f, 1.5f));

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
                player.GetComponent<PlayerCodesign>().OnStartPitchButtonClicked();
            }
        }
        
        TMP_InputField[] Inputs = FindObjectsOfType<TMP_InputField>();
        foreach (var Input in Inputs)
        {
            Input.interactable = false;
        }
    }
    
    [Command(requiresAuthority = false)] public void Pitch()
    {
        if (currentPlayerIndex < playerComponets.Count)
        {
            RpcPitchButtonPressed(playerComponets[currentPlayerIndex].clientId, currentPlayerIndex);
            currentPlayerIndex++;
        }
    }

    [ClientRpc] public void RpcPitchButtonPressed(int clientId, int index)
    {
        if (playerComponetsDictionary.TryGetValue(clientId, out var playerComponents))
        {
            // 获取玩家的特定对象
            var cardFace = playerComponents.Componets.questionCardFace;
            var stickyNotes = playerComponents.Componets.StickyNotesAnimator.gameObject;

            // 获取Canvas对象
            Canvas canvas = FindObjectOfType<Canvas>();
            
            // 创建卡片和便签的实例
            var cardFaceCopy = Instantiate(cardFace, pitchCardPos[index].position, Quaternion.identity, canvas.transform);
            var stickyNoteCopy = Instantiate(stickyNotes, pitchNotesPos[index].position, Quaternion.identity,canvas.transform);

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
                    break;
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


}

