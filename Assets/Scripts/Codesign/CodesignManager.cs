using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Mirror;
using TMPro;
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
    public TextMeshProUGUI playerName;
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
    
    private int currentPlayerskinIndex;
    
    //——————————————————————————————————————————————————
    
    // 使用SerializedField来公开组的列表
    [SerializeField] private List<CodesignPlayerComponets> playerComponets = new List<CodesignPlayerComponets>();
    // 创建一个字典来存储整数键与组的关联
    public Dictionary<int, CodesignPlayerComponets> playerComponetsDictionary = new Dictionary<int, CodesignPlayerComponets>();
    
    //——————————————————————————————————————————————————
    
    public Sprite[] avatarSkins;
    // 角色名
    public string[] playerNames;
    // 不同角色的动画器控制器
    public RuntimeAnimatorController[] characterAnimatorControllers;
    
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
    
    // 用于记录每个礼物类型的总点击次数
    private Dictionary<GiftType, int> giftTypeClickCount = new Dictionary<GiftType, int>();
    
    public override void OnStartClient() // 每个客户端在进入场景后独立调用一次该方法，类似于 Start，本地调用，本地执行
    {
        CustomNetworkManager.instance.AddComponentsForAllPlayers(typeof(PlayerCodesign));
        
        // 初始化点击计数器
        foreach (GiftType giftType in System.Enum.GetValues(typeof(GiftType)))
        {
            giftTypeClickCount[giftType] = 0;
        }
        
        foreach (var gift in AssetsLoader.instance.gifts)
        {
            gift.btn.onClick.AddListener(()=>
            {
                // 增加当前礼物类型的点击次数
                giftTypeClickCount[gift.giftType]++;

                // 检测当前礼物类型的点击次数是否超过礼物本身的上限
                if (giftTypeClickCount[gift.giftType] >= gift.btnClickedCount)
                {
                    gift.btn.interactable = false; // 禁用按钮
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
            });
        }
    }

    [Command(requiresAuthority = false)]public void OnGiftClicked(GiftType giftType,int id)
    {
        RpcOnGiftClicked(giftType,id);
    }

    // 记录哪个玩家丢了哪些礼物 (ID, 礼物列表)
    private Dictionary<int, List<GiftType>> giftList = new();
    
    [ClientRpc] public void RpcOnGiftClicked(GiftType giftType,int id)
    {
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();

        // 在Canvas范围内生成随机位置
        float randomX = Random.Range( 0,canvasRect.rect.width);
        float randomY = Random.Range(0,canvasRect.rect.height / 2);
        
        // 生成物体在随机的世界坐标位置
        var image = Instantiate<Image>(giftPref, canvas.transform);
        
        // 放大特效
        image.GetComponent<RectTransform>().localScale = new Vector3(3.5f, 3.5f, 1);

        image.GetComponent<Image>().raycastTarget = false;
        
        // 设置新生成对象的RectTransform位置
        RectTransform rectTransform = image.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = new Vector2(randomX, randomY);

        image.sprite = AssetsLoader.instance.gifts.First(item => item.giftType == giftType).giftSprite;
        
        Sequence sequence = DOTween.Sequence();

        sequence.Append(image.DOFade(0, 2).OnComplete(() => Destroy(image.gameObject)));
        sequence.Join(image.rectTransform.DOMoveY(image.rectTransform.anchoredPosition.y + 20f, 1.5f));

        if (currentState == StateType.Pitching && id!=0)
        {
            // 将礼物添加到礼物列表中
            if (!giftList.ContainsKey(id))
            {
                giftList.Add(id, new List<GiftType>());
                Debug.Log(id+" " + giftType);
            }
            
            if (!giftList[id].Contains(giftType))
            {
                Debug.Log(id+" " + giftType);
                giftList[id].Add(giftType);
                HandleScoreAdd(id,giftType);
            }

            // 收到赞的动画
            if (giftType == GiftType.Flower || giftType == GiftType.Heart)
            {
                characterAnimator.Play("Good");
                Debug.Log("Good");
            }
            // 收到补充的动画
            // if (giftType == GiftType.Speaker || giftType == GiftType.Microphone)
            // {
            //     characterAnimator.Play("Talk");
            //     Debug.Log("Talk");
            // }
            // 收到踩的动画
            if (giftType == GiftType.Shit || giftType == GiftType.Slippers)
            {
                characterAnimator.Play("Bad");
                Debug.Log("Bad");
            }
            
            currentDisplayingPlayer.AddGift(giftType);
        }
    }

    public void HandleScoreAdd(int id,GiftType giftType)
    {
        if (giftList.Count(gt => gt.Value.Contains(GiftType.Shit) || gt.Value.Contains(GiftType.Slippers))>= 2)
        {
            giftList.Where(gt => gt.Value.Contains(GiftType.Shit) || gt.Value.Contains(GiftType.Slippers)).ToList().ForEach(gt=>AddShitGifts(gt.Key,3));
        }
        
        if (giftType == GiftType.Flower || giftType == GiftType.Heart)
        {
            currentDisplayingPlayer.AddScoreWithoutGift(1);
            AssetsLoader.instance.GetPlayer(id).AddScoreWithoutGift(1);
        }
        
        if (giftType == GiftType.Speaker || giftType == GiftType.Microphone)
        {
            AssetsLoader.instance.GetPlayer(id).AddScoreWithoutGift(2);
        }
        
        if (giftType == GiftType.Shit || giftType == GiftType.Slippers)
        {
            // 获取所有发送过踩类礼物的玩家
            var stepGiftSenders = giftList
                .Where(gt => gt.Value.Any(g => IsStepGift(g)))
                .Select(gt => gt.Key)
                .Distinct()
                .ToList();

            // 如果至少有两个玩家发送了踩类礼物
            if (stepGiftSenders.Count >= 2)
            {
                // 为所有发送过踩类礼物的玩家增加3分
                stepGiftSenders.ForEach(senderId => AddShitGifts(senderId, 3));
            }

            // 踩类礼物不直接加分，返回
            return;
        }
    }
    private bool IsStepGift(GiftType giftType)
    {
        return giftType == GiftType.Shit || giftType == GiftType.Slippers;
    }
    
    public void AddShitGifts(int id,int score)
    {
        AssetsLoader.instance.GetPlayer(id).AddScoreWithoutGift(score);
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
        // 白板素材淡入
        whiteBoard.GetComponent<Image>().DOFade(1, 1f).OnComplete(() => { });
        
        // 猴子淡出后换位置
        monkeyAnimator.GetComponent<Image>().DOFade(0, 1f).OnComplete(() =>
        {
            // 猴子换位置
            monkeyAnimator.gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(695,144);
            monkeyAnimator.gameObject.GetComponent<RectTransform>().rotation = Quaternion.Euler(0,180,0);
            
            // 猴子淡入
            monkeyAnimator.GetComponent<Image>().DOFade(1, 1f);
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
        
        // 初始化点击计数器
        foreach (GiftType giftType in System.Enum.GetValues(typeof(GiftType)))
        {
            giftTypeClickCount[giftType] = 0;
        }

        // 猴子淡出后失活
        monkeyAnimator.GetComponent<Image>().DOFade(0, 1f).OnComplete(() =>
        {
            monkeyAnimator.gameObject.SetActive(false);
        });
        
        // 获取当前演讲玩家的皮肤序号
        Player[] playerss = FindObjectsOfType<Player>();
        foreach (var player in playerss)
        {
            if (player.clientId == clientId)
            {
                currentPlayerskinIndex = player.skinIndex;
            }
        }
        // 设置当前演讲玩家的角色动画器
        characterAnimator.runtimeAnimatorController = characterAnimatorControllers[currentPlayerskinIndex];
        
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
                var cardFaceCopyImageLast = cardFaceCopy.GetComponent<Image>();
                
                stickyNote1ImageLast.DOFade(0, 1f).AsyncWaitForCompletion();
                stickyNote2ImageLast.DOFade(0, 1f).AsyncWaitForCompletion();
                await cardFaceCopyImageLast.DOFade(0, 1f).AsyncWaitForCompletion();
                
                Destroy(cardFaceCopy);
                Destroy(stickyNoteCopy);

                // 角色演讲皮肤GameObject淡出
                await characterAnimator.GetComponent<Image>().DOFade(0, 1f).AsyncWaitForCompletion();
            }

            //——————————————————————————————————————————————————————————————————————
            // 正常流程

            // 角色演讲皮肤Animator播对应动画
            characterAnimator.Play("Idle");

            // 角色演讲皮肤GameObject淡入
            await characterAnimator.GetComponent<Image>().DOFade(1, 1f).AsyncWaitForCompletion();
            
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

