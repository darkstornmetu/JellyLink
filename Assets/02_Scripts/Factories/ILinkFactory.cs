using UnityEngine;

public interface ILinkFactory
{
    public void EstablishLink(ILinkable from, ILinkable to, float duration);
    public void SetLinkMat(Material mat);
}