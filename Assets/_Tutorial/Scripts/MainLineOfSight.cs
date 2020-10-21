using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainLineOfSight : MonoBehaviour
{
    public MeshFilter m_meshFilter_main;
    public MeshFilter m_meshFilter_secondary;
    public int m_numberOfRaycast = 9;

    private Vector3[] m_raycastHiPositions;
    public float m_maxDistance;
    public float m_amplitudeOfSightInDegrees;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        DrawLineOfSight();
    }

    private void DrawLineOfSight()
    {
        m_raycastHiPositions = new Vector3[m_numberOfRaycast];
        
        var step = m_amplitudeOfSightInDegrees / (m_numberOfRaycast-1);
        
        for (var i = 0; i < m_numberOfRaycast; i++)
        {
            var rayCastDirection = Quaternion.Euler(0, (-m_amplitudeOfSightInDegrees*.5f)+(i*step), 0)*transform.forward;
            m_raycastHiPositions[i] = transform.position + (rayCastDirection * m_maxDistance);
        }
        
        Mesh mesh = new Mesh();
        var nbOfTriangles = m_numberOfRaycast - 1;
        var nbOfVertices = nbOfTriangles * 3;
        var vertices = new Vector3[nbOfVertices];
        for (var i = 0; i < m_raycastHiPositions.Length-1; i ++)
        {
            vertices[i*3] = Vector3.zero;
            vertices[i * 3 + 1] = Quaternion.Inverse(Quaternion.LookRotation(transform.forward))*(m_raycastHiPositions[i]-transform.position);
            vertices[i*3+2] = Quaternion.Inverse(Quaternion.LookRotation(transform.forward))*(m_raycastHiPositions[i+1]-transform.position);
        }


        var triangles = new int[nbOfTriangles * 3];
        for (var i = 0; i < triangles.Length; i += 3)
        {
            triangles[i] = 0;
            triangles[i+ 1] = i + 1;
            triangles[i+ 2] = i + 2;
        }
        
        var uvs = new Vector2[nbOfVertices];
        
        for (var i = 0; i < triangles.Length-1; i +=3)
        {
            uvs[i] = new Vector2(0,1);
            uvs[i+1] = new Vector2(0,0); 
            uvs[i+2] = new Vector2(0,0); 
        }
        

        //uvs[0] = new Vector3(0,1);
        /*uvs[1] = new Vector3(0,1); 
        uvs[2] = new Vector3 (0,1);*/

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        
        //mesh.normals = normals;
        m_meshFilter_main.mesh = mesh;
        m_meshFilter_secondary.mesh = mesh;
    }
}
