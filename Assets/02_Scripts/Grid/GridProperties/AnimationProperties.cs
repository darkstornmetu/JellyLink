using UnityEngine;

[CreateAssetMenu(menuName = "JellyLink/AnimationProperties", fileName = "AnimationProperties")]
public class AnimationProperties : ScriptableObject
{
    public float TimeBetweenSelection = 0.1f;
    public float TimeBetweenFoldOutAnim = 0.05f;
    public float FoldoutTweenDuration = 0.15f;
    public float TimeBetweenUnstack = 0.03f;
    public float RearrangeTime = 0.3f;
}
