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
    
    public MeshFilter m_meshFilter;

    [SerializeField]
    private LayerMask m_layerMask;

    private List<Vector3> m_raycastHiPositions = new List<Vector3>();
    private List<Vector3> m_endOfLineOfSightPositions = new List<Vector3>();

    //private NativeArray<RaycastHit> m_results;
    private EntityManager m_entityManager;

    private bool m_isCurrentlyDrawingMesh= true;
    //private NativeArray<RaycastHit> m_result;

    // Start is called before the first frame update
    void Start()
    {
        m_entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        
        
    }

    private void RayCast()
    {
        
        m_endOfLineOfSightPositions.Clear();
        m_raycastHiPositions.Clear();
        
        var step = m_lineOfSight.m_amplitudeOfSightInDegrees / (m_lineOfSight.m_numberOfRaycast-1);
        
        double startTimer = Time.realtimeSinceStartup;
        var drawMesh = false;
        
        for (var i = 0; i < m_lineOfSight.m_numberOfRaycast; i++)
        {
            var rayCastDirection = Quaternion.Euler(0, (-m_lineOfSight.m_amplitudeOfSightInDegrees*.5f)+(i*step), 0)*transform.forward;
            var ray = new UnityEngine.Ray(transform.position,rayCastDirection);
            
            if (!Physics.Raycast(ray, out var hit, m_lineOfSight.m_maxDistance, m_layerMask))
            {
                if (m_isCurrentlyDrawingMesh)
                {
                    CloseMeshInfo();
                    m_isCurrentlyDrawingMesh = false;
                    continue;
                }
            }

            if (m_isCurrentlyDrawingMesh)
            {
                m_raycastHiPositions.Add(hit.point);
                m_endOfLineOfSightPositions.Add(ray.GetPoint(m_lineOfSight.m_maxDistance));
                drawMesh = true;
                AddNewMeshPoints(hit.point,ray.GetPoint(m_lineOfSight.m_maxDistance));
            }
            else
            {
                m_isCurrentlyDrawingMesh = true;
                //CreateNewMeshInfo();
            }
            
        }

        if (drawMesh)
        {
            DrawMesh();
        }
        
        for (var i = 0; i < m_raycastHiPositions.Count; i++)
        {
            var firstRaycastHit = m_raycastHiPositions[i];
            float distance;
            distance = Vector3.Distance(m_endOfLineOfSightPositions[i],firstRaycastHit);
            var rayCastDirection = (m_endOfLineOfSightPositions[i] - firstRaycastHit).normalized;
            Debug.DrawRay(firstRaycastHit,rayCastDirection*distance);
        }
        
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
        Debug.Log(workTime+" ms");
    }

    private void CreateNewMeshInfo() 
    {
        //throw new NotImplementedException();
    }

    private void AddNewMeshPoints(Vector3 _hitPoint, Vector3 _getPoint)
    {
        //throw new NotImplementedException();
        
    }

    private void CloseMeshInfo()
    {
        //throw new NotImplementedException();
        
    }

    private void DrawMesh()
    {
        Mesh mesh = new Mesh();
        var nbOfTriangles = (m_raycastHiPositions.Count - 1)*2;
        var nbOfVertices = nbOfTriangles * 3;
        var vertices = new Vector3[nbOfVertices];

        var relativeRotation = Quaternion.Inverse(Quaternion.LookRotation(transform.forward));
        for (var i = 0; i < m_raycastHiPositions.Count-2; i ++)
        {
            vertices[i*6] = relativeRotation*(m_raycastHiPositions[i]-transform.position);
            vertices[i*6+1] = relativeRotation*(m_endOfLineOfSightPositions[i]-transform.position);
            vertices[i*6+2] = relativeRotation*(m_raycastHiPositions[i+1]-transform.position);
            vertices[i*6+3] = relativeRotation*(m_endOfLineOfSightPositions[i]-transform.position);
            vertices[i*6+4] = relativeRotation*(m_endOfLineOfSightPositions[i+1]-transform.position);
            vertices[i*6+5] = relativeRotation*(m_raycastHiPositions[i+1]-transform.position);
            
            
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
        m_meshFilter.mesh = mesh;
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
