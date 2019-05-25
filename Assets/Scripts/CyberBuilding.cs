using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CyberBuilding : MonoBehaviour, ICyberBlock
{
    public int height = 3;
    public int width = 3;
    public int depth = 3;

    public float pieceSize = 2.0f;
    public bool randomize = false;
    public bool generateOnUpdate = false;
    public int seed = 0;

    public CyberBuildingFloor floorPrefab;

    public List<Transform> prefabs;
    public List<Material> materials;

    public Material initialMaterial;
    public Material finalMaterial;

    // Start is called before the first frame update
    void Start()
    {

        GetComponent<BoxCollider>().center = new Vector3(width * pieceSize / 2, height * pieceSize / 2, depth * pieceSize / 2);
        GetComponent<BoxCollider>().size = new Vector3(width * pieceSize, height * pieceSize, depth * pieceSize);
    }
    bool coroutineRunning = true;
    // Update is called once per frame
    void Update()
    {
        if (coroutineRunning)
        {
            coroutineRunning = false;
            StartCoroutine("Generate");
        }
    }

    public IEnumerator Generate()
    {
        Random.InitState(seed);
        if(randomize)
        {
            Random.InitState((int)System.DateTime.Now.Ticks);
        }
        if(prefabs.Count == 0)
        {
            yield return null;
        }

        for(int h = 0; h < height; ++h)
        {
            int heightScale = Random.Range(1, 4);
                
            Vector3 scale = new Vector3(1, heightScale, 1);

            var newFloor = Instantiate(floorPrefab.transform, transform);
            newFloor.localPosition = new Vector3(0, h * pieceSize, 0);
            newFloor.localScale = scale;
            var floor = newFloor.GetComponent<CyberBuildingFloor>();
            floor.width = width;
            floor.depth = depth;
            floor.prefabs = prefabs;
            floor.initialMaterial = initialMaterial;
            floor.finalMaterial = finalMaterial;
            floor.Init();
            
            h += heightScale-1;
            yield return null;
        }
        coroutineRunning = false;
    }

    void ICyberBlock.Randomise()
    {
        height = Random.Range(1, 50);
        finalMaterial = materials[Random.Range(0, materials.Count)];
    }
}
