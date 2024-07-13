using UnityEngine;

public class FlockManager : MonoBehaviour
{
    public GameObject birdPrefab;
    public int flockSize = 500;
    public BoxCollider birdBounds;

    void Start()
    {
        for (int i = 0; i < flockSize; i++)
        {
            Vector3 spawnPosition = GetRandomPointInBounds(birdBounds);
            GameObject bird = Instantiate(birdPrefab, spawnPosition, Quaternion.identity);
            bird.GetComponent<Bird>().group = i % 2;
        }
    }

    private static Vector3 GetRandomPointInBounds(BoxCollider boxCollider)
    {
        Bounds bounds = boxCollider.bounds;

        float x = Random.Range(bounds.min.x, bounds.max.x);
        float y = Random.Range(bounds.min.y, bounds.max.y);
        float z = Random.Range(bounds.min.z, bounds.max.z);

        return new Vector3(x, y, z);
    }
}
