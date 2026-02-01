using System;
using UnityEngine;

public class WorldChanger : MonoBehaviour
{
    [SerializeField] private GameObject _forestPrefab;
    [SerializeField] private GameObject _firePrefab;

    private void Start()
    {
        MinuteChoiceController.Instance.OnChoiceSelected += ChangeWorld;
        ChangeWorld(WorldType.Forest);
    }

    private void ChangeWorld(WorldType worldType)
    {
        if (worldType == WorldType.Forest)
        {
            _forestPrefab.SetActive(true);
            _firePrefab.SetActive(false);
        }
        else
        {
            _forestPrefab.SetActive(false);
            _firePrefab.SetActive(true);
        }
    }
}
