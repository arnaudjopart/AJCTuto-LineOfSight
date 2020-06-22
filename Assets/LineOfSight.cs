using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineOfSight : MonoBehaviour
{
    public MeshRenderer m_meshRenderer;
    public MeshFilter m_meshFilter;
    
    public float m_amplitudeOfSightInDegrees = 60;

    public int m_numberOfRaycast = 9;
    [SerializeField]
    private float m_maxDistance;
    [SerializeField]
    private LayerMask m_layerMask;

    private Vector3[] m_raycastHiPositions;
    // Start is called before the first frame update
    void Start()
    {
        m_raycastHiPositions = new Vector3[m_numberOfRaycast];
        
    }

    private void DrawMesh()
    {
        var step = m_amplitudeOfSightInDegrees / (m_numberOfRaycast-1);
        m_raycastHiPositions = new Vector3[m_numberOfRaycast];
        double startTimer = Time.realtimeSinceStartup;
        for (var i = 0; i < m_numberOfRaycast; i++)
        {
            var rayCastDirection = Quaternion.Euler(0, (-m_amplitudeOfSightInDegrees*.5f)+(i*step), 0)*transform.forward;
            var ray = new Ray(transform.position,rayCastDirection);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, m_maxDistance, m_layerMask))
            {
                m_raycastHiPositions[i] = hit.point;
            }
            else
            {
                m_raycastHiPositions[i] = ray.GetPoint(m_maxDistance);
            }
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
        
        for (var i = 0; i < triangles.Length; i += 3)
        {
            uvs[i] = new Vector2(0,0);
        }
        
        
        /*var normals = new Vector3[3];
        uvs[0] = new Vector3(0,1);
        uvs[1] = new Vector3(0,1);
        uvs[2] = new Vector3 (0,1);*/

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        
        //mesh.normals = normals;
        m_meshFilter.mesh = mesh;
        
        var endTimer = Time.realtimeSinceStartup;
        var workTime = (endTimer - startTimer)*1000;
        //Debug.Log(workTime+" ms");
    }

    // Update is called once per frame
    void Update()
    {
        DrawMesh();
    }

    private void OnDrawGizmos()
    {
        var step = m_amplitudeOfSightInDegrees / (m_numberOfRaycast-1);
        for (var i = 0; i < m_numberOfRaycast; i++)
        {
            var distance = Vector3.Distance(transform.position,m_raycastHiPositions[i]);
            var rayCastDirection = Quaternion.Euler(0, (-m_amplitudeOfSightInDegrees*.5f)+(i*step), 0)*transform.forward;
            Debug.DrawRay(transform.position,rayCastDirection*distance);
        }
    }
}
