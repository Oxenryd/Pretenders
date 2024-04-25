using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingHeadlight : MonoBehaviour
{
    public Vector3 corner1;
    public Vector3 corner2;
    public Vector3 corner3;
    public Vector3 corner4;

    public float moveSpeed = 5F;

    private Vector3 targetPosition;

    void Start()
    {
        // Set initial target posirion
        SetRandomTargetPosition();
    }

    void Update()
    {
        // Move towards the next pos
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

        // Check if the object has reached the target position
        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            // Set a new random target position
            SetRandomTargetPosition();
        }
    }

    void SetRandomTargetPosition()
    {
        // Decide where the next randomized pos will be inside the area
        float randomX = Random.Range(Mathf.Min(corner1.x, corner2.x), Mathf.Max(corner3.x, corner4.x));
        float randomZ = Random.Range(Mathf.Min(corner1.z, corner4.z), Mathf.Max(corner2.z, corner3.z));

        targetPosition = new Vector3(randomX, transform.position.y, randomZ);
    }
}
