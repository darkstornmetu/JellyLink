using UnityEngine;

[Injectable]
public class CandyPackManager : MonoBehaviour
{
    private GridManager _gridManager;
    private LevelProperties _levelProperties;
    private IJellyFactory _jellyFactory;
    
    private CandyPack[] _candies;

    [Inject]
    private void Construct(GridManager gridManager, LevelProperties levelProperties, IJellyFactory jellyFactory)
    {
        _gridManager = gridManager;
        _levelProperties = levelProperties;
        _jellyFactory = jellyFactory;
    }

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
