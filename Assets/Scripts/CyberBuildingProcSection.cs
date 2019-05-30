using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CyberBuildingProcSection : MonoBehaviour
{
    public Transform floorPrefab;
    public int floorNum = 3;
    public int width = 3;
    public int length = 3;

    public Material initialMaterial;
    public Material finalMaterial;

    List<Transform> floors;

    CyberBuildingProcFloor currentFloor;

    enum BuildState
    {
        Init,
        Floors,
        WaitFloorsComplete,
        MatChange,
        Combine,
        Done
    }

    BuildState state;

    // Start is called before the first frame update
    void Start()
    {
        floors = new List<Transform>();
    }

    public bool Complete() { return state == BuildState.Done; }

    // Update is called once per frame
    int t = 0;
    bool ready = true;
    void Update()
    {
        switch (state)
        {
            case BuildState.Init:
                state = BuildState.Floors;
                break;
            case BuildState.Floors:
                // Create new floor
                var newFloor = Instantiate(floorPrefab, transform);
                newFloor.localPosition = new Vector3(0, t, 0);
                newFloor.GetComponent<MeshRenderer>().material = initialMaterial;
                currentFloor = newFloor.GetComponent<CyberBuildingProcFloor>();
                currentFloor.width = width;
                currentFloor.length = length;
                floors.Add(newFloor);
                ++t;

                if (t >= floorNum)
                {
                    state = BuildState.WaitFloorsComplete;
                }
                break;
            case BuildState.WaitFloorsComplete:
                bool complete = true;
                foreach (var floor in floors)
                {
                    if (!floor.GetComponent<CyberBuildingProcFloor>().Complete())
                    {
                        complete = false;
                        break;
                    }
                }
                if (complete)
                {
                    state = BuildState.MatChange;
                    t = 0;
                }
                break;
            case BuildState.MatChange:
                if(t < floors.Count)
                {
                    floors[t].GetComponent<MeshRenderer>().material = finalMaterial;
                    ++t;
                }
                else
                {
                    state = BuildState.Combine;
                    t = 0;
                }
                break;
            case BuildState.Combine:
                // Build the equivalent mesh of all the floors.
                Mesh mesh = GetCombinedMesh();
                GetComponent<MeshFilter>().mesh = mesh;
                GetComponent<MeshRenderer>().material = finalMaterial;

                // Can now destroy all floors.
                foreach(var floor in floors)
                {
                    Destroy(floor.gameObject);
                }
                state = BuildState.Done;

                break;
            case BuildState.Done:
                break;
            default: break;
        }
    }

    static Vector3[] normals = new Vector3[16]
    {
            new Vector3(0,0,-1),new Vector3(0,0,-1),new Vector3(0,0,-1),new Vector3(0,0,-1),
            new Vector3(1,0,0),new Vector3(1,0,0),new Vector3(1,0,0),new Vector3(1,0,0),
            new Vector3(0,0,1),new Vector3(0,0,1),new Vector3(0,0,1),new Vector3(0,0,1),
            new Vector3(-1,0,0),new Vector3(-1,0,0),new Vector3(-1,0,0),new Vector3(-1,0,0)
    };

    static int[] triangles = new int[3 * 8]
    {
            0,1,3,
            1,2,3,
            4,5,7,
            5,6,7,
            8,9,11,
            9,10,11,
            12,13,15,
            13,14,15
    };

    Mesh GetCombinedMesh()
    {
        int h = floorNum; // height
        int w = width;
        int l = length;
        Vector3[] vertices = new Vector3[16] {
            // Quad 1 -> (0,0) in +ve x direction
            new Vector3(0,0,0),
            new Vector3(0,h,0),
            new Vector3(l,h,0),
            new Vector3(l,0,0),
            // Quad 2 -> (length,0) in +ve z direction
            new Vector3(l,0,0),
            new Vector3(l,h,0),
            new Vector3(l,h,w),
            new Vector3(l,0,w),
            // Quad 3 -> (length,width) in -ve x direction
            new Vector3(l,0,w),
            new Vector3(l,h,w),
            new Vector3(0,h,w),
            new Vector3(0,0,w),
            // Quad 4 -> (0,3) in -ve z direction
            new Vector3(0,0,w),
            new Vector3(0,h,w),
            new Vector3(0,h,0),
            new Vector3(0,0,0)
        };

        Vector2[] uv = new Vector2[16]
        {
            new Vector2(0,0),new Vector2(0,h),new Vector2(l,h),new Vector2(l,0),
            new Vector2(0,0),new Vector2(0,h),new Vector2(w,h),new Vector2(w,0),
            new Vector2(0,0),new Vector2(0,h),new Vector2(l,h),new Vector2(l,0),
            new Vector2(0,0),new Vector2(0,h),new Vector2(w,h),new Vector2(w,0)
        };

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.uv = uv;
        mesh.triangles = triangles;
        return mesh;
    }
}
