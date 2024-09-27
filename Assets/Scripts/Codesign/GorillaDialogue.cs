using DG.Tweening;
using UnityEngine;
using Mirror;
using TMPro;
using UnityEngine.UI; // 使用TextMeshPro

public class GorillaDialogueController : NetworkBehaviour
{
    [SerializeField] private GorillaDialogueData dialogueData; // 引用ScriptableObject
    [SerializeField] private Animator gorillaAnimator; // 猩猩Animator引用

    [SerializeField] private Image dialogueBox; // 对话框的UI元素
    [SerializeField] private TMP_Text dialogueText; // 对话文本（TextMeshPro）

    private int currentDialogueIndex = 0;
   
    [SerializeField] private Button nextDialogBtn;
    
    public override void OnStartServer() // 激活并绑定对话切换按钮
    {
        nextDialogBtn.gameObject.SetActive(true);
        nextDialogBtn.onClick.AddListener(NextDialogue);
    }
    
    public void NextDialogue()
    {
        if (currentDialogueIndex < dialogueData.dialogues.Length)
        {
            GorillaDialogue dialogue = dialogueData.dialogues[currentDialogueIndex];
            CmdPlayDialogue(dialogue);
            currentDialogueIndex++;
        }
        else
        {
            // 对话结束，隐藏对话框
            HideDialogue();
        }
    }
    
    [Command(requiresAuthority = false)]
    public void CmdPlayDialogue(GorillaDialogue dialogue)
    {
        RpcPlayDialogue(dialogue);
    }

    [ClientRpc]
    public void RpcPlayDialogue(GorillaDialogue dialogue)
    {
        // 根据对话的暂停状态来处理
        if (dialogue.isPaused)
        {
            HideDialogue(); // 暂停时隐藏对话框
        }
        else
        {
            DisplayDialogue(dialogue.dialogueText); // 显示对话文本
            
            // 设置对话文本的位置
            dialogueText.transform.localPosition = dialogue.dialogueTextPosition; // 更新文本位置
        }

        // 播放动画
        gorillaAnimator.Play(dialogue.animationName);
        
    }

    public void DisplayDialogue(string text)
    {
        dialogueBox.DOFade(1,1f);  // 激活对话框
        dialogueText.text = text; // 显示文本
    }

    public void HideDialogue()
    {
        dialogueBox.DOFade(0,1f); // 隐藏对话框
        dialogueText.text = string.Empty; // 清空文本
    }
    
}
