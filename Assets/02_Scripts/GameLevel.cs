using Sirenix.OdinInspector;
using UnityEngine;

public class GameLevel : MonoBehaviour
{
    [SerializeField, ReadOnly] private Grid<BaseGridItem> _levelGrid;

    public Grid<BaseGridItem> LevelGrid => _levelGrid;
    //Only called from GridManagerEditor
    
    public void SetGrid(Grid<BaseGridItem> givenGrid)
    {
        _levelGrid = givenGrid;
    }
}