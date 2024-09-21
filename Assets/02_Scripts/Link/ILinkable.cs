using System;
using UnityEngine;

public interface ILinkable
{
    public Vector3 Pos { get; }
    public event Action onDestroyLink;
    public void DestroyLink();
}