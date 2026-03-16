using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    public GameObject itemPrefab;
    public Transform spawnPoint;

    public void SpawnItem()
    {
        if (itemPrefab == null || spawnPoint == null) return;

        Instantiate(itemPrefab, spawnPoint.position, spawnPoint.rotation);
    }
}