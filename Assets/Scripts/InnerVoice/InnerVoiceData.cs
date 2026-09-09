using UnityEngine;

[CreateAssetMenu(menuName = "InnerVoice/InnerVoiceData")]
public class InnerVoiceData : ScriptableObject
{
    [TextArea] public string[] lines;
}
