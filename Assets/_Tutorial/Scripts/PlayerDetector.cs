using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerDetector : MonoBehaviour
{
    public Player m_player;
    public MainLineOfSight m_lineOfSight;
    public Transform m_lineStartPosition;
    // Start is called before the first frame update
    void Start()
    {
        m_lineRenderer = GetComponent<LineRenderer>();
        m_progressHandler = new SimpleProgress();
    }

    // Update is called once per frame
    void Update()
    {
        var playerDirection = (m_player.Position - transform.position).normalized;
        var angle = Vector3.Angle(transform.forward, playerDirection);
        if (angle < m_lineOfSight.m_amplitudeOfSightInDegrees * .5f)
        {
            m_lineRenderer.enabled = true;
            Vector3[] linePoints = {
                m_lineStartPosition.position,
                m_player.m_headPosition.position
            };
            m_lineRenderer.positionCount = 2;
            m_lineRenderer.SetPositions(linePoints);

            
            RaycastCheck(playerDirection);
        }
        else
        {
            m_lineRenderer.enabled = false;
        }
    }

    private void RaycastCheck(Vector3 _direction)
    {
        //m_lineRenderer.material = m_defaultMaterial;
        var ray = new Ray(transform.position,_direction);
        var raycastHits = Physics.RaycastAll(ray, 500, m_layerMask);
        

        if (raycastHits.Length == 0) return;
        
        
        var incrementalLength = 0f;
        var positions = new Vector3[m_lineRenderer.positionCount];
        var nbPositions = m_lineRenderer.GetPositions(positions);
        for(var i =1;i<nbPositions;i++)
        {
            incrementalLength += Vector3.Distance(positions[i], positions[i - 1]);
        }
            
        var timeToFill = incrementalLength / m_progressSpeedInUnitPerSecond;
        var timeToEmpty = incrementalLength / m_retreatSpeedInUnitPerSecond;
            
        var progressSpeed = 1 / timeToFill;
        var retreatSpeed = 1 / timeToEmpty;

        var semiCover = false;
   
        var orderedByDistanceRaycastHits = raycastHits.OrderBy(hit => hit.distance).ToArray();
        foreach (var t in orderedByDistanceRaycastHits)
        {
            bool isPlayerSeen = false;

            var progress = 0f;
            if (t.collider.gameObject.CompareTag("FullCover"))
            {
                m_lineRenderer.enabled = false;
                progress = m_progressHandler.UpdateValue(- retreatSpeed * Time.deltaTime);
                m_detectedMaterial.SetFloat("_Progress",progress);
                return;
            }

            if (t.collider.gameObject.CompareTag("SemiCover"))
            {
                semiCover = true;
                continue;
            }
            
            if (semiCover == false)
            {
                //Player is seen
                isPlayerSeen = true;
            }
            else
            {
                //If semiCover, Player must stand up to be detected. If he is crouch, he is not detected.
                isPlayerSeen = !m_player.isCrouch;
            }
                
            m_lineRenderer.enabled = isPlayerSeen;
            
            progress = isPlayerSeen ? m_progressHandler.UpdateValue(progressSpeed * Time.deltaTime) : m_progressHandler.UpdateValue( -retreatSpeed * Time.deltaTime);
            m_detectedMaterial.SetFloat("_Progress",progress);

        }
    }

    private LineRenderer m_lineRenderer;
    public LayerMask m_layerMask;
    public Material m_detectedMaterial;
    private SimpleProgress m_progressHandler;
    [FormerlySerializedAs("m_progressSpeed")] public float m_progressSpeedInUnitPerSecond;
    [FormerlySerializedAs("m_retreatSpeed")] public float m_retreatSpeedInUnitPerSecond;
}

public class SimpleProgress : ProgressHandler
{
    public SimpleProgress() : base()
    {
        
    }
}

public class ProgressHandler
{
    private float m_currentProgress;

    protected ProgressHandler()
    {
        
    }
    public virtual float UpdateValue(float _step)
    {
        
        m_currentProgress += _step;
        m_currentProgress = Mathf.Clamp(m_currentProgress, 0, 1);
        return m_currentProgress;
    }
}