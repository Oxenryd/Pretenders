using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Transitions : MonoBehaviour
{
    [SerializeField] private TransitionType _transitionType = TransitionType.FadeToBlack;
    [SerializeField][Range(0f, 1f)] private float _value = 0f;

    [ExecuteInEditMode]
    void Update()
    {
        Debug.Log("Running");
    }
}
