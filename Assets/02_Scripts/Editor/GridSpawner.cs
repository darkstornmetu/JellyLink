using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GridSpawner : EditorWindow
{
    [MenuItem("Tools/Grid Spawner")]
    static void Init() => GetWindow<GridSpawner>();

    private Grid<GridItem> _itemGrid;
    
    private float _buttonSize;

    private Vector2Int _gridSize = new Vector2Int(5, 7);
    private Vector2Int _lastGridSize = new Vector2Int(5, 7);
    private float _spacingX = 0.6f;
    private float _spacingY = 0.6f;
    
    private string _savePath;
    
    private List<BaseGridItem> _prefabList; // Store the loaded prefabs
    private Vector2 _scrollPosition;

    private BaseGridItem _defaultGridItem;
    private BaseGridItem _selectedGridItem;
    
    private bool _unavailabilityMode;
    
    private void OnGUI()
    {
        EditorGUIUtility.wideMode = true;

        EditorGUI.BeginChangeCheck();
        GUILayout.Label("Grid Settings", EditorStyles.boldLabel);
        
        _gridSize.x = EditorGUILayout.IntSlider("Grid Size X", _gridSize.x, 1, 20);
        _gridSize.y = EditorGUILayout.IntSlider("Grid Size Y", _gridSize.y, 1, 20);
        
        if (EditorGUI.EndChangeCheck())
        {
            UpdateGridContent();
            _lastGridSize = _gridSize;
        }
        
        EditorGUILayout.Space();

        _spacingX = EditorGUILayout.FloatField("Spacing X", _spacingX);
        _spacingY = EditorGUILayout.FloatField("Spacing Y", _spacingY);
        
        EditorGUILayout.Space();
        
        _defaultGridItem = (BaseGridItem)EditorGUILayout.ObjectField("Default Grid Item",
            _defaultGridItem, typeof(BaseGridItem), false);
        
        EditorGUILayout.Space();
        
        using (new EditorGUILayout.HorizontalScope())
        {
            DropArea();
            
            using (new EditorGUILayout.VerticalScope())
            {
                 GUILayout.Label("Unavailability", EditorStyles.boldLabel,
                     GUILayout.Width(80));
                if (GUILayout.Button("X", GUILayout.Width(80), GUILayout.ExpandHeight(true))) 
                    _unavailabilityMode = true; // Toggle availability mode
            }
        }
        
        EditorGUILayout.Space();
        
        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Spawn Selected Pieces"))
                SpawnSelectedPieces(_itemGrid);

            if (GUILayout.Button("Reset Grid"))
            {
                _itemGrid = new Grid<GridItem>(_gridSize.x, _gridSize.y);
                UpdateGridContent();
            }
        }

        if (GUILayout.Button("Copy Grid From Scene"))
            CopyGridFromScene();
        
        EditorGUILayout.Space();

        DrawGrid();
    }
    
    private void DropArea()
    {
        using (new EditorGUILayout.VerticalScope())
        {
            GUILayout.Label("Prefab Preview and Drop Area", EditorStyles.boldLabel);
            var dropAreaStyle = new GUIStyle(GUI.skin.box) {margin = new RectOffset(10, 10, 10, 10)};
    
            // Clamp the height of the drop area to not exceed a certain portion of the window
            //float dropAreaHeight = Mathf.Min(Mathf.Max(50, position.height - 200), position.height * 0.15f);
            float dropAreaHeight = 90;
            var dropArea = GUILayoutUtility.GetRect(0, dropAreaHeight, 
                GUILayout.ExpandWidth(true));
            
            GUI.Box(dropArea, "Drop Prefabs Here", dropAreaStyle);

            _scrollPosition = GUI.BeginScrollView(dropArea, _scrollPosition, 
                new Rect(0, 0, dropArea.width - 20, 
                Mathf.Max(dropArea.height, _prefabList.Count * 75)));
            
            float x = 5;
            float y = 5;

            int counter = 0;
            
            foreach (var prefab in _prefabList)
            {
                if (GUI.Button(new Rect(x, y, 80, 80),
                        new GUIContent(GetAssetPreview(prefab), prefab.name)))
                {
                    _unavailabilityMode = false;
                    _selectedGridItem = prefab;
                }

                counter++;

                if (counter >= 5)
                {
                    x = 5;
                    y += 85;
                    counter = 0;
                }
                else
                    x += 85;
            }

            GUI.EndScrollView();

            HandleDragAndDrop(dropArea);
        }

    }

    private void UpdateGridContent()
    {
        Grid<GridItem> prevGrid = _itemGrid;
        Vector2Int prevGridSize = _lastGridSize;
        
        _itemGrid = new Grid<GridItem>(_gridSize.x, _gridSize.y);

        for (int y = 0; y < _gridSize.y; y++)
        {
            for (int x = 0; x < _gridSize.x; x++)
            {
                Vector2Int coords = new Vector2Int(x, y);

                if (prevGrid != null && x <= prevGridSize.x && y <= prevGridSize.y
                    && prevGrid.TryGetItem(coords, out GridItem item))
                {
                    _itemGrid.SetItem(item, coords);
                    _itemGrid.SetUnavailability(coords, prevGrid.GetUnavailability(coords));
                    _itemGrid.SetGridPosition(coords, prevGrid.GetGridPosition(coords));
                }
                else
                {
                    GUIContent content = _defaultGridItem != null
                        ? new GUIContent(GetAssetPreview(_defaultGridItem), _defaultGridItem.name)
                        : new GUIContent();
                                    
                    _itemGrid.SetItem(new GridItem(_defaultGridItem, content), 
                        coords);
                    _itemGrid.SetUnavailability(coords, false);
                    _itemGrid.SetGridPosition(coords, Vector3.zero);
                }
            }
        }
    }

    private void CopyGridFromScene()
    {
        var level = FindAnyObjectByType<GameLevel>();

        if (level)
        {
            var baseItemGrid = level.LevelGrid;

            int width = baseItemGrid.Width;
            int height = baseItemGrid.Height;
            
            _itemGrid = new Grid<GridItem>(width, height);
            _gridSize.x = width;
            _gridSize.y = height;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Vector2Int coords = new Vector2Int(x, y);

                    if (baseItemGrid.TryGetItem(coords, out BaseGridItem item))
                    {
                        var path = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(item);
                        
                        BaseGridItem prefabMatch = _prefabList.Find(i =>
                            path ==
                            AssetDatabase.GetAssetPath(i));
                        
                        _itemGrid.SetItem(new GridItem(prefabMatch, new GUIContent(item.IconTexture)), 
                            coords);
                        _itemGrid.SetGridPosition(coords, baseItemGrid.GetGridPosition(coords));
                        _itemGrid.SetUnavailability(coords,false);
                    }
                    else
                    {
                        _itemGrid.SetItem(new GridItem(null, new GUIContent("X")), 
                            coords);
                        _itemGrid.SetGridPosition(coords, baseItemGrid.GetGridPosition(coords));
                        _itemGrid.SetUnavailability(coords, true);
                    }
                }
            }
        }
        
        UpdateGridContent();
    }

    private void DrawGrid()
    {
        float horizontalOffset = Mathf.Max(0, (position.width - _buttonSize * _gridSize.x) * 0.5f);

        GUILayout.BeginVertical();
        // Calculate the vertical space needed for the grid and other elements
        float verticalSpace = position.height - 320; // Adjust this value as needed
        float maxButtonSize = Mathf.Min((verticalSpace) / _gridSize.y, position.width / _gridSize.x);

        // Calculate the button size dynamically based on available space
        _buttonSize = Mathf.Min(maxButtonSize, 80);
        
        for (int y = _gridSize.y - 1; y >= 0; y--)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(horizontalOffset);
            
            for (int x = 0; x < _gridSize.x; x++)
            {
                Vector2Int coords = new Vector2Int(x, y);

                GUIContent content;
                
                if (_itemGrid.TryGetItem(coords, out GridItem item))
                    content = item.GUIContent;
                else
                    content = new GUIContent();
                
                if (GUILayout.Button(content, 
                        GUILayout.Width(_buttonSize), 
                        GUILayout.Height(_buttonSize)))
                {
                    if (_unavailabilityMode)
                    {
                        content = new GUIContent("X");
                        _itemGrid.SetItem(new GridItem(null, content), coords);
                        _itemGrid.SetUnavailability(coords, true);
                    }
                    else if(_selectedGridItem)
                    {
                        content = new GUIContent(GetAssetPreview(_selectedGridItem), _selectedGridItem.name);
                        
                        _itemGrid.SetItem(new GridItem(_selectedGridItem, 
                            content), coords);
                        _itemGrid.SetUnavailability(coords, false);
                    }
                }
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        GUILayout.EndVertical();
    }

    private void SpawnSelectedPieces(Grid<GridItem> pieceGrid)
    {
        if (string.IsNullOrWhiteSpace(_savePath)) 
            _savePath = EditorUtility.OpenFolderPanel("Select Save Path", "", "");

        GameLevel currentLevel = new GameObject(SceneManager.GetActiveScene().name).AddComponent<GameLevel>();

        Grid<BaseGridItem> levelGrid = new Grid<BaseGridItem>(_gridSize.x, _gridSize.y);

        for (int x = 0; x < _gridSize.x; x++)
        {
            for (int y = 0; y < _gridSize.y; y++)
            {
                var currentPos = new Vector3(
                    -(_gridSize.x - 1) * _spacingX + x * _spacingX,
                    0,
                    y * _spacingY);

                currentPos.x += (_gridSize.x - 1) * _spacingX / 2f;
                currentPos.z -= (_gridSize.y - 1) * _spacingY / 2f;

                Vector2Int coords = new Vector2Int(x, y);

                if (_itemGrid.GetUnavailability(coords))
                {
                    levelGrid.SetUnavailability(coords, true);
                    continue;
                }
                
                var currentItem = pieceGrid.GetItem(coords).Item;
                
                var item = PrefabUtility.InstantiatePrefab(currentItem, currentLevel.transform) as BaseGridItem;
                item.Transform.localPosition = currentPos;
                
                levelGrid.SetUnavailability(coords, false);
                levelGrid.SetGridPosition(coords, currentPos);
                levelGrid.SetItem(item, coords);
                
                item.name = "( " + x + ", " + y + " )";
                EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            }
        }
        
        currentLevel.SetGrid(levelGrid);
        
        PrefabUtility.InstantiatePrefab(
            PrefabUtility.SaveAsPrefabAsset(currentLevel.gameObject, 
                _savePath + "/" + currentLevel.name + "_" + GUID.Generate() + ".prefab"));
        
        DestroyImmediate(currentLevel.gameObject);
    }
    
    private void HandleDragAndDrop(Rect dropArea)
    {
        Event evt = Event.current;

        switch (evt.type)
        {
            case EventType.DragUpdated:
            case EventType.DragPerform:
                if (!dropArea.Contains(evt.mousePosition))
                    return;

                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                if (evt.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();

                    foreach (GameObject draggedObject in DragAndDrop.objectReferences.OfType<GameObject>())
                    {
                        var baseGridItem = draggedObject.GetComponent<BaseGridItem>();
                        if (baseGridItem != null && !_prefabList.Contains(baseGridItem))
                        {
                            _prefabList.Add(baseGridItem);
                        }
                    }
                    Event.current.Use();
                }
                break;
        }
    }
    
    private Texture2D GetAssetPreview(BaseGridItem item)
    {
        return item.IconTexture;
    }

    private void Awake()
    {
        _prefabList = new List<BaseGridItem>();
        
        string[] guids = AssetDatabase.FindAssets("t:Prefab");
        
        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            
            if (prefab != null && prefab.TryGetComponent(out BaseGridItem baseGridItem)) 
                _prefabList.Add(baseGridItem);
        }

        _defaultGridItem = _prefabList.FirstOrDefault(x => x is Jelly);
        
        _itemGrid = new Grid<GridItem>(_gridSize.x, _gridSize.y);

        for (int x = 0; x < _gridSize.x; x++)
        {
            for (int y = 0; y < _gridSize.y; y++)
            {
                Vector2Int coords = new Vector2Int(x, y);
                _itemGrid.SetItem(new GridItem(_defaultGridItem, 
                         new GUIContent(GetAssetPreview(_defaultGridItem))),
                    coords);
            }
        }
        
        UpdateGridContent();
    }
    
    [Serializable]
    private class GridItem : IGridItem
    {
        public BaseGridItem Item;
        public GUIContent GUIContent;

        public GridItem(BaseGridItem item, GUIContent content)
        {
            Item = item;
            GUIContent = content;
        }
        
        public Vector2Int GridCoords
        {
            get => Item.GridCoords;
            set => Item.GridCoords = value;
        }
    }
}

