using UnityEngine;

public interface IDraggable
{
    public bool IsDraggedByOther { get; }
    public GameObject GameObject { get; }
    public void StartTug(Tug tug);
}
