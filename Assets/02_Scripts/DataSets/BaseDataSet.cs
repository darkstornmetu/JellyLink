using System;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

public abstract class BaseDataSet<TEnum, TData> : ScriptableObject 
    where TEnum : Enum
{
    [SerializeField, 
     ListDrawerSettings(HideAddButton = true, HideRemoveButton = true, DraggableItems = false, ShowFoldout = false)]
    private BaseDataParent[] _baseDataArray;

    private void Reset()
    {
        var enumValues = Enum.GetValues(typeof(TEnum)) as TEnum[];
        int count = enumValues.Length;

        _baseDataArray = new BaseDataParent[count];

        for (int i = 0; i < count; i++) 
            _baseDataArray[i] = new BaseDataParent(enumValues[i]);
    }
    
    public TData GetDataByEnum(TEnum enumValue)
    {
        return _baseDataArray.First(x => Equals(x.Type, enumValue)).Data;
    }
    
    [Serializable]
    private class BaseDataParent
    {
        [ReadOnly] 
        public TEnum Type;
        public TData Data;

        public BaseDataParent(TEnum type)
        {
            Type = type;
        }
    }
}