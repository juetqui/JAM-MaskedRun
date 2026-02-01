using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class EnvironmentChanger : MonoBehaviour
{
    [SerializeField] private Material forestMaterial;
    [SerializeField] private Material fireMaterial;

    private Material _targetMaterial;
    private WorldType _currentWorld = WorldType.Forest;

    private void Start()
    {
        MinuteChoiceController.Instance.OnChoiceSelected += UpdateWorldType;
        _targetMaterial = fireMaterial;
    }

    private void UpdateWorldType(WorldType worldType)
    {
        RenderSettings.skybox = _targetMaterial;
        
        _currentWorld = worldType;
        _targetMaterial = _currentWorld == WorldType.Forest ? fireMaterial : forestMaterial;
    }
}

public enum WorldType
{
    Forest,
    Fire
}
