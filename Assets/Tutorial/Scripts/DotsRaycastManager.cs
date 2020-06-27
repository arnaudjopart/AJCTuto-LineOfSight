using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Physics;


public class DotsRaycastManager
{
    [BurstCompile]
    public struct RaycastJob : IJobParallelFor
    {
        [ReadOnly] public CollisionWorld m_world;
        [ReadOnly] public NativeArray<RaycastInput> m_inputs;
        public NativeArray<RaycastHit> m_results;
        
        public void Execute(int index)
        {
            var hit = new RaycastHit();
            m_world.CastRay(m_inputs[index], out hit);
            m_results[index] = hit;
        }
    }


    public static JobHandle ScheduleBatchRaycast(CollisionWorld _collisionWorld, NativeArray<RaycastInput> _inputs,
        NativeArray<RaycastHit> _results)
    {
        var raycastJob = new RaycastJob()
        {
            m_world = _collisionWorld,
            m_inputs = _inputs,
            m_results = _results
        };

        JobHandle jobHandle = raycastJob.Schedule(_inputs.Length, 4);
        return jobHandle;
    }

    public static void SingleRaycast(CollisionWorld _collisionWorld, RaycastInput _input,
        ref RaycastHit _result)
    {
        var singleInput = new NativeArray<RaycastInput>(1,Allocator.TempJob);
        var singleResult = new NativeArray<RaycastHit>(1,Allocator.TempJob);

        singleInput[0] = _input;

        var handle = ScheduleBatchRaycast(_collisionWorld, singleInput, singleResult);
        handle.Complete();
        _result = singleResult[0];

        singleInput.Dispose();
        singleResult.Dispose();
    }

    public static void MultipleRaycast(CollisionWorld _collisionWorld, NativeArray<RaycastInput> _inputs,
        ref NativeArray<RaycastHit> _results)
    {
        var handle = ScheduleBatchRaycast(_collisionWorld, _inputs, _results);
        handle.Complete();
    }
    
}
