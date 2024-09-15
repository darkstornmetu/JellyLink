using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;

[SelectionBase]
public class Jelly : BaseGridItem, ISelectable
{
    [SerializeField] private TextMeshPro _levelText;
    public int Level => _level;
    public int DepthLevel => _depthLevel;
    public int ChildCount => _childJellies.Count;
    public JellyMesh Mesh => _mesh;
    
    private Jelly _rootJelly;
    private readonly List<Jelly> _childJellies = new ();
    
    private LinkMesh _currentLink;
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

    public async UniTask Activate(Jelly rootJelly, LinkMesh linkMesh, int depthLevel, float delay)
    {
        _rootJelly = rootJelly;
        _depthLevel = depthLevel;
        _currentLink = linkMesh;
        await UniTask.WaitForSeconds(delay);
        Activate();
    }

    public void Destroy()
    {
        //MMVibrationManager.Haptic(HapticTypes.LightImpact);
        onDestroy?.Invoke(GridCoords);
        CallMatchEvent();
        _mesh.Destroy();
        Destroy(gameObject);
    }
    
    public async UniTask GoToRoot(float waitBetween, float tweenDuration)
    {
        if (_currentLink != null)
            Destroy(_currentLink.gameObject);
        
        List<UniTask> taskList = new();
        
        var orderedList = 
            GetAllChildJellies().OrderByDescending(j => j.Transform.position.y);
        
        foreach (var j in orderedList)
        {
            taskList.Add(j.DoPath(_rootJelly.GetCurrentRootPosition(), tweenDuration).ToUniTask());            
            await UniTask.WaitForSeconds(waitBetween);
        }

        await UniTask.WhenAll(taskList);
    }

    private List<Jelly> GetAllChildJellies()
    {
        List<Jelly> jellies = new List<Jelly>() {this};
        
        foreach (var c in _childJellies)
            jellies.AddRange(c.GetAllChildJellies());
        
        return jellies;
    }
    
    public void AddToChildList(Jelly j)
    {
        _childJellies.Add(j);
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
    
    private Vector3 GetCurrentRootPosition()
    {
        _currentHeightCount++;
        Vector3 pos = Transform.position;
        pos.y += _currentHeightCount * s_currentMeshHeight;
        return pos;
    }

    public bool CanSelect { get; set; } = true;
}