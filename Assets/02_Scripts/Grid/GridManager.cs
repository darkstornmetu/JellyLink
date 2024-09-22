using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using ScriptableObjectEvents;
using UnityEngine;

[Injectable]
public class GridManager : MonoBehaviour
{
    [SerializeField] private GameEventUniTask _onMoveEvent;
    [SerializeField] private GameEventVector2Int _onGridItemDestroyed;
    
    private Grid<BaseGridItem> _gameGrid;
    
    private LevelProperties _levelProperties;
    private AnimationProperties _animationProperties;
    
    private ILinkFactory _linkFactory;
    private ICollectableFactory _collectableFactory;
    
    private bool _inReaction;
    
    [Inject]
    private void Construct(LevelProperties levelProperties,
        AnimationProperties animationProperties,
        ILinkFactory linkFactory,
        ICollectableFactory collectableFactory)
    {
        _levelProperties = levelProperties;
        _animationProperties = animationProperties;
        _linkFactory = linkFactory;
        _collectableFactory = collectableFactory;
    }
    
    private void Start()
    {
        InitializeGrid().Forget();
    }
    
    public bool Select(ISelectable s)
    {
        if (s is not CollectableGridItem i) return false;
        
        if (_inReaction) return false;
        
        bool atLeastOneNeighbour = false;
        
        foreach (var direction in sr_directions)
            if (_gameGrid.TryGetItem(i.GridCoords + direction, out BaseGridItem neighbour))
                if (neighbour is CollectableGridItem collectableGridItem && collectableGridItem.Level == i.Level)
                {
                    atLeastOneNeighbour = true;
                    break;
                }
        
        if (atLeastOneNeighbour)
        {
            _onMoveEvent.Raise(StartReaction(i));
            return true;
        }

        return false;
    }
    
    private async UniTaskVoid InitializeGrid()
    {
        GameLevel level = FindAnyObjectByType<GameLevel>();

        _gameGrid = level.LevelGrid;
        
        for (int x = 0; x < _gameGrid.Width; x++)
        {
            for (int y = 0; y < _gameGrid.Height; y++)
            {
                Vector2Int coordinates = new Vector2Int(x, y);
                
                if (_gameGrid.GetUnavailability(coordinates)) continue;

                var item = _gameGrid.GetItem(coordinates);
                
                Vector3 pos = _gameGrid.GetGridPosition(coordinates);

                if (item is CollectableGridItem)
                {
                    Destroy(item.gameObject);

                    var collectable = InstantiateCollectable(coordinates, _levelProperties.GetRandomLevel(),
                        pos + Vector3.forward * 8,
                        transform);
                    collectable.Move(pos, _animationProperties.RearrangeTime);
                }
                else
                {
                    item.Transform.position += Vector3.forward * 8;
                    item.Move(pos, _animationProperties.RearrangeTime);
                }
            }

            await UniTask.WaitForSeconds(0.1f);
        }
    }
    
    private async UniTask StartReaction(CollectableGridItem selectedCollectable)
    {
        _inReaction = true;
        
        var selectedJellies = await SelectAllConnectedJellies(selectedCollectable);

        await FoldBack(selectedJellies);
        await Unstack(selectedJellies);

        Vector2Int coords = selectedCollectable.GridCoords;
        
        var collectable =  InstantiateCollectable(coords,selectedCollectable.Level + 1, 
            _gameGrid.GetGridPosition(coords),
            transform);
        collectable.Transform.DOScale(Vector3.one, 1).From(Vector3.zero).SetEase(Ease.OutElastic);
        
        await RearrangeGridVertically();
        
        _inReaction = false;
    }

    private async UniTask<List<CollectableGridItem>> SelectAllConnectedJellies(CollectableGridItem selectedCollectable)
    {
        // Initialize a queue to perform breadth-first search
        var collectableQueue = new Queue<CollectableGridItem>();
        var connectedCollectables = new List<CollectableGridItem>();
        collectableQueue.Enqueue(selectedCollectable);
        
        int currentDepthLevel = 0;

        // Mark the initially selected collectable as selected
        selectedCollectable.Activate();
        connectedCollectables.Add(selectedCollectable);
       
        _linkFactory
            .SetLinkMat(_collectableFactory.GetCollectableMaterial(selectedCollectable.Level));
        
        while (collectableQueue.Count > 0)
        {
            // Dequeue all collectables from the current level of the wave
            int currentLevelCount = collectableQueue.Count;
            currentDepthLevel++;
    
            for (int i = 0; i < currentLevelCount; i++)
            {
                var currentCollectable = collectableQueue.Dequeue();

                // Select all adjacent collectables that meet the selection criteria
                SelectAdjacentJellies(currentCollectable, connectedCollectables, collectableQueue, currentDepthLevel);
            }

            // Wait for a short duration before proceeding to the next level of the wave
            await UniTask.WaitForSeconds(_animationProperties.TimeBetweenSelection);
        }

        return connectedCollectables;
    }

    private void SelectAdjacentJellies(CollectableGridItem selectedCollectable, 
        List<CollectableGridItem> connectedCollectables, 
        Queue<CollectableGridItem> collectableQueue, int depthLevel)
    {
        // Get the coordinates and level of the current collectable
        Vector2Int coordinate = selectedCollectable.GridCoords;
        int level = selectedCollectable.Level;

        // Iterate over all adjacent cells
        foreach (var direction in sr_directions)
        {
            Vector2Int adjacentCoordinate = coordinate + direction;
            
            if (!_gameGrid.TryGetItem(adjacentCoordinate, out BaseGridItem item)) continue;
            if (item is not CollectableGridItem adjacentCollectable || adjacentCollectable.Level != level) continue;
            if (connectedCollectables.Contains(adjacentCollectable)) continue;
            
            selectedCollectable.AddToChildList(adjacentCollectable);
            
            _linkFactory.EstablishLink(
                selectedCollectable,
                adjacentCollectable,
                _animationProperties.TimeBetweenSelection);
            
            // Mark the adjacent collectable as selected
            adjacentCollectable.Activate(selectedCollectable, depthLevel,
                _animationProperties.TimeBetweenSelection).Forget();
            connectedCollectables.Add(adjacentCollectable);
            collectableQueue.Enqueue(adjacentCollectable);
        }
    }

    private async UniTask FoldBack(List<CollectableGridItem> collectables)
    {
        int maxDepthLevel = collectables.Max(j => j.DepthLevel);
        
        while (maxDepthLevel > 0)
        {
            var currentDepthCollectibles = 
                collectables.Where(c => c.DepthLevel == maxDepthLevel)
                    .OrderByDescending(c => c.ChildCount);

            List<UniTask> foldTaskList = new();

            foreach (var j in currentDepthCollectibles)
            {
                foldTaskList.Add(j.CollectAll(_animationProperties.TimeBetweenFoldOutAnim, 
                    _animationProperties.FoldoutTweenDuration));

                foreach (var direction in sr_cardinalDirections)
                {
                    if (_gameGrid.TryGetItem(j.GridCoords + direction, out BaseGridItem item) && 
                        item is not CollectableGridItem) 
                        item.Activate();
                }
            }
            
            maxDepthLevel--;
            await UniTask.WhenAll(foldTaskList);
        }

        var collectableGridItem = collectables.First(c => c.DepthLevel == 0);
        
        foreach (var direction in sr_cardinalDirections)
        {
            if (_gameGrid.TryGetItem(collectableGridItem.GridCoords + direction, out BaseGridItem item) && 
                item is not CollectableGridItem) 
                item.Activate();
        }
    }

    private async UniTask Unstack(List<CollectableGridItem> collectibles)
    {
        var sortedStack = 
            collectibles.OrderByDescending(c => c.Transform.position.y);
        
        collectibles.ForEach(c => c.Transform.SetParent(null, true));

        foreach (var collectible in sortedStack)
        {
            await UniTask.WaitForSeconds(_animationProperties.TimeBetweenUnstack);
            collectible.Destroy();
        }
    }

    private async UniTask RearrangeGridVertically()
    {
        for (int x = 0; x < _gameGrid.Width; x++)
        {
            for (int y = 0; y < _gameGrid.Height; y++)
            {
                Vector2Int coordinates = new Vector2Int(x, y);

                if (_gameGrid.GetUnavailability(coordinates)) continue;
                if (!_gameGrid.IsEmpty(coordinates)) continue;
                
                bool didFindACandidateInGrid = false;
                
                for (int i = y + 1; i < _gameGrid.Height; i++)
                {
                    Vector2Int lookupCoordinate = new Vector2Int(x, i);
                    
                    if (!_gameGrid.TryGetItem(lookupCoordinate, out BaseGridItem item)) 
                        continue;
                    
                    if (item.IsStatic)
                    {
                        if (item.IsImpervious)
                        {
                            didFindACandidateInGrid = true;
                            break;    
                        }
                        continue;
                    }
                    
                    _gameGrid.MoveItem(lookupCoordinate, coordinates);
                    item.Move(_gameGrid.GetGridPosition(coordinates), _animationProperties.RearrangeTime);
                    didFindACandidateInGrid = true;
                    break;     
                }

                if (didFindACandidateInGrid) continue;

                CollectableGridItem collectable = InstantiateCollectable(coordinates, _levelProperties.GetRandomLevel(), 
                    _gameGrid.GetGridPosition(coordinates) + Vector3.forward * 5,
                    transform);
                collectable.Move(_gameGrid.GetGridPosition(coordinates), _animationProperties.RearrangeTime);
            }
            await UniTask.WaitForSeconds(0.1f);
        }

        await UniTask.WaitForSeconds(_animationProperties.RearrangeTime);
    }
    
    private CollectableGridItem InstantiateCollectable(Vector2Int coords, int level, Vector3 pos, Transform parent = null)
    {
        var collectable = _collectableFactory.CreateCollectableItem(level, parent);
        _gameGrid.SetItem(collectable, coords);
        collectable.Transform.position = pos;
        return collectable;
    }

    private void OnItemDestroyed(Vector2Int coords)
    {
        if (!_gameGrid.TryGetItem(coords, out BaseGridItem item)) return;

        var replacement = item.ReplacementItem;
        
        if (replacement!= null) 
            _gameGrid.SetItem(replacement, coords);
        else
            _gameGrid.RemoveItemFromGrid(coords);
    }

    private void OnEnable()
    {
        _onGridItemDestroyed.AddListener(OnItemDestroyed);
    }

    private void OnDisable()
    {
        _onGridItemDestroyed.RemoveListener(OnItemDestroyed);
    }

    private static readonly Vector2Int[] sr_directions = {
        new(-1, 1),
        new(-1, 0),
        new(-1, -1),
        new(0, 1),
        new(0, -1),
        new(1, 1),
        new(1, 0),
        new(1, -1),
    };

    private static readonly Vector2Int[] sr_cardinalDirections =
    {
        new(-1, 0),
        new(0, 1),
        new(0, -1),
        new(1, 0),
    };
}
