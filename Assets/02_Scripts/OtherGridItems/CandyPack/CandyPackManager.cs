using UnityEngine;

public class CandyPackManager : MonoBehaviour
{
    [SerializeField] private GridManager _gridManager;
    [SerializeField] private JellyFactory _jellyFactory;
    [SerializeField] private LevelProperties _levelProperties;
    
    private CandyPack[] _candies;

    private void Awake()
    {
        _candies = FindObjectsByType<CandyPack>(FindObjectsSortMode.None);
    }

    private void Start()
    {
        InitializeCandies();
    }

    private void InitializeCandies()
    {
        foreach (var candy in _candies)
        {
            var jelly = _jellyFactory.GetJellyByLevel(_levelProperties.GetRandomLevel(), candy.transform);
            candy.SetJellyInsideThisCandy(jelly);
        }
    }
}
