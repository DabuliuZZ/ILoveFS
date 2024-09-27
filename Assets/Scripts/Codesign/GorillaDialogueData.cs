using UnityEngine;

[System.Serializable]
public class GorillaDialogue
{
    public string dialogueText; // 对话文本
    public string animationName; // 动画名
    public bool isPaused; // 是否暂停
    public Vector3 dialogueTextPosition; // 文本位置XYZ
}

[CreateAssetMenu(fileName = "GorillaDialogueData", menuName = "ScriptableObjects/GorillaDialogueData", order = 1)]
public class GorillaDialogueData : ScriptableObject
{
    public GorillaDialogue[] dialogues; // 存储多个对话
}
