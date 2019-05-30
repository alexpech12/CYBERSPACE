using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public interface ICyberBlock
{
    void Randomise();
}

public struct CityBlock
{
    public Transform prefab;
    public int x;
    public int y;

    public CityBlock(Transform prefab, int x, int y)
    {
        this.prefab = prefab;
        this.x = x;
        this.y = y;
    }
}

public class CityGenerator : MonoBehaviour
{
    public Transform player;
    public float drawDistance = 100f;
    public float blockSize = 30f;

    public List<Transform> blockPrefabs;

    List<CityBlock> blocks;
    // Start is called before the first frame update
    void Start()
    {
        blocks = new List<CityBlock>();
    }

    // Update is called once per frame
    void Update()
    {
        StartCoroutine("GenerateBlock");
    }

    IEnumerator GenerateBlock()
    {
        if (player == null)
        {
            yield return null;
        }
        // Check player position.
        Vector3 playerPosition = player.position;

        int max_x = Mathf.RoundToInt((playerPosition.x + drawDistance) / blockSize);
        int min_x = Mathf.RoundToInt((playerPosition.x - drawDistance) / blockSize);
        int max_y = Mathf.RoundToInt((playerPosition.z + drawDistance) / blockSize);
        int min_y = Mathf.RoundToInt((playerPosition.z - drawDistance) / blockSize);

        foreach (var block in blocks)
        {
            if (block.x < min_x || block.x > max_x || block.y < min_y || block.y > max_y)
            {
                // Destroy block
                Destroy(block.prefab.gameObject);
            }
        }

        blocks.RemoveAll(block => block.x < min_x || block.x > max_x || block.y < min_y || block.y > max_y);
        

        // Check all grid points within range.
        // 1. Do they currently have a prefab? If not, create one.
        for (int y = min_y; y <= max_y; ++y)
        {
            for (int x = min_x; x <= max_x; ++x)
            {
                // Is this grid point actually in range? (Use circular distance)
                if((player.position - new Vector3(x * blockSize, 0, y * blockSize)).sqrMagnitude > drawDistance*drawDistance)
                {
                    continue;
                }

                bool blockExists = false;
                foreach (var block in blocks)
                {
                    if (block.x == x && block.y == y)
                    {
                        blockExists = true;
                    }
                }
                if (!blockExists)
                {
                    // Create block
                    var newBlock = Instantiate(blockPrefabs[Random.Range(0, blockPrefabs.Count)]);
                    newBlock.localPosition = new Vector3(x * blockSize, 0, y * blockSize);
                    var scripts = newBlock.GetComponentsInChildren<ICyberBlock>();
                    foreach(var s in scripts)
                    {
                        s.Randomise();
                    }
                    blocks.Add(new CityBlock(newBlock, x, y));
                    yield return null;
                }
            }
        }
        
        
    }
}
