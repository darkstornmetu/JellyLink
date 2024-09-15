using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ScriptableObjectEvents
{
    public abstract class GameEventGeneric<T> : ScriptableObject
    {
#if UNITY_EDITOR
        [PropertySpace(8,8)]
        [TitleGroup("Description", default, TitleAlignments.Centered, false, true, false, 0)]
        [HideLabel] [SerializeField] [TextArea] private string description;
    
        [ShowInInspector] [ToggleLeft] private bool showListeners;

        [ShowIf("showListeners")]
        [ShowInInspector] [OnValueChanged("ReorderInvocations")] [ShowIf("showListeners")]
        private List<Action<T>> listenerList = new();
#endif
    
        private event Action<T> _event;

        public void Raise(T t)
        {
            _event?.Invoke(t);
        }
    
        public void AddListener(Action<T> method)
        {
            _event += method;
        
#if UNITY_EDITOR
            listenerList.Add(method);
#endif
        }

        public void RemoveListener(Action<T> method)
        {
            _event -= method;
        
#if UNITY_EDITOR
            listenerList.Remove(method);
#endif
        }

#if UNITY_EDITOR
        [PropertySpace(8,8)]
        [Button(ButtonSizes.Medium)]
        private void TestTheEvent(T t)
        {
            Raise(t);
        }
    
        private void ReorderInvocations()
        {
            _event = null;
            GC.Collect();
        
            listenerList.ForEach(action => _event += action);
        }
#endif
    }
}