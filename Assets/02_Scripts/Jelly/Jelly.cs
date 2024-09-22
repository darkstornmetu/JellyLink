using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;

[SelectionBase]
public class Jelly : CollectableGridItem
{
    [SerializeField] private TextMeshPro _levelText;

    public override bool CanSelect { get; set; } = true;
    
    public override Vector3 Pos => Transform.position;
    
    public override int Level => _level;
    public override int DepthLevel => _depthLevel;
    public override int ChildCount => _childJellies.Count;
    
    private ICollectableGridItem _rootJelly;
    private readonly List<ICollectableGridItem> _childJellies = new ();
    
    private JellyMesh _mesh;
    
    private static float s_currentMeshHeight;
    private int _currentHeightCount;
        
    private int _level;
    private int _depthLevel;
    
    public void SetupJelly(int level, JellyMesh currentMesh)
    {
        _level = level;
        _levelText.text = level.ToString();
        _mesh = currentMesh;
        s_currentMeshHeight = currentMesh.GetMeshHeight;
    }
    
    public override void Activate()
    {
        _tr.DOLocalMoveY(0.1f, 0.1f);
        //MMVibrationManager.Haptic(HapticTypes.LightImpact);
    }

    public override async UniTask Activate(ICollectableGridItem rootJelly, int depthLevel, float delay)
    {
        _rootJelly = rootJelly;
        _depthLevel = depthLevel;
        await UniTask.WaitForSeconds(delay);
        Activate();
    }

    public override void Destroy()
    {
        CallDestroyEvent(GridCoords);
        CallMatchEvent();
        _mesh.Destroy();
        Destroy(gameObject);
    }
    
    public override async UniTask CollectAll(float waitBetween, float tweenDuration)
    {
        DestroyLink();
        List<UniTask> taskList = new();
        
        var orderedList = 
            GetAllChildItems().OrderByDescending(j => j.Transform.position.y);
        
        foreach (var item in orderedList)
        {
            taskList.Add(item.GoToRoot(_rootJelly.GetCurrentRootPosition(), 
                tweenDuration));            
            await UniTask.WaitForSeconds(waitBetween);
        }

        await UniTask.WhenAll(taskList);
    }

    public override List<ICollectableGridItem> GetAllChildItems()
    {
        List<ICollectableGridItem> jellies = new() {this};
        
        foreach (var c in _childJellies)
            jellies.AddRange(c.GetAllChildItems());
        
        return jellies;
    }
    
    public override void AddToChildList(ICollectableGridItem j)
    {
        _childJellies.Add(j);
    }
    
    public override Vector3 GetCurrentRootPosition()
    {
        _currentHeightCount++;
        Vector3 pos = Transform.position;
        pos.y += _currentHeightCount * s_currentMeshHeight;
        return pos;
    }

    public override UniTask GoToRoot(Vector3 pos, float duration)
    {
        return DoPath(pos, duration).ToUniTask();
    }
    
    private Tween DoPath(Vector3 pos, float duration)
    {
        //MMVibrationManager.Haptic(HapticTypes.LightImpact);
        
        Sequence seq = DOTween.Sequence();

        Vector3 startPos = _tr.position;
        Vector3 finalPos = pos;
        Vector3 middlePos = Vector3.Lerp(startPos, finalPos, 0.5f);
        middlePos.y += 0.5f;
        
        seq.Append(_tr.DOPath(new Vector3[] {startPos, middlePos, finalPos}, duration, PathType.CatmullRom));
        seq.Join(_mesh.RotateTween(duration));

        return seq;
    }
}