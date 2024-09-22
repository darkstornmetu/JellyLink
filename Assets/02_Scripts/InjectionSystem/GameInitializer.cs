using System;
using System.Collections.Generic;
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
        ServiceContainer.Register<ICollectableFactory>(_jellyFactory);
        ServiceContainer.Register<IJellyFactory>(_jellyFactory);
        ServiceContainer.Register<ILinkFactory>(_linkFactory);
        ServiceContainer.Register(_gridManager);
        ServiceContainer.Register(_selectionManager);
        ServiceContainer.Register(_mainCamera);
        ServiceContainer.Register(_levelProperties);
        ServiceContainer.Register(_animationProperties);
        
        InjectDependenciesInScene();
    }

    private void InjectDependenciesInScene()
    {
        var injectables = InjectableMonoBehaviours();
        
        foreach (var injectable in injectables) 
            ServiceContainer.InjectDependencies(injectable);
    }

    private IEnumerable<MonoBehaviour> InjectableMonoBehaviours()
    {
        var allMonoBehaviours = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
        
        foreach (var monoBehaviour in allMonoBehaviours)
        {
            var type = monoBehaviour.GetType();
            if (Attribute.IsDefined(type, typeof(InjectableAttribute)))
            {
                yield return monoBehaviour;
            }
        }
    }
}