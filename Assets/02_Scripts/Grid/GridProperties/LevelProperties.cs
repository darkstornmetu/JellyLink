using UnityEngine;

[CreateAssetMenu(menuName = "JellyLink/LevelProperties", fileName = "LevelProperties")]
public class LevelProperties : ScriptableObject
{
    [SerializeField] private int _minLevel;
    [SerializeField] private int _maxLevel;
    [SerializeField] private AnimationCurve _probabilityCurve;

    public int GetRandomLevel()
    {
        return Mathf.RoundToInt(Mathf.Lerp(_minLevel, _maxLevel, 
            _probabilityCurve.Evaluate(Random.value)));  
    }

    public int GetRandomLevel(int minLevel, int maxLevel)
    {
        minLevel = Mathf.Max(_minLevel, minLevel);
        maxLevel = Mathf.Min(_maxLevel, maxLevel);
        
        return Mathf.RoundToInt(Mathf.Lerp(minLevel, maxLevel, 
            _probabilityCurve.Evaluate(Random.value)));
    }
}