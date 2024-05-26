using UnityEngine;

/// <summary>
/// Class for controlling the Head-Stomp-Bonk mechanisms.
/// </summary>
public class HeadBody : MonoBehaviour
{
    [SerializeField] private Collider _headBox;
    [SerializeField] private Collider _bodyBox;
    [SerializeField] private Transform _stompPoint;
    [SerializeField] private Hero _hero;
    [SerializeField] private HeroMovement _movement;

    public Collider Head { get { return _headBox; } }
    public Collider Body { get { return _bodyBox; } }
    public Transform StompPoint {  get { return _stompPoint; } }

    void Start()
    {
        _bodyBox.gameObject.tag = GlobalStrings.HERO_BODY_COLLIDER_TAG;
    }



    // Collisions
    void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.gameObject.tag != GlobalStrings.HERO_BODY_COLLIDER_TAG) return;

        var otherMovement = collision.collider.transform.parent.GetComponentInParent<HeroMovement>();
        var otherHeadBody = otherMovement.GetComponent<HeadBody>();
        // Check if my head is in collision with others' feet
        // (Just using 'this' for clarification
        bool intersect = collision.collider.bounds.Intersects(_headBox.bounds);
        if (intersect &&
            !_movement.IsShoved && otherMovement.IsFalling &&
            otherHeadBody.StompPoint.position.y > _headBox.bounds.center.y)
        {
            otherMovement.OnHitOthersHead(_movement);
            otherMovement.TryBump(new Vector3(-otherMovement.FaceDirection.x, 2f, -otherMovement.FaceDirection.z).normalized, GlobalValues.CHAR_BUMPFORCE * 1.5f);
            _movement.OnHeadHit(otherMovement);
        } else
        {
            // Bump each character backwards
            var collisionDirDot = Vector3.Dot(otherMovement.FaceDirection, _movement.FaceDirection);
            if (collisionDirDot < GlobalValues.CHAR_BUMP_DOT_LIMIT)
                _movement.TryBump(new Vector3(-_movement.FaceDirection.x, 1f, -_movement.FaceDirection.z).normalized, GlobalValues.CHAR_BUMPFORCE);
        }
        
    }

}