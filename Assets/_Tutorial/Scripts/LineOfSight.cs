using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Unity.Physics;
using Unity.Physics.Systems;
using UnityEngine.UIElements;
using RaycastHit = Unity.Physics.RaycastHit;

public class LineOfSight : MonoBehaviour
{
    public MainLineOfSight m_lineOfSight;
    
    [SerializeField]
    private LayerMask m_semiCoverLayerMask;
    
    [SerializeField]
    private LayerMask m_fullCoverLayerMask;
    
    private RaycastData m_previousRaycastData;

    private List<MaskMeshData> m_meshDataCollection;

    private MaskMeshData m_currentMeshData;
    [SerializeField]
    private MeshFilter[] m_meshFilters;
    //private NativeArray<RaycastHit> m_result;

    // Start is called before the first frame update
    void Start()
    {
        m_previousRaycastData = new RaycastData();
        m_meshDataCollection = new List<MaskMeshData>();
    }

    private List<MaskMeshData> RayCast(MaskMeshData.TYPE _type)
    {
        LayerMask mask;
        switch (_type)
        {
            case MaskMeshData.TYPE.FULL:
                mask = m_fullCoverLayerMask;
                break;
            case MaskMeshData.TYPE.SEMI:
                mask = m_semiCoverLayerMask;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(_type), _type, null);
        }
       var meshDataCollection = new List<MaskMeshData>();
       var currentMeshData = new MaskMeshData(_type);
        
        var step = m_lineOfSight.m_amplitudeOfSightInDegrees / (m_lineOfSight.m_numberOfRaycast-1);

        for (var i = 0; i < m_lineOfSight.m_numberOfRaycast; i++)
        {
            var angle = (-m_lineOfSight.m_amplitudeOfSightInDegrees*.5f)+(i * step);
            var rayCastDirection = Quaternion.Euler(0, angle, 0)*transform.forward;
            var ray = new UnityEngine.Ray(transform.position,rayCastDirection);

            var currentRayCastResult = new RaycastData()
            {
                m_angle = angle,
                m_direction =  rayCastDirection

            };
            if (!Physics.Raycast(ray, out var hit, m_lineOfSight.m_maxDistance, mask))
            {
                currentRayCastResult.m_hit = false;
                currentRayCastResult.m_start = transform.position;
                currentRayCastResult.m_end = ray.GetPoint(m_lineOfSight.m_maxDistance);
                
                if (m_previousRaycastData.m_hit)
                {
                    if(currentMeshData.m_datas.Count>0) meshDataCollection.Add(currentMeshData);
                }

                m_previousRaycastData = currentRayCastResult;
                continue;

            }
            currentRayCastResult.m_hit = true;
            currentRayCastResult.m_start = hit.point;
            currentRayCastResult.m_end = ray.GetPoint(m_lineOfSight.m_maxDistance);
            
            if (m_previousRaycastData.m_hit)
            {
                currentMeshData.m_datas.Add(currentRayCastResult);
                if (i == m_lineOfSight.m_numberOfRaycast-1)
                {
                    if(currentMeshData.m_datas.Count>0) meshDataCollection.Add(currentMeshData);
                }
            }
            else
            {
                currentMeshData = new MaskMeshData(_type);
                currentMeshData.m_datas.Add(currentRayCastResult);
                if (i > 0)
                {
                    FindEdge();
                }
                
            }
            m_previousRaycastData = currentRayCastResult;
        }
        
        return meshDataCollection;

    }

    private void FindEdge()
    {
        
    }

    private void CreateNewMeshInfo() 
    {
        // m_currentMeshData = new MaskMeshData();
        
    }

    private void AddRaycastResult(RaycastData _data)
    {
       m_currentMeshData.m_datas.Add(_data);
                
    }

    private void CloseMeshInfo()
    {
        m_meshDataCollection.Add(m_currentMeshData);
    }

    private void DrawMesh()
    {
        var nbOfMeshToDraw = m_meshDataCollection.Count;
        print(nbOfMeshToDraw);
        for (var i =0;i<nbOfMeshToDraw;i++)
        {
            var mesh = DrawMeshFromData(m_meshDataCollection[i]);
            if (m_meshFilters.Length > i - 1)
            {
                m_meshFilters[i].mesh = mesh;
                if (m_meshDataCollection[i].m_type == MaskMeshData.TYPE.FULL)
                {
                    m_meshFilters[i].gameObject.layer = 8;
                }
                if (m_meshDataCollection[i].m_type == MaskMeshData.TYPE.SEMI)
                {
                    m_meshFilters[i].gameObject.layer = 11;
                }
            }
        }

        if (nbOfMeshToDraw > m_meshFilters.Length)
        {
            
        }

        for (var i = 0; i < m_meshFilters.Length; i++)
        {
            m_meshFilters[i].gameObject.SetActive(i < nbOfMeshToDraw);
        }
    }

    private Mesh DrawMeshFromData(MaskMeshData _data)
    {
        var mesh = new Mesh();
        if (_data.m_datas.Count < 2) return null;
        var nbOfTriangles = (_data.m_datas.Count - 1)*2;

        var nbOfVertices = nbOfTriangles * 3;
        var vertices = new Vector3[nbOfVertices];

        var relativeRotation = Quaternion.Inverse(Quaternion.LookRotation(transform.forward));
        
        for (var i = 0; i < _data.m_datas.Count-1; i ++)
        {
            vertices[i*6] = relativeRotation*(_data.m_datas[i].m_start-transform.position);
            vertices[i*6+1] = relativeRotation*(_data.m_datas[i].m_end-transform.position);
            vertices[i*6+2] = relativeRotation*(_data.m_datas[i+1].m_start-transform.position);
            vertices[i*6+3] = relativeRotation*(_data.m_datas[i].m_end-transform.position);
            vertices[i*6+4] = relativeRotation*(_data.m_datas[i+1].m_end-transform.position);
            vertices[i*6+5] = relativeRotation*(_data.m_datas[i+1].m_start-transform.position);

        }


        var triangles = new int[nbOfTriangles * 3];
        
        for (var i = 0; i < triangles.Length; i ++ )
        {
            triangles[i] = i;
        }
        
        var uvs = new Vector2[nbOfVertices];
        
        for (var i = 0; i < uvs.Length; i ++)
        {
            uvs[i] = new Vector2(0,0);
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        
        //mesh.normals = normals;
        return mesh;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        var semiCoverMeshDataCollection = RayCast(MaskMeshData.TYPE.SEMI);
        var fullCoverMeshDataCollection = RayCast(MaskMeshData.TYPE.FULL);

        m_meshDataCollection.Clear();
        m_meshDataCollection.AddRange(semiCoverMeshDataCollection);
        m_meshDataCollection.AddRange(fullCoverMeshDataCollection);
        
        DrawMesh();
    }

    private void OnDrawGizmos()
    {
        
    }

    private void OnDestroy()
    {
        
    }
}

public class MaskMeshData
{
    public List<RaycastData> m_datas;

    public enum TYPE
    {
        FULL,
        SEMI
    };

    public TYPE m_type;
    public MaskMeshData(TYPE _type)
    {
        m_datas = new List<RaycastData>();
        m_type = _type;
    }
}

public struct RaycastData
{
    public Vector3 m_direction;
    public Vector3 m_start;
    public Vector3 m_end;
    public float m_angle;
    public bool m_hit;
}
