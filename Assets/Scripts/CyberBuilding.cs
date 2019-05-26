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

        GetComponent<MeshRenderer>().sharedMaterial = finalMaterial;
    }
    bool coroutineRunning = true;
    bool coroutineDone = false;
    bool combineDone = false;
    // Update is called once per frame
    void Update()
    {
        if (coroutineRunning)
        {
            coroutineRunning = false;
            StartCoroutine("Generate");
        }
        StartCoroutine("Combine");
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
            //int heightScale = Random.Range(1, 10);
            int heightScale = 1;

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
            //while (floor.StillBuilding())
            //{
            //   yield return null;// new WaitForSeconds(0.2f);
            //}
            yield return null;
        }
        coroutineRunning = false;
    }

    public IEnumerator Combine()
    {
        Vector3 savedPosition = transform.position;
        Vector3 savedScale = transform.localScale;
        transform.position = Vector3.zero;
        transform.localScale = new Vector3(1, 1, 1);

        // Iterate through floors and combine meshes
        List<CombineInstance> combine = new List<CombineInstance>();
        foreach (var floor in GetComponentsInChildren<CyberBuildingFloor>())
        {
            if(!floor.StillBuilding())
            {
                // Add mesh to be combined.
                MeshFilter floorMeshFilter = floor.GetComponent<MeshFilter>();
                CombineInstance newCombineInstance = new CombineInstance();
                newCombineInstance.mesh = floorMeshFilter.sharedMesh;
                newCombineInstance.transform = floorMeshFilter.transform.localToWorldMatrix;
                //floorMeshFilter.gameObject.SetActive(false);
                combine.Add(newCombineInstance);
                // Delete floor object
                Destroy(floorMeshFilter.gameObject);
                // TODO
            }
        }

        if (combine.Count > 0)
        {
            MeshFilter myMeshFilter = GetComponent<MeshFilter>();
            CombineInstance myCombine = new CombineInstance();
            myCombine.mesh = myMeshFilter.sharedMesh;
            myCombine.transform = myMeshFilter.transform.localToWorldMatrix;
            combine.Add(myCombine);

            myMeshFilter.mesh = new Mesh();
            myMeshFilter.mesh.CombineMeshes(combine.ToArray());
            transform.GetComponent<MeshRenderer>().sharedMaterial = finalMaterial;
            transform.gameObject.SetActive(true);
            myMeshFilter.mesh.Optimize();
        }

        transform.position = savedPosition;
        transform.localScale = savedScale;



        yield return null;
    }

    void ICyberBlock.Randomise()
    {
        height = Random.Range(1, 50);
        finalMaterial = materials[Random.Range(0, materials.Count)];
    }
}
