using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public struct MeshOutput
{
    public Vector3[] vertices;
    public Vector3[] normals;
    public Vector2[] uvs;
    public int[] tris;
}

public struct MeshStepJob : IJob
{
    public float size;
    public Vector3 currentDirection;
    public NativeArray<Vector3> vertices;
    public NativeArray<Vector3> normals;
    public NativeArray<Vector2> uvs;
    public NativeArray<int> tris;
    //MeshOutput res;

    void IJob.Execute()
    {
        //res = new MeshOutput();

        // Shift the last two vertices along by size
        Vector3 v1 = vertices[vertices.Length - 2];
        Vector3 v2 = vertices[vertices.Length - 1];
        v1 = v1 + size * currentDirection;
        v2 = v2 + size * currentDirection;
        vertices[vertices.Length - 2] = v1;
        vertices[vertices.Length - 1] = v2;
        //res.vertices = vertices.ToArray();

        // Shift UV to prevent stretching
        Vector2 uv1 = uvs[uvs.Length - 2];
        Vector2 uv2 = uvs[uvs.Length - 1];
        uv1 = new Vector2(uv1.x + 1, uv1.y);
        uv2 = new Vector2(uv2.x + 1, uv2.y);
        uvs[uvs.Length - 2] = uv1;
        uvs[uvs.Length - 1] = uv2;

        //resultVertices.CopyFrom(vertices);
        //resultUVs.CopyFrom(uvs);
        //resultNormals.CopyFrom(normals);
        //resultTriangles.CopyFrom(tris);
        //res.uvs = uvs.ToArray();
        //res.normals = normals.ToArray();
        //res.tris = tris.ToArray();

        //result[0] = res;
    }
    
    //void Shift()
    //{
    //    // Shift the last two vertices along by size
    //    Vector3 v1 = vertices[vertices.Count - 2];
    //    Vector3 v2 = vertices[vertices.Count - 1];
    //    v1 = v1 + size * currentDirection;
    //    v2 = v2 + size * currentDirection;
    //    vertices[vertices.Count - 2] = v1;
    //    vertices[vertices.Count - 1] = v2;
    //    res.vertices = vertices;

    //    // Shift UV to prevent stretching
    //    Vector2 uv1 = uvs[uvs.Count - 2];
    //    Vector2 uv2 = uvs[uvs.Count - 1];
    //    uv1 = new Vector2(uv1.x + 1, uv1.y);
    //    uv2 = new Vector2(uv2.x + 1, uv2.y);
    //    uvs[uvs.Count - 2] = uv1;
    //    uvs[uvs.Count - 1] = uv2;
    //    res.uvs = uvs;

    //    res.normals = normals;
    //    res.tris = tris;
    //}
}

public struct MeshTurnJob : IJob
{
    public Quaternion rot;
    public NativeArray<Vector3> vertices;
    public NativeArray<Vector3> normals;
    public NativeArray<Vector2> uvs;
    public NativeArray<int> tris;

    //MeshOutput res;

    void IJob.Execute()
    {
        //res = new MeshOutput();
     
        //Quaternion rot = Quaternion.Euler(0, -90, 0);
        //currentDirection = rot * currentDirection;

        int index0 = vertices.Length - 4;

        Vector3 newV1 = vertices[index0];
        Vector3 newV2 = vertices[index0 + 1];

        vertices[index0+2] = newV1;
        vertices[index0+3] = newV1;
        
        uvs[index0 + 2] = uvs[index0];
        uvs[index0 + 3] = uvs[index0 + 1];

        int tri_index0 = tris.Length - 6;

        tris[tri_index0] = (index0 + 1);
        tris[tri_index0 + 1] = (index0);
        tris[tri_index0+2] = (index0 + 3);
        tris[tri_index0+3] = (index0);
        tris[tri_index0+4] = (index0 + 2);
        tris[tri_index0+5] = (index0 + 3);
        
        normals[index0 + 2] = (rot * normals[index0]);
        normals[index0 + 3] = (rot * normals[index0 + 1]);

        //res.vertices = vertices.ToArray();
        //res.normals = normals.ToArray();
        //res.uvs = uvs.ToArray();
        //res.tris = tris.ToArray();

        //result[0] = res;

        //resultVertices.CopyFrom(vertices.ToArray());
        //resultUVs.CopyFrom(uvs.ToArray());
        //resultNormals.CopyFrom(normals.ToArray());
        //resultTriangles.CopyFrom(tris.ToArray());
    }
}

public class CyberBuildingProceduralFloor : MonoBehaviour
{
    public float size = 2.0f;

    NativeArray<MeshOutput> result;

    public NativeArray<Vector3> vertices;
    public NativeArray<Vector3> normals;
    public NativeArray<Vector2> uvs;
    public NativeArray<int> tris;

    Vector3 currentDirection = Vector3.forward;

    // Start is called before the first frame update
    void Start()
    {

    }

    Mesh combinedMesh;
    bool started = false;
    bool complete = false;

    int i_side = 0;
    int i_corner = 0;

    int width = 3;
    int length = 4;

    JobHandle handle;

    // Update is called once per frame
    void Update()
    {
        if (complete) return;

        if (!started)
        {
            // Init
            combinedMesh = MakePlane(1.0f, Vector3.right);
            i_side = 1;
            started = true;
        }
        else
        {

            // Check job status
            if (!handle.IsCompleted) return;

            // Get result
            handle.Complete();

            combinedMesh = new Mesh();
            combinedMesh.vertices = vertices.ToArray();
            combinedMesh.normals = normals.ToArray();
            combinedMesh.uv = uvs.ToArray();
            combinedMesh.triangles = tris.ToArray();

            GetComponent<MeshFilter>().mesh = combinedMesh;

            vertices.Dispose();
            normals.Dispose();
            uvs.Dispose();
            tris.Dispose();
        }

        // Check next job state
        if (i_corner % 2 == 0 ? i_side == width : i_side == length)
        {
            if (i_corner == 3)
            {
                // We are done
                complete = true;
            }
            else
            {

                // Turn corner
                i_side = 0;
                ++i_corner;

                //TurnCorner(combinedMesh);
                MeshTurnJob turnJob = new MeshTurnJob();
                //turnJob.vertices = new List<Vector3>();
                //turnJob.normals = new List<Vector3>();
                //turnJob.uvs = new List<Vector2>();
                //turnJob.tris = new List<int>();
                //combinedMesh.GetVertices(turnJob.vertices);
                //combinedMesh.GetNormals(turnJob.normals);
                //combinedMesh.GetUVs(0, turnJob.uvs);
                //combinedMesh.GetTriangles(turnJob.tris, 0);
                //turnJob.currentDirection = currentDirection;

                ////turnJob.result = new NativeArray<MeshOutput>(1, Allocator.TempJob);
                //turnJob.resultVertices = new NativeArray<Vector3>(combinedMesh.vertices.Length, Allocator.TempJob);
                //turnJob.resultNormals = new NativeArray<Vector3>(combinedMesh.normals.Length, Allocator.TempJob);
                //turnJob.resultUVs = new NativeArray<Vector2>(combinedMesh.uv.Length, Allocator.TempJob);
                //turnJob.resultTriangles = new NativeArray<int>(combinedMesh.triangles.Length, Allocator.TempJob);

                List<Vector3> newVertices = new List<Vector3>();
                combinedMesh.GetVertices(newVertices);
                newVertices.Add(Vector3.zero);
                newVertices.Add(Vector3.zero);

                List<Vector3> newNormals = new List<Vector3>();
                combinedMesh.GetNormals(newNormals);
                newNormals.Add(Vector3.zero);
                newNormals.Add(Vector3.zero);

                List<Vector2> newUVs = new List<Vector2>();
                combinedMesh.GetUVs(0, newUVs);
                newUVs.Add(Vector2.zero);
                newUVs.Add(Vector2.zero);

                List<int> newTris = new List<int>();
                combinedMesh.GetTriangles(newTris, 0);
                newTris.Add(0);
                newTris.Add(0);
                newTris.Add(0);
                newTris.Add(0);
                newTris.Add(0);
                newTris.Add(0);

                vertices = new NativeArray<Vector3>(newVertices.ToArray(), Allocator.TempJob);
                normals = new NativeArray<Vector3>(newNormals.ToArray(), Allocator.TempJob);
                uvs = new NativeArray<Vector2>(newUVs.ToArray(), Allocator.TempJob);
                tris = new NativeArray<int>(newTris.ToArray(), Allocator.TempJob);

                Quaternion rot = Quaternion.Euler(0, -90, 0);
                currentDirection = rot * currentDirection;

                turnJob.rot = rot;
                turnJob.vertices = vertices;
                turnJob.normals = normals;
                turnJob.uvs = uvs;
                turnJob.tris = tris;


                handle = turnJob.Schedule();

                //Quaternion rot = Quaternion.Euler(0, -90, 0);
                //currentDirection = rot * currentDirection;
            }
        }
        else
        {
            // Step along
            //Shift(combinedMesh, 1.0f);
            MeshStepJob job = new MeshStepJob();
            //job.vertices = combinedMesh.vertices;
            //job.normals = combinedMesh.normals;
            //job.uvs = combinedMesh.uv;
            //job.tris = combinedMesh.triangles;
            //combinedMesh.GetVertices(job.vertices);
            //combinedMesh.GetNormals(job.normals);
            //combinedMesh.GetUVs(0, job.uvs);
            //combinedMesh.GetTriangles(job.tris, 0);
            job.currentDirection = currentDirection;
            job.size = size;

            //job.result = new NativeArray<MeshOutput>(1, Allocator.TempJob);

            vertices = new NativeArray<Vector3>(combinedMesh.vertices, Allocator.TempJob);
            normals = new NativeArray<Vector3>(combinedMesh.normals, Allocator.TempJob);
            uvs = new NativeArray<Vector2>(combinedMesh.uv, Allocator.TempJob);
            tris = new NativeArray<int>(combinedMesh.triangles, Allocator.TempJob);

            job.vertices = vertices;
            job.normals = normals;
            job.uvs = uvs;
            job.tris = tris;

            handle = job.Schedule();

            ++i_side;
        }


        


        //else
        //{

        //    if (!handle.IsCompleted) return;

        //    // Get result
        //    handle.Complete();

        //    combinedMesh = new Mesh();
        //    combinedMesh.SetVertices(result[0].vertices);
        //    combinedMesh.SetNormals(result[0].normals);
        //    combinedMesh.SetUVs(0, result[0].uvs);
        //    combinedMesh.SetTriangles(result[0].tris, 0);


        //    result.Dispose();
        //}

        //// Set mesh

        //GetComponent<MeshFilter>().mesh = combinedMesh;

        //if (!started)
        //{
        //    // Do init
        //    combinedMesh = MakePlane(1.0f, Vector3.right);
        //    i_side = 1;
        //    started = true;
        //}
        //else
        //{
        //    // Do step
        //    if(i_corner % 2 == 0 ? i_side == width : i_side == length)
        //    {
        //        if (i_corner == 3)
        //        {
        //            // We are done
        //            complete = true;
        //        }
        //        else
        //        {

        //            // Turn corner
        //            i_side = 0;
        //            ++i_corner;

        //            TurnCorner(combinedMesh);
        //        }
        //    }
        //    else
        //    {
        //        // Step along
        //        Shift(combinedMesh, 1.0f);
        //        ++i_side;
        //    }
        //}

        //GetComponent<MeshFilter>().mesh = combinedMesh;
    }

    Mesh MakePlane(float size, Vector3 normal)
    {
        Mesh mesh = new Mesh();
        size /= 2;
        Quaternion rot = Quaternion.FromToRotation(Vector3.up, normal);
        mesh.SetVertices(new List<Vector3> {
            rot * new Vector3(size, 0, -size),
            rot * new Vector3(-size, 0, -size),
            rot * new Vector3(-size, 0, size),
            rot * new Vector3(size, 0, size)
        });

        mesh.SetTriangles(new int[6] { 0, 1, 3, 1, 2, 3 }, 0);
        mesh.SetNormals(new List<Vector3> { normal, normal, normal, normal });
        mesh.SetUVs(0, new List<Vector2> {
            new Vector2(0, 0),
            new Vector2(0, 1),
            new Vector2(1, 1),
            new Vector2(1, 0),
        });
        
        return mesh;
    }

    void Shift(Mesh mesh, float size)
    {
        // Shift the last two vertices along by size
        Vector3[] vertices = mesh.vertices;
        Vector3 v1 = mesh.vertices[mesh.vertexCount - 2];
        Vector3 v2 = mesh.vertices[mesh.vertexCount - 1];
        v1 = v1 + size * currentDirection;
        v2 = v2 + size * currentDirection;
        vertices[mesh.vertexCount - 2] = v1;
        vertices[mesh.vertexCount - 1] = v2;
        mesh.SetVertices(new List<Vector3>(vertices));

        // Shift UV to prevent stretching
        Vector2[] uvs = mesh.uv;
        Vector2 uv1 = mesh.uv[mesh.uv.Length - 2];
        Vector2 uv2 = mesh.uv[mesh.uv.Length - 1];
        uv1 = new Vector2(uv1.x + 1, uv1.y);
        uv2 = new Vector2(uv2.x + 1, uv2.y);
        uvs[mesh.uv.Length - 2] = uv1;
        uvs[mesh.uv.Length - 1] = uv2;
        mesh.SetUVs(0, new List<Vector2>(uvs));
        
    }
    

    void TurnCorner()
    {
        currentDirection = Quaternion.Euler(0, 90, 0) * currentDirection;
    }

    void TurnCorner(Mesh mesh)
    {
        Quaternion rot = Quaternion.Euler(0, -90, 0);
        currentDirection = rot * currentDirection;

        List<Vector3> vertices = new List<Vector3>(mesh.vertexCount);
        mesh.GetVertices(vertices);
        int index0 = mesh.vertexCount - 2;

        vertices.Add(vertices[index0]);
        vertices.Add(vertices[index0 + 1]);

        List<Vector2> uvs = new List<Vector2>(mesh.uv.Length);
        mesh.GetUVs(0, uvs);
        uvs.Add(uvs[index0]);
        uvs.Add(uvs[index0 + 1]);

        List<int> tris = new List<int>(mesh.triangles.Length);
        mesh.GetTriangles(tris, 0);
        tris.Add(index0+1);
        tris.Add(index0);
        tris.Add(index0+3);
        tris.Add(index0);
        tris.Add(index0 + 2);
        tris.Add(index0 + 3);

        List<Vector3> norms = new List<Vector3>(mesh.normals.Length);
        mesh.GetNormals(norms);
        norms.Add(rot * norms[index0]);
        norms.Add(rot * norms[index0+1]);


        mesh.SetVertices(vertices);
        mesh.SetUVs(0, uvs);
        mesh.SetTriangles(tris, 0);
        mesh.SetNormals(norms);
    }
}
