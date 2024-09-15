using ScriptableObjectEvents;
using UnityEngine;

public class SelectionManager : MonoBehaviour
{
    [SerializeField] private LayerMask _selectionLayer;
    [SerializeField] private GameEventBool _onCanSelectChanged;
        
    private Ray _ray;
    private Camera _camera;
    private RaycastHit _hit;

    private bool _canSelect = true;

    private GridManager _gridManager;
        
    private void Awake()
    {
        _camera = Camera.main;
        _gridManager = FindAnyObjectByType<GridManager>();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && _canSelect && TryGetJelly(out Jelly j))
            SelectionProcess(j);
    }

    private void SelectionProcess(Jelly j)
    {
        _gridManager.Select(j);
    }
    
    private bool TryGetJelly(out Jelly jelly)
    {
        jelly = null;
        _ray = _camera.ScreenPointToRay(Input.mousePosition);

        if (!Physics.Raycast(_ray, out _hit, _camera.farClipPlane, _selectionLayer)) return false;
            
        if (!_hit.collider.TryGetComponent(out Jelly c)) return false;
        
        jelly = c;
        return c.CanSelect;
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