using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using UnityEngine.Jobs;
using Unity.Collections;

// --------------------------------------------------------------------------------------------------
// Reference Example
// https://github.com/stella3d/job-system-cookbook/blob/master/Assets/Scripts/MeshComplexParallel.cs
// --------------------------------------------------------------------------------------------------

public class CyberBuildingProcFloor : MonoBehaviour
{
     
    public NativeArray<Vector3> vertices;
    public NativeArray<Vector2> uvs;

    public int length = 3;
    public int width = 3;

    Vector3[] modifiedVertices;
    Vector3[] modifiedNormals;
    Vector2[] modifiedUVs;

    Mesh mesh;

    MeshJob job;

    JobHandle handle;

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

    Vector2[] uv = new Vector2[16]
    {
        new Vector2(0,0),new Vector2(0,1),new Vector2(0,1),new Vector2(0,0),
        new Vector2(0,0),new Vector2(0,1),new Vector2(0,1),new Vector2(0,0),
        new Vector2(0,0),new Vector2(0,1),new Vector2(0,1),new Vector2(0,0),
        new Vector2(0,0),new Vector2(0,1),new Vector2(0,1),new Vector2(0,0)
    };

    // Start is called before the first frame update
    void Start()
    {
        // this persistent memory setup assumes our vertex count will not expand

        // Initialize mesh vertices
        vertices = new NativeArray<Vector3>( new Vector3[16] {
            // Quad 1 -> (0,0) in +ve x direction
            new Vector3(0,0,0),
            new Vector3(0,1,0),
            new Vector3(0,1,0),
            new Vector3(0,0,0),
            // Quad 2 -> (length,0) in +ve z direction
            new Vector3(length,0,0),
            new Vector3(length,1,0),
            new Vector3(length,1,0),
            new Vector3(length,0,0),
            // Quad 3 -> (length,width) in -ve x direction
            new Vector3(length,0,width),
            new Vector3(length,1,width),
            new Vector3(length,1,width),
            new Vector3(length,0,width),
            // Quad 4 -> (0,3) in -ve z direction
            new Vector3(0,0,width),
            new Vector3(0,1,width),
            new Vector3(0,1,width),
            new Vector3(0,0,width)
        }, Allocator.Persistent);

        // Initialize mesh uvs
        uvs = new NativeArray<Vector2>(uv, Allocator.Persistent);


        mesh = new Mesh();

        gameObject.GetComponent<MeshFilter>().mesh = mesh;
        mesh.MarkDynamic();


        modifiedVertices = new Vector3[vertices.Length];
        modifiedUVs = new Vector2[uvs.Length];

        mesh.vertices = modifiedVertices;
        mesh.uv = modifiedUVs;
        mesh.normals = normals;
        mesh.triangles = triangles;
    }

    public bool Complete() { return complete; }

    int t = 0;
    bool complete = false;
    public void Update()
    {
        if (complete) return;
        
        if ( t > 2*width + 2*length)
        {
            // Waiting for final job to complete
            if(handle.IsCompleted)
            {
                handle.Complete();
                vertices.Dispose();
                uvs.Dispose();
                
                complete = true;
                Debug.Log("Complete!");
                return;
            }

        }
        else if (t == 0)
        {
            ScheduleNewJob();
            ++t;
        }
        

        if(handle.IsCompleted)
        {
            handle.Complete();

            // copy our results to managed arrays so we can assign them
            job.vertices.CopyTo(modifiedVertices);
            job.uvs.CopyTo(modifiedUVs);

            mesh.vertices = modifiedVertices;
            mesh.uv = modifiedUVs;

            ScheduleNewJob();

            ++t;
        }
    }

    void ScheduleNewJob()
    {
        job = new MeshJob()
        {
            vertices = vertices,
            uvs = uvs,
            length = length,
            width = width,
            t = t
        };

        handle = job.Schedule();
    }

    private void OnDestroy()
    {
        // make sure to Dispose() any NativeArrays when we're done
        if (vertices.IsCreated)  vertices.Dispose();
        if (uvs.IsCreated) uvs.Dispose();
    }

    struct MeshJob : IJob
    {
        public NativeArray<Vector3> vertices;
        public NativeArray<Vector2> uvs;

        public int t;
        public int length; // Length used first
        public int width;

        void IJob.Execute()
        {
            Vector2 uv = new Vector2(1, 0);
            // What section are we in?
            if (t < length)
            {
                // Shift vertices 2 and 3 along +ve x
                Vector3 v = new Vector3(1, 0, 0);
                vertices[2] += v;
                vertices[3] += v;
                uvs[2] += uv;
                uvs[3] += uv;
            }
            else if(t < (length + width))
            {

                // Shift vertices 6 and 7 along +ve z
                vertices[6] += new Vector3(0, 0, 1);
                vertices[7] += new Vector3(0, 0, 1);
                uvs[6] += uv;
                uvs[7] += uv;
            }
            else if(t < (2*length + width))
            {
                // Shift vertices 10 and 11 along -ve x
                vertices[10] += new Vector3(-1, 0, 0);
                vertices[11] += new Vector3(-1, 0, 0);
                uvs[10] += uv;
                uvs[11] += uv;
            }
            else if (t < (2 * length + 2*width))
            {
                // Shift vertices 14 and 15 along -ve z
                vertices[14] += new Vector3(0, 0, -1);
                vertices[15] += new Vector3(0, 0, -1);
                uvs[14] += uv;
                uvs[15] += uv;
            }
        }
    }

}
