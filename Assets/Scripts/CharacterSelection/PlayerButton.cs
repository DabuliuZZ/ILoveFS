using System;
using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class PlayerButton : NetworkBehaviour
{
    [SerializeField] private GameObject buttonSet1; // 客户端1的按钮组
    [SerializeField] private GameObject buttonSet2; // 客户端2的按钮组
    private Button switchButton;
    private Button confirmButton;
    private Image characterImage;
    private Sprite[] characterSkins;
    private int currentSpriteIndex;
    
    private void Awake()
    {
        buttonSet1 = GameObject.Find("ButtonSet1");
        buttonSet2 = GameObject.Find("ButtonSet2");
    }

    private void Start()
    {
        enabled = false;
    }

    private void OnEnable()
    {
        buttonSet1.SetActive(false);
        buttonSet2.SetActive(false);
        
        var clientId = GetComponent<Player>().clientId;
        
        if (clientId == 1)
        {
            buttonSet1.SetActive(true);
            GetButtonAndCharacter(buttonSet1.transform,"Character1");
        }
        if (clientId == 2)
        {
            buttonSet2.SetActive(true);
            GetButtonAndCharacter(buttonSet1.transform,"Character2");
        }
    }

    private void GetButtonAndCharacter(Transform buttonSet,string character)
    {
        foreach (Transform button in buttonSet)
        {
            if (button.name == "SwitchButton") { switchButton = button.GetComponent<Button>(); }
            if (button.name == "ConfirmButton") { confirmButton = button.GetComponent<Button>(); }
        }
        switchButton.onClick.AddListener(SwitchSkin);
        confirmButton.onClick.AddListener(ConfirmSkin);
        
        characterImage = GameObject.Find(character).GetComponent<Image>();
        characterSkins = GameObject.Find(character).GetComponent<CharacterSkins>().characterSkins;
    }

    private void SwitchSkin()
    {
        currentSpriteIndex = (currentSpriteIndex + 1) % characterSkins.Length;
        characterImage.sprite = characterSkins[currentSpriteIndex];

        CmdSendSpriteChange(currentSpriteIndex);
    }
    
    [Command] public void CmdSendSpriteChange(int newSpriteIndex)
    {
        RpcUpdateSprite(newSpriteIndex);
    }
    
    [ClientRpc] public void RpcUpdateSprite(int newSpriteIndex)
    {
        characterImage.sprite = characterSkins[newSpriteIndex];
    }
    
    private void ConfirmSkin()
    {
        Debug.Log("Confirmed");
    }
}