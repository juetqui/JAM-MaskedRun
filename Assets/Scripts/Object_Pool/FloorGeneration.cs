using UnityEngine;

public class FloorGeneration : MonoBehaviour
{
    [SerializeField] private GameObject groundPrefab;
    [SerializeField] private int length = 150;
    [SerializeField] private int width = 3;

    private const int Separation = 3;

    private void Awake()
    {
        GenerateFloor();
    }

    private void GenerateFloor()
    {
        for (int z = 0; z < length; z++)
        {
            for (int x = 0; x < width; x++)
            {
                float posX = (x - 1) * Separation;
                var pos = new Vector3(posX, 0, z) + transform.position;

                Instantiate(groundPrefab, pos, Quaternion.identity);
            }
        }
    }
}
