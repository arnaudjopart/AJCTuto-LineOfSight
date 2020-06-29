using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Unity.Physics;
using Unity.Physics.Systems;
using RaycastHit = Unity.Physics.RaycastHit;

public class LineOfSight : MonoBehaviour
{
    public MainLineOfSight m_lineOfSight;
    
    [SerializeField]
    private LayerMask m_layerMask;

    //private List<Vector3> m_raycastHiPositions = new List<Vector3>();
    //private List<Vector3> m_endOfLineOfSightPositions = new List<Vector3>();

    //private NativeArray<RaycastHit> m_results;
    private EntityManager m_entityManager;

    //private bool m_isCurrentlyDrawingMesh= true;

    private RaycastData m_previousRaycastData;

    private List<LineOfSightMeshData> m_meshDataCollection;

    private LineOfSightMeshData m_currentMeshData;
    [SerializeField]
    private MeshFilter[] m_meshFilters;
    //private NativeArray<RaycastHit> m_result;

    // Start is called before the first frame update
    void Start()
    {
        m_entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        m_previousRaycastData = new RaycastData();
        m_meshDataCollection = new List<LineOfSightMeshData>();
    }

    private void RayCast()
    {
        m_meshDataCollection.Clear();

        var step = m_lineOfSight.m_amplitudeOfSightInDegrees / (m_lineOfSight.m_numberOfRaycast-1);
        
        double startTimer = Time.realtimeSinceStartup;
        var drawMesh = false;
        
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
            if (!Physics.Raycast(ray, out var hit, m_lineOfSight.m_maxDistance, m_layerMask))
            {
                currentRayCastResult.m_hit = false;
                currentRayCastResult.m_start = transform.position;
                currentRayCastResult.m_end = ray.GetPoint(m_lineOfSight.m_maxDistance);
                
                if (m_previousRaycastData.m_hit)
                {
                    CloseMeshInfo();
                    //m_isCurrentlyDrawingMesh = false;
                    
                }

                m_previousRaycastData = currentRayCastResult;
                
            }
            else
            {
                currentRayCastResult.m_hit = true;
                currentRayCastResult.m_start = hit.point;
                currentRayCastResult.m_end = ray.GetPoint(m_lineOfSight.m_maxDistance);
            
                if (m_previousRaycastData.m_hit)
                {
                    AddRaycastResult(currentRayCastResult);
                }
                else
                {
                    drawMesh = true;
                    //m_isCurrentlyDrawingMesh = true;
                    CreateNewMeshInfo();
                    AddRaycastResult(currentRayCastResult);
                    if (i > 0)
                    {
                        FindEdge();
                    }
                
                }
                m_previousRaycastData = currentRayCastResult; 
            }

            
            
        }

        if (drawMesh)
        {
            DrawMesh();
        }
        
        /*for (var i = 0; i < m_raycastHiPositions.Count; i++)
        {
            var firstRaycastHit = m_raycastHiPositions[i];
            var distance = Vector3.Distance(m_endOfLineOfSightPositions[i],firstRaycastHit);
            var rayCastDirection = (m_endOfLineOfSightPositions[i] - firstRaycastHit).normalized;
            Debug.DrawRay(firstRaycastHit,rayCastDirection*distance);
        }*/
        
        /*
        
                var collisionWorld = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<BuildPhysicsWorld>()
        .PhysicsWorld.CollisionWorld;
        var inputs = new NativeArray<RaycastInput>(m_numberOfRaycast,Allocator.TempJob);
        var result = new NativeArray<RaycastHit>(m_numberOfRaycast,Allocator.TempJob);
        
        for (var i = 0; i < m_numberOfRaycast; i++)
        {
            var rayCastDirection = Quaternion.Euler(0, (-m_amplitudeOfSightInDegrees*.5f)+(i*step), 0)*transform.forward;
            var raycastInput = new RaycastInput
            {
                Start = transform.position,
                End = transform.position+rayCastDirection*m_maxDistance,
                Filter = new CollisionFilter
                {
                    BelongsTo = ~0u,
                    CollidesWith = ~0u, // all 1s, so all layers, collide with everything
                    GroupIndex = 0
                }
            };

            inputs[i] = raycastInput;

        }

        NativeArray<float3> raycastPositions = new NativeArray<float3>(m_numberOfRaycast,Allocator.TempJob);
        
        DotsRaycastManager.MultipleRaycast(collisionWorld,inputs,ref result);
        DotsRaycastManager.GetRaycastDistances(collisionWorld,inputs,ref raycastPositions);
        
        
        for (var i = 0; i < m_numberOfRaycast; i++)
        {
            var firstRaycastHit = result[i];
            float distance;
            if (m_entityManager.Exists(firstRaycastHit.Entity))
            {
                distance = Vector3.Distance(transform.position,firstRaycastHit.Position);
            }
            else
            {
                distance = m_maxDistance; 
            }
            var rayCastDirection = Quaternion.Euler(0, (-m_amplitudeOfSightInDegrees*.5f)+(i*step), 0)*transform.forward;
            Debug.DrawRay(transform.position,rayCastDirection*distance);
        }
        
        inputs.Dispose();
        result.Dispose();
        
        /*
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
        uvs[2] = new Vector3 (0,1);

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        
        //mesh.normals = normals;
        m_meshFilter.mesh = mesh;
        */
        var endTimer = Time.realtimeSinceStartup;
        var workTime = (endTimer - startTimer)*1000;
        //Debug.Log(workTime+" ms");
    }

    private void FindEdge()
    {
        
    }

    private void CreateNewMeshInfo() 
    {
        m_currentMeshData = new LineOfSightMeshData();
        
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

        for (var i =0;i<m_meshDataCollection.Count;i++)
        {
            var mesh = DrawMeshFromData(m_meshDataCollection[i]);
            m_meshFilters[i].mesh = mesh;
        }
        
    }

    private Mesh DrawMeshFromData(LineOfSightMeshData _data)
    {
        Mesh mesh = new Mesh();
        if (_data.m_datas.Count < 2) return null;
        var nbOfTriangles = (_data.m_datas.Count - 1)*2;
        //var nbOfTriangles = (m_raycastHiPositions.Count - 1)*2;
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
        //if (!Input.GetMouseButtonDown(0)) return;
        RayCast();
    }

    private void OnDrawGizmos()
    {
        
    }

    private void OnDestroy()
    {
        
    }
}

public class LineOfSightMeshData
{
    public List<RaycastData> m_datas;

    public LineOfSightMeshData()
    {
        m_datas = new List<RaycastData>();
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
