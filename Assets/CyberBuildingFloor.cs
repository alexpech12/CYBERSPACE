using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class CyberBuildingFloor : MonoBehaviour, ICyberBlock
{
    public int width = 3;
    public int depth = 3;

    public float pieceSize = 2.0f;
    public bool randomize = false;
    public bool generateOnUpdate = false;
    public int seed = 0;

    public List<Transform> prefabs;
    public Material initialMaterial;
    public Material finalMaterial;

    List<Transform> pieces;

    // Start is called before the first frame update
    void Start()
    {
        pieces = new List<Transform>();
        foreach (var p in prefabs)
        {
            p.GetComponentInChildren<MeshRenderer>().sharedMaterial = initialMaterial;
        }
        
    }
    bool coroutineRunning = true;
    bool inited = false;

    public void Init()
    {
        inited = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (coroutineRunning && inited)
        {
            coroutineRunning = false;
            StartCoroutine("Generate");
        }
    }

    void AddPiece(Vector3 pos, Quaternion rot)
    {
        var newPiece = Instantiate(GetPrefab(), transform);
        newPiece.localPosition = pos;
        newPiece.localRotation = rot;
        pieces.Add(newPiece);
    }

    public IEnumerator Generate()
    {
        Random.InitState(seed);
        if (randomize)
        {
            Random.InitState((int)System.DateTime.Now.Ticks);
        }
        if (prefabs.Count == 0)
        {
            yield return null;
        }
        

        InitPrefabSelector();

        // Do each side
        // -X
        Quaternion rot = Quaternion.Euler(0, 180, 0);
        for (int w = 0; w < width; ++w)
        {
            Vector3 pos = new Vector3(w * pieceSize, 0, 0);
            AddPiece(pos, rot);
            yield return null;
        }

        //+X
        rot = Quaternion.Euler(0, 0, 0);
        for (int w = 1; w <= width; ++w)
        {
            Vector3 pos = new Vector3(w * pieceSize, 0, depth * pieceSize);
            AddPiece(pos, rot);
            yield return null;
        }

        //-Z
        rot = Quaternion.Euler(0, -90, 0);
        for (int d = 1; d <= depth; ++d)
        {
            Vector3 pos = new Vector3(0, 0, d * pieceSize);
            AddPiece(pos, rot);
            yield return null;
        }

        //+Z
        rot = Quaternion.Euler(0, 90, 0);
        for (int d = 0; d < depth; ++d)
        {
            Vector3 pos = new Vector3(width * pieceSize, 0, d * pieceSize);
            AddPiece(pos, rot);
            yield return null;
        }

        yield return null;

        CombineMeshes();

        // We can now remove the individial pieces
        foreach(var p in pieces)
        {
            Destroy(p.gameObject);
        }
        pieces.Clear();

        coroutineRunning = false;
    }

    void CombineMeshes()
    {
        MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();
        CombineInstance[] combine = new CombineInstance[meshFilters.Length];

        Vector3 savedPosition = transform.position;
        Vector3 savedScale = transform.localScale;
        transform.position = Vector3.zero;
        transform.localScale = new Vector3(1, 1, 1);
        int i = 0;
        while (i < meshFilters.Length)
        {
            combine[i].mesh = meshFilters[i].sharedMesh;
            combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
            meshFilters[i].gameObject.SetActive(false);
            
            i++;
        }
        transform.GetComponent<MeshFilter>().mesh = new Mesh();
        transform.GetComponent<MeshFilter>().mesh.CombineMeshes(combine);
        transform.GetComponent<MeshRenderer>().sharedMaterial = finalMaterial;
        transform.gameObject.SetActive(true);
        transform.position = savedPosition;
        transform.localScale = savedScale;

    }


    enum PatternType
    {
        Uniform,
        Alternating,
        Random,
        Last = Random
    }

    PatternType pattern;
    int alternatingFrequency1 = 1;
    int alternatingFrequency2 = 1;

    List<Transform> selectedPrefabs;

    void InitPrefabSelector()
    {
        pattern = (PatternType)Random.Range(0, (int)PatternType.Last + 1);
        switch (pattern)
        {
            case PatternType.Uniform:
                selectedPrefabs = SelectRandom(prefabs, 1);
                break;
            case PatternType.Alternating:
                selectedPrefabs = SelectRandom(prefabs, 2);
                alternatingFrequency1 = Random.Range(1, 4);
                alternatingFrequency2 = Random.Range(1, 4);
                alternate = false;
                alternateCount = 0;
                break;
            case PatternType.Random:
                selectedPrefabs = prefabs;
                break;
            default:
                break;
        }

    }

    bool alternate = false;
    int alternateCount = 0;
    Transform GetPrefab()
    {
        if (selectedPrefabs.Count == 0)
        {
            return null;
        }

        switch (pattern)
        {
            case PatternType.Uniform:
                return selectedPrefabs[0];
            case PatternType.Alternating:
                Transform prefab = null;
                ++alternateCount;
                if (!alternate)
                {
                    prefab = selectedPrefabs[0];
                    if (alternateCount > alternatingFrequency1)
                    {
                        alternateCount = 0;
                        alternate = !alternate;
                    }
                }
                else
                {
                    prefab = selectedPrefabs[1];
                    if (alternateCount > alternatingFrequency2)
                    {
                        alternateCount = 0;
                        alternate = !alternate;
                    }
                }
                return prefab;
            case PatternType.Random:
                int i;
                return SelectRandom(selectedPrefabs, out i);
            default:
                break;
        }
        return null;
    }

    T SelectRandom<T>(List<T> list, out int index)
    {
        index = Random.Range(0, list.Count);
        return list[index];
    }

    List<T> SelectRandom<T>(List<T> list, int num)
    {
        if (num >= list.Count)
        {
            return list;
        }

        List<T> unselected;
        CopyList(out unselected, list);

        List<T> selected = new List<T>();

        for (int i = 0; i < num; ++i)
        {
            int index;
            T sel = SelectRandom(unselected, out index);
            unselected.RemoveAt(index);
            selected.Add(sel);
        }
        return selected;
    }

    void CopyList<T>(out List<T> l1, List<T> l2)
    {
        l1 = new List<T>(l2.Count);
        for (int i = 0; i < l2.Count; ++i)
        {
            l1.Add(l2[i]);
        }
    }

    void ICyberBlock.Randomise()
    {
        return;
    }
}
