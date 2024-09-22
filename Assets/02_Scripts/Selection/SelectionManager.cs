using ScriptableObjectEvents;
using UnityEngine;

[Injectable]
public class SelectionManager : MonoBehaviour
{
    [SerializeField] private LayerMask _selectionLayer;
    [SerializeField] private GameEventBool _onCanSelectChanged;
    [SerializeField] private GameEvent _onSelectionFailed;
        
    private Ray _ray;
    private Camera _camera;
    private RaycastHit _hit;

    private bool _canSelect = true;

    private GridManager _gridManager;
    
    [Inject]
    private void Construct(Camera cam, GridManager gridManager)
    {
        _camera = cam;
        _gridManager = gridManager;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && _canSelect && TryGetSelectable(out ISelectable s))
            SelectionProcess(s);
    }

    private void SelectionProcess(ISelectable s)
    {
        if (!_gridManager.Select(s))
            _onSelectionFailed.Raise();
    }
    
    private bool TryGetSelectable(out ISelectable selectable)
    {
        selectable = null;
        _ray = _camera.ScreenPointToRay(Input.mousePosition);

        if (!Physics.Raycast(_ray, out _hit, _camera.farClipPlane, _selectionLayer)) return false;
            
        if (!_hit.collider.TryGetComponent(out ISelectable s)) return false;
        
        selectable = s;
        return s.CanSelect;
    }
        
    private void ChangeCanSelect(bool canSelect)
    {
        _canSelect = canSelect;
    }

    private void OnEnable()
    {
        _onCanSelectChanged.AddListener(ChangeCanSelect);
    }

    private void OnDisable()
    {
        _onCanSelectChanged.RemoveListener(ChangeCanSelect);
    }
}
