using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneSettings : MonoBehaviour, IScene
{
    [SerializeField] private ControlSchemeType _controlScheme;

    public ControlSchemeType ControlScheme
    { get { return _controlScheme; } }
}
