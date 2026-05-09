using UnityEngine;

public class LeafSpawner : MonoBehaviour
{
    public GameObject leafPrefab;

    public float spawnRate = 0.2f;

    public Vector3 spawnArea = new Vector3(10, 0, 10);

    public float spawnHeight = 10f;

    private float timer;

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= spawnRate)
        {
            SpawnLeaf();
            timer = 0f;
        }
    }

    void SpawnLeaf()
    {
        Vector3 pos = new Vector3(
            Random.Range(-spawnArea.x, spawnArea.x),
            spawnHeight,
            Random.Range(-spawnArea.z, spawnArea.z)
        );

        Instantiate(leafPrefab, pos, Quaternion.identity);
    }
}