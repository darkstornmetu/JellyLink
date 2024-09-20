using UnityEngine;

public class GameInitializer : MonoBehaviour
{
    [Header("Inject Scene References")]
    [SerializeField] private GridManager _gridManager;
    [SerializeField] private SelectionManager _selectionManager;
    [SerializeField] private LinkFactory _linkFactory;
    [SerializeField] private JellyFactory _jellyFactory;
    [SerializeField] private Camera _mainCamera;

    [Header("Inject Data References")]
    [SerializeField] private LevelProperties _levelProperties;
    [SerializeField] private AnimationProperties _animationProperties;
    
    private void Awake()
    {
        _selectionManager.Construct(_mainCamera, _gridManager);
        _gridManager.Construct(_levelProperties, _animationProperties, _linkFactory, _jellyFactory);
    }
}
