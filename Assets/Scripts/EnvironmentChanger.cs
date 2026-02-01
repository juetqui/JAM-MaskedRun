using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class EnvironmentChanger : MonoBehaviour
{
    [SerializeField] private Material forestSkybox;
    [SerializeField] private Material fireSkybox;

    private Material _targetSkyBox;
    
    private WorldType _currentWorld = WorldType.Forest;
    private WorldType _targetWorld = WorldType.Fire;

    private void Start()
    {
        PlayerLaneMovement.Instance.OnWorldChanged += UpdateWorldType;
    }

    private void UpdateWorldType()
    {
        RenderSettings.skybox = _targetSkyBox;
        
        _currentWorld = _targetWorld;
        _targetWorld = _currentWorld == WorldType.Forest ? WorldType.Fire : WorldType.Forest;
        _targetSkyBox = _currentWorld == WorldType.Forest ? fireSkybox : forestSkybox;
    }

    private enum WorldType
    {
        Forest,
        Fire
    }
}
