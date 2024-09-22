public interface ISelectable
{
    public bool CanSelect { get; set; }
    public void Activate();
}