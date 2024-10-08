using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class PlayerScorePanel : MonoBehaviour
{
    public TMP_Text scoreUp; 
    public TMP_Text scoreDown;
    public TMP_Text totalScore;
    public RectTransform giftsContainer;
    public Image giftPrefab;
    public List<Gift> gifts = new List<Gift>();

    public void Start()
    {
        scoreUp.text = "0";
        scoreDown.text = "0";
        totalScore.text = "";
    }

    public void UpdateScoreUp(int score)
    {
        scoreUp.text = "+" + score;
    }

    public void UpdateScoreDown(int score)
    {
        scoreDown.text = "-" + score;
    }

    public void UpdateTotalScore(int score)
    {
        totalScore.text="总分："+ score;
    }

    public void AddGift(GiftType giftType)
    {
        if (gifts.All(item => item.giftType != giftType))
        {
            var giftObj = Instantiate<Image>(giftPrefab, giftsContainer);
            var giftText= giftObj.GetComponentInChildren<TMP_Text>();
            Gift gift = new Gift()
            {
                giftType = giftType,
                giftSprite = AssetsLoader.instance.GetGift(giftType).giftSprite,
                num = 1,
                giftNumText = giftText
            };
            giftObj.sprite = gift.giftSprite;
            gift.giftNumText.text = "";
            gifts.Add(gift);
        }
        else
        {
            var target=gifts.First(item => item.giftType == giftType);
            target.num++;
            Debug.Log(gameObject.name);
            target.giftNumText.text = "X"+target.num;
        }
    }
}

[Serializable]
public class Gift
{
    public GiftType giftType;
    public Sprite giftSprite;
    public Button btn;
    public int btnClickedCount;
    
    [HideInInspector]public int num;
    [HideInInspector]public TMP_Text giftNumText;
   
}

public enum GiftType
{
    Flower,
    Heart,
    
    Microphone,
    Speaker,
    
    Shit,
    Slippers,
}