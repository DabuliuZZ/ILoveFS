using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AssetsLoader : MonoBehaviour
{
    public static AssetsLoader instance;
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
        
    }
    
    public List<Gift> gifts;
    
    public List<DisplayingPlayer> displayingPlayers;
    
    
    public Gift GetGift(GiftType type)
    {
        return gifts.First(item => item.giftType == type);
    }
    public DisplayingPlayer GetPlayer(int id)
    {
        return displayingPlayers.First(item => item.id == id);
    }
}
[Serializable]
public class DisplayingPlayer
{
    public int id;
    [HideInInspector]public int score;
    [HideInInspector]public int scoreDown;
    [HideInInspector]public int scoreUp;
    public PlayerScorePanel panel;

    private void UpdateScorePanel()
    {
        panel.UpdateScoreDown(scoreDown);
        panel.UpdateScoreUp(scoreUp);
        panel.UpdateTotalScore(score);
    }

    public void AddGift(GiftType giftType)
    {
        panel.AddGift(giftType);
        var gift = AssetsLoader.instance.GetGift(giftType);
        if (gift.score > 0)
        {
            scoreUp+= gift.score;
        }
        else if(gift.score < 0)
        {
            scoreDown+= gift.score;
        }
        score = scoreUp + scoreDown;
        UpdateScorePanel();
    }

    // public void UpdateScorePanel(GiftType giftType)
    // {
    //     var gift = AssetsLoader.instance.GetGift(giftType);
    //     if (gift.score > 0)
    //     {
    //         scoreUp+= gift.score;
    //     }
    //     else if(gift.score < 0)
    //     {
    //         scoreDown+= gift.score;
    //     }
    //     score = scoreUp + scoreDown;
    //     UpdateScorePanel();
    // }
    
    public void AddGiftNoScore(GiftType giftType)
    {
        panel.AddGift(giftType);
    }
}