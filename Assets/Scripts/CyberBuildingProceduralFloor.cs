using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CyberBuildingProceduralFloor : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Mesh mesh1 = MakePlane(1.0f, Vector3.up);

        Mesh mesh2 = MakePlane(2.0f, Vector3.right);
        for (int x = 0; x < 3; ++x)
        {
            Shift(mesh2, 2.0f);
        }

        for (int x = 0; x < 3; ++x)
        {
            Shift(mesh2, 2.0f);
        }
        //TurnCorner();
        //for (int x = 0; x < 12; ++x)
        //{
        //    Shift(mesh2, 2.0f);
        //}
        //TurnCorner();
        //for (int x = 0; x < 7; ++x)
        //{
        //    Shift(mesh2, 2.0f);
        //}
        //TurnCorner();

        CombineInstance[] combine = new CombineInstance[2];
        combine[0].mesh = mesh1;
        combine[0].transform = Matrix4x4.identity;
        combine[1].mesh = mesh2;
        combine[1].transform = Matrix4x4.identity;

        GetComponent<MeshFilter>().mesh = new Mesh();
        GetComponent<MeshFilter>().mesh.CombineMeshes(combine);
    }

    // Update is called once per frame
    void Update()
    {
        
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

    Vector3 currentDirection = Vector3.forward;
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

    void TurnCorner(Mesh mesh)
    {
        currentDirection = Quaternion.Euler(0, 90, 0) * currentDirection;

        List<Vector3> vertices = new List<Vector3>(mesh.vertexCount);
        mesh.GetVertices(vertices);
        vertices.Add(vertices[mesh.vertexCount - 2]);
        vertices.Add(vertices[mesh.vertexCount - 1]);
        mesh.SetVertices(vertices);

        List<Vector2> uvs = new List<Vector2>(mesh.vertexCount);
        mesh.GetUVs(0, uvs);
        uvs[mesh.vertexCount - 2] = uvs[0];
        uvs[mesh.vertexCount - 1] = uvs[1];
        mesh.SetUVs(0, uvs);


        
    }
}
