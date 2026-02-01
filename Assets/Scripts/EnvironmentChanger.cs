using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class EnvironmentChanger : MonoBehaviour
{   
    private WorldType _currentWorld = WorldType.Forest;
    private WorldType _targetWorld = WorldType.Fire;

    private void Start()
    {
        PlayerLaneMovement.Instance.OnWorldChanged += UpdateWorldType;
    }

    private void UpdateWorldType()
    {
        // RenderSettings.ambientSkyColor = skyColor;
        // RenderSettings.ambientEquatorColor = equatorColor;
        // RenderSettings.ambientGroundColor = groundColor;
        
        _currentWorld = _targetWorld;
        _targetWorld = _currentWorld == WorldType.Forest ? WorldType.Fire : WorldType.Forest;
    }

    private enum WorldType
    {
        Forest,
        Fire
    }
}
