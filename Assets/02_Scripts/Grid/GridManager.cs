using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using ScriptableObjectEvents;
using Sirenix.Utilities;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [SerializeField] private GameEventUniTask _onMoveEvent;
    
    private Grid<BaseGridItem> _gameGrid;
    
    private LevelProperties _levelProperties;
    private AnimationProperties _animationProperties;
    private LinkFactory _linkFactory;
    private JellyFactory _jellyFactory;
    
    private bool _inReaction;

    public void Construct(LevelProperties levelProperties,
        AnimationProperties animationProperties,
        LinkFactory linkFactory,
        JellyFactory jellyFactory)
    {
        _levelProperties = levelProperties;
        _animationProperties = animationProperties;
        _linkFactory = linkFactory;
        _jellyFactory = jellyFactory;
    }

    private void Start()
    {
        InitializeGrid().Forget();
    }

    public void SetItemOnGrid(BaseGridItem item, Vector2Int coords)
    {
        _gameGrid.SetItem(item, coords);
        item.onDestroy += OnItemDestroyed;
    }

    public Jelly GetRandomJelly(Transform parent)
    {
        return _jellyFactory.GetJellyByLevel(_levelProperties.GetRandomLevel(), parent);
    }
    
    public bool Select(Jelly j)
    {
        if (_inReaction) return false;
        
        bool atLeastOneNeighbour = false;
        
        foreach (var direction in sr_directions)
            if (_gameGrid.TryGetItem(j.GridCoords + direction, out BaseGridItem neighbour))
                if (neighbour is Jelly jelly && jelly.Level == j.Level)
                {
                    atLeastOneNeighbour = true;
                    break;
                }


        if (atLeastOneNeighbour)
        {
            _onMoveEvent.Raise(StartReaction(j));
            return true;
        }

        return false;
    }
    
    private async UniTaskVoid InitializeGrid()
    {
        GameLevel level = FindAnyObjectByType<GameLevel>();

        _gameGrid = level.LevelGrid;

        level.GetComponentsInChildren<BaseGridItem>().
            ForEach(b => b.Transform.position += Vector3.forward * 8);
        
        for (int x = 0; x < _gameGrid.Width; x++)
        {
            for (int y = 0; y < _gameGrid.Height; y++)
            {
                Vector2Int coordinates = new Vector2Int(x, y);
                
                if (_gameGrid.GetUnavailability(coordinates)) continue;

                var item = _gameGrid.GetItem(coordinates);
                item.onDestroy += OnItemDestroyed;
                
                Vector3 pos = _gameGrid.GetGridPosition(coordinates);

                if (item is Jelly)
                {
                    Destroy(item.gameObject);

                    Jelly jelly = InstantiateJelly(coordinates, _levelProperties.GetRandomLevel(),
                        pos + Vector3.forward * 8,
                        transform);
                    jelly.Move(pos, _animationProperties.RearrangeTime);
                }
                else
                {
                    item.Move(pos, _animationProperties.RearrangeTime);
                }
            }

            await UniTask.WaitForSeconds(0.1f);
        }
    }
    
    private async UniTask StartReaction(Jelly selectedJelly)
    {
        _inReaction = true;
        
        var selectedJellies = await SelectAllConnectedJellies(selectedJelly);

        await FoldBack(selectedJellies);
        await Unstack(selectedJellies);

        Vector2Int coords = selectedJelly.GridCoords;
        
        Jelly jelly =  InstantiateJelly(coords,selectedJelly.Level + 1, 
            _gameGrid.GetGridPosition(coords),
            transform);
        jelly.Transform.DOScale(Vector3.one, 1).From(Vector3.zero).SetEase(Ease.OutElastic);
        
        await RearrangeGridVertically();
        
        _inReaction = false;
    }

    private async UniTask<List<Jelly>> SelectAllConnectedJellies(Jelly selectedJelly)
    {
        // Initialize a queue to perform breadth-first search
        var jellyQueue = new Queue<Jelly>();
        var connectedJellies = new List<Jelly>();
        jellyQueue.Enqueue(selectedJelly);
        
        int currentDepthLevel = 0;

        // Mark the initially selected jelly as selected
        selectedJelly.Activate();
        connectedJellies.Add(selectedJelly);
        _linkFactory
            .SetLinkMat(_jellyFactory.GetJellyMeshByLevel(selectedJelly.Level).GetJellyMat);
        
        while (jellyQueue.Count > 0)
        {
            // Dequeue all jellies from the current level of the wave
            int currentLevelCount = jellyQueue.Count;
            currentDepthLevel++;
    
            for (int i = 0; i < currentLevelCount; i++)
            {
                Jelly currentJelly = jellyQueue.Dequeue();

                // Select all adjacent jellies that meet the selection criteria
                SelectAdjacentJellies(currentJelly, connectedJellies, jellyQueue, currentDepthLevel);
            }

            // Wait for a short duration before proceeding to the next level of the wave
            await UniTask.WaitForSeconds(_animationProperties.TimeBetweenSelection);
        }

        return connectedJellies;
    }

    private void SelectAdjacentJellies(Jelly selectedJelly, List<Jelly> connectedJellies, 
        Queue<Jelly> jellyQueue, int depthLevel)
    {
        // Get the coordinates and level of the current jelly
        Vector2Int jellyCoordinate = selectedJelly.GridCoords;
        int level = selectedJelly.Level;

        // Iterate over all adjacent cells
        foreach (var direction in sr_directions)
        {
            Vector2Int adjacentCoordinate = jellyCoordinate + direction;
            
            if (!_gameGrid.TryGetItem(adjacentCoordinate, out BaseGridItem item)) continue;
            if (item is not Jelly adjacentJelly || adjacentJelly.Level != level) continue;
            if (connectedJellies.Contains(adjacentJelly)) continue;
            
            selectedJelly.AddToChildList(adjacentJelly);
            
            var link = _linkFactory.EstablishLink(
                selectedJelly.Transform.position,
                adjacentJelly.Transform.position,
                _animationProperties.TimeBetweenSelection);
            
            // Mark the adjacent jelly as selected
            adjacentJelly.Activate(selectedJelly, link, depthLevel,
                _animationProperties.TimeBetweenSelection).Forget();
            connectedJellies.Add(adjacentJelly);
            jellyQueue.Enqueue(adjacentJelly);
        }
    }

    private async UniTask FoldBack(List<Jelly> jellies)
    {
        int maxDepthLevel = jellies.Max(j => j.DepthLevel);
        
        while (maxDepthLevel > 0)
        {
            var currentDepthJellies = 
                jellies.Where(j => j.DepthLevel == maxDepthLevel)
                    .OrderByDescending(j => j.ChildCount);

            List<UniTask> foldTaskList = new();

            foreach (var j in currentDepthJellies)
            {
                foldTaskList.Add(j.GoToRoot(_animationProperties.TimeBetweenFoldOutAnim, 
                    _animationProperties.FoldoutTweenDuration));

                foreach (var direction in sr_cardinalDirections)
                {
                    if (_gameGrid.TryGetItem(j.GridCoords + direction, out BaseGridItem item) && 
                        item is not Jelly) 
                        item.Activate();
                }
            }
            
            maxDepthLevel--;
            await UniTask.WhenAll(foldTaskList);
        }

        var jelly = jellies.First(j => j.DepthLevel == 0);
        
        foreach (var direction in sr_cardinalDirections)
        {
            if (_gameGrid.TryGetItem(jelly.GridCoords + direction, out BaseGridItem item) && 
                item is not Jelly) 
                item.Activate();
        }
    }

    private async UniTask Unstack(List<Jelly> jellies)
    {
        var sortedStack = 
            jellies.OrderByDescending(j => j.Transform.position.y);
        
        jellies.ForEach(j => j.Transform.SetParent(null, true));

        foreach (var jelly in sortedStack)
        {
            await UniTask.WaitForSeconds(_animationProperties.TimeBetweenUnstack);
            jelly.Destroy();
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

                Jelly jelly = InstantiateJelly(coordinates, _levelProperties.GetRandomLevel(), 
                    _gameGrid.GetGridPosition(coordinates) + Vector3.forward * 5,
                    transform);
                jelly.Move(_gameGrid.GetGridPosition(coordinates), _animationProperties.RearrangeTime);
            }
            await UniTask.WaitForSeconds(0.1f);
        }

        await UniTask.WaitForSeconds(_animationProperties.RearrangeTime);
    }
    
    private Jelly InstantiateJelly(Vector2Int coords, int level, Vector3 pos, Transform parent = null)
    {
        var j = _jellyFactory.GetJellyByLevel(level, parent);
        SetItemOnGrid(j, coords);
        j.Transform.position = pos;
        return j;
    }

    private void OnItemDestroyed(Vector2Int coords)
    {
        if (_gameGrid.TryGetItem(coords, out BaseGridItem item))
        {
            _gameGrid.RemoveItemFromGrid(coords);
            item.onDestroy -= OnItemDestroyed;
        }
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
