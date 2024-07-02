using Mirror;
using UnityEngine;

public class PlayerButton : NetworkBehaviour
{
    [SerializeField] private GameObject buttonSet1; // 客户端1的按钮组
    [SerializeField] private GameObject buttonSet2; // 客户端2的按钮组
    
    private void Awake()
    {
        buttonSet1 = GameObject.Find("ButtonSet1");
        buttonSet2 = GameObject.Find("ButtonSet2");
    }

    private void Start()
    {
        buttonSet1.SetActive(false);
        buttonSet2.SetActive(false);
        
        var clientId = GetComponent<Player>().clientId;
        
        if (clientId == 1)
        {
            buttonSet1.SetActive(true);
        }
        if (clientId == 2)
        {
            buttonSet2.SetActive(true);
        }
    }
}
