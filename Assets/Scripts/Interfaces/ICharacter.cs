using UnityEngine;

public interface ICharacter
{
    public int Index { get; set; }
    public GameObject gameObject { get; }
    public ICharacterMovement Movement { get; }
    public bool Playable { get; }
}
