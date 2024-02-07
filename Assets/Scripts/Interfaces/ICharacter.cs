using UnityEngine;

public interface ICharacter
{
    public int Index { get; }
    public ICharacterMovement Movement { get; }
    public GameObject gameObject { get; }
    public bool Playable { get; }
}
