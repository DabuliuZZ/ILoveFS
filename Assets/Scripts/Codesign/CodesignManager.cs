using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DG.Tweening;
using Mirror;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using Sequence = DG.Tweening.Sequence;


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
    public GameObject whiteBoard;
    // 角色演讲皮肤Animator引用
    public Animator characterAnimator;
    
    //——————————————————————————————————————————————————
    
    // 使用SerializedField来公开组的列表
    [SerializeField] private List<CodesignPlayerComponets> playerComponets = new List<CodesignPlayerComponets>();
    // 创建一个字典来存储整数键与组的关联
    public Dictionary<int, CodesignPlayerComponets> playerComponetsDictionary = new Dictionary<int, CodesignPlayerComponets>();
    
    //——————————————————————————————————————————————————
    
    public Sprite[] avatarSkins;
    
    // 角色演讲皮肤动画名列表，string
    public string[] characterAnimNames;
    
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
                // 检测按钮点击次数
                gift.btnCurrentClickedCount++;
                if (gift.btnCurrentClickedCount > gift.btnClickedCount)
                {
                    gift.btn.interactable = false;
                    return;
                }
                
                int id = 0;
                // 查找对应传入clientId的Player实例
                Player[] playerss = FindObjectsOfType<Player>();
                foreach (var player in playerss)
                {
                    if (player.isLocalPlayer)
                    {
                        id = player.clientId;
                    }
                }
                
                OnGiftClicked(gift.giftType,id);
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

    [Command(requiresAuthority = false)]public void OnGiftClicked(GiftType giftType,int id)
    {
        RpcOnGiftClicked(giftType,id);
    }

    // 记录哪个玩家丢了哪些礼物 (ID, 礼物列表)
    private Dictionary<int, List<GiftType>> giftList = new();
    // 记录玩家点击礼物的次数
    private Dictionary<int, Dictionary<GiftType, int>> playerGiftClickCount = new();
    
    [ClientRpc] public void RpcOnGiftClicked(GiftType giftType,int id)
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

        image.sprite = AssetsLoader.instance.gifts.First(item => item.giftType == giftType).giftSprite;
        
        Sequence sequence = DOTween.Sequence();

        sequence.Append(image.DOColor(Color.clear, 2).OnComplete(() => Destroy(image.gameObject)));
        sequence.Join(image.rectTransform.DOMoveY(image.rectTransform.anchoredPosition.y + 20f, 1.5f));

        if (currentState == StateType.Pitching)
        {
            // 特殊逻辑检测，如果满足特殊逻辑，直接返回，不执行后续代码
            if (CheckGifts(id, giftType)) { return; }
            
            // 如果 CheckGifts() 返回 false，执行后续的常规处理逻辑
            currentDisplayingPlayer.AddGift(giftType);
            
            // 将礼物添加到礼物列表中
            if (!giftList.ContainsKey(id))
            {
                giftList[id] = new List<GiftType>();
            }
            giftList[id].Add(giftType);
        }
    }

    // 检测礼物的特殊逻辑
    public bool CheckGifts(int playerId, GiftType giftType)
    {
        //————————————————————————————————————————————————————————————————————————————————————
        // 专门处理 Shit 和 Slippers 类型的礼物
        // 筛选出所有 GiftType.Shit 和 GiftType.TuoXie 类型的礼物
        var relevantBadGifts = giftList
            .SelectMany(g => g.Value
                .Where(gt => gt == GiftType.Shit || gt == GiftType.Slippers)
                .Select(gt => new { PlayerId = g.Key, GiftType = gt }))
            .ToList();
        
        // 获取相关玩家的ID列表（去重）
        var badPlayerIds = relevantBadGifts
            .Select(g => g.PlayerId)
            .Distinct()
            .ToList();

        // 计算相关礼物的总数
        int giftCount = relevantBadGifts.Count;

        // 检查礼物总数是否大于 2，且来自至少两个不同的玩家
        if (giftCount >= 2 && badPlayerIds.Count >= 2)
        {
            // 遍历相关礼物并给对应玩家添加相同类型的礼物
            foreach (var badGift in relevantBadGifts)
            { 
                AssetsLoader.instance.GetPlayer(badGift.PlayerId).AddGift(badGift.GiftType);
            }
            
            // 如果满足特殊条件，返回 true
            return true;
        }
        
        //————————————————————————————————————————————————————————————————————————————————————————
        // 专门处理 Flower 和 Heart 类型的礼物
        // 初始化玩家的礼物点击记录
        if (!playerGiftClickCount.ContainsKey(playerId))
        {
            playerGiftClickCount[playerId] = new Dictionary<GiftType, int>();
        }

        // 初始化该礼物类型的点击次数
        if (!playerGiftClickCount[playerId].ContainsKey(giftType))
        {
            playerGiftClickCount[playerId][giftType] = 0;
        }

        // 增加礼物的点击次数
        playerGiftClickCount[playerId][giftType]++;

        // 如果玩家不是第一次送出 Flower 或 Heart 类型的礼物，则执行特殊逻辑
        if ((giftType == GiftType.Flower || giftType == GiftType.Heart) &&
            playerGiftClickCount[playerId][giftType] > 1)
        {
            AssetsLoader.instance.GetPlayer(playerId).AddGiftNoScore(giftType);

            // 返回 true 表示已经执行了特殊逻辑
            return true;
        }
        
        //————————————————————————————————————————————————————————————————————————————————————————
        // 如果不满足特殊条件，返回 false
        return false;
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
        // 猴子淡出后失活
        monkeyAnimator.GetComponent<Image>().DOFade(0, 1f).OnComplete(() =>
        {
            monkeyAnimator.gameObject.SetActive(false);

            // 白板素材淡入
            whiteBoard.GetComponent<Image>().DOFade(1, 1f).OnComplete(() => { });
        });
        
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
        giftList.Clear();
        playerGiftClickCount.Clear();
        RpcPitchButtonPressedUniTask(clientId, index);
    }

    public async UniTask RpcPitchButtonPressedUniTask(int clientId, int index)
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
            if (cardFaceCopy != null && stickyNoteCopy != null)
            {
                // 问题卡和答题卡淡出后销毁
                var stickyNote1ImageLast = stickyNoteCopy.transform.GetChild(0).GetComponent<Image>();
                var stickyNote2ImageLast = stickyNoteCopy.transform.GetChild(1).GetComponent<Image>();
                
                await stickyNote1ImageLast.GetComponent<Image>().DOFade(0, 1f).AsyncWaitForCompletion();
                await stickyNote2ImageLast.GetComponent<Image>().DOFade(0, 1f).AsyncWaitForCompletion();
                Destroy(stickyNoteCopy);
            
                await cardFaceCopy.GetComponent<Image>().DOFade(0, 1f).AsyncWaitForCompletion();
                Destroy(cardFaceCopy);
                
                // 角色演讲皮肤GameObject淡出
                await characterAnimator.GetComponent<Image>().DOFade(0, 1f).AsyncWaitForCompletion();
            }

            //——————————————————————————————————————————————————————————————————————
            // 正常流程
            
            // 查找对应传入clientId的Player实例
            Player[] playerss = FindObjectsOfType<Player>();
            foreach (var player in playerss)
            {
                // 从player脚本处拿到skinIndex，获取角色演讲皮肤动画名
                if (player.clientId == clientId)
                {
                    string characterAnimName = characterAnimNames[player.skinIndex];

                    // 角色演讲皮肤Animator播对应动画
                    characterAnimator.Play(characterAnimName);

                    // 角色演讲皮肤GameObject淡入
                    await characterAnimator.GetComponent<Image>().DOFade(1, 1f).AsyncWaitForCompletion();
                }
            }
            
            // 创建卡片和便签的实例
            cardFaceCopy = Instantiate(cardFace, pitchCardPos[index].position, Quaternion.identity,
                    canvas.transform);
            stickyNoteCopy = Instantiate(stickyNotes, pitchNotesPos[index].position, Quaternion.identity,
                    canvas.transform);
            stickyNoteCopy.GetComponent<Animator>().Play("Empty");

            // 问题卡与答题卡激活后淡入
            cardFaceCopy.SetActive(true);
            stickyNoteCopy.SetActive(true);
            
            var cardFaceCopyImage = cardFaceCopy.GetComponent<Image>();
            var stickyNote1Image = stickyNoteCopy.transform.GetChild(0).GetComponent<Image>();
            var stickyNote2Image = stickyNoteCopy.transform.GetChild(1).GetComponent<Image>();     
            
            
            cardFaceCopyImage.color = new Color(255,255,255,0);
            cardFaceCopyImage.DOFade(1, 1f);

            stickyNote1Image.color = new Color(255,255,255,0);
            stickyNote1Image.gameObject.SetActive(true);
            stickyNote1Image.DOFade(1, 1f);
            
            stickyNote2Image.color = new Color(255,255,255,0);
            stickyNote2Image.gameObject.SetActive(true);
            stickyNote2Image.DOFade(1, 1f);

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

                    if (player.isLocalPlayer && player.clientId != 0)
                    {
                        giftPanel.SetActive(false);
                        player.GetComponent<PlayerCodesign>().EnableCards();
                        player.GetComponent<PlayerCodesign>().InActiveInputField();
                    }
                }
                else
                {
                    if (player.isLocalPlayer && player.clientId != 0)
                    {
                        giftPanel.SetActive(true);
                        player.GetComponent<PlayerCodesign>().DisableCards();
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

