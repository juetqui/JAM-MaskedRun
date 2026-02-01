using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class EnvironmentChanger : MonoBehaviour
{
    [SerializeField] private Material forestMaterial;
    [SerializeField] private Material fireMaterial;

    private Material _targetMaterial;
    private WorldType _currentWorld = WorldType.Forest;
    private WorldType _targetWorld = WorldType.Fire;

    private void Start()
    {
        PlayerLaneMovement.Instance.OnWorldChanged += UpdateWorldType;
        _targetMaterial = fireMaterial;
    }

    private void UpdateWorldType()
    {
        RenderSettings.skybox = _targetMaterial;
        
        _currentWorld = _targetWorld;
        _targetWorld = _currentWorld == WorldType.Forest ? WorldType.Fire : WorldType.Forest;
        _targetMaterial = _currentWorld == WorldType.Forest ? fireMaterial : forestMaterial;
    }

    private enum WorldType
    {
        Forest,
        Fire
    }
}
