using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

using math = Unity.Mathematics.math;
namespace jp.geometry
{
//    [BurstCompile]
    public struct AnimationJob: UnityEngine.Animations.IAnimationJob
    {
        public struct Edge
        {
            public int first;
            public int second;
        };

        public struct Constraint
        {
            public int pointIndex;
            public float3 fixedPosition;
            public float3 fixedVelocity;
        };

        public struct SphereCollision
        {
            public int enabled;
            public float3 centerPosition;
            public float radious;
        };

        public enum BaseAxis
        {
            X,
            Y,
            Z
        };
        public NativeArray<UnityEngine.Animations.TransformStreamHandle> joints;
        //        public NativeArray<TransformSceneHandle> effectors;
        private NativeArray<Edge> edges;
        private NativeArray<Constraint> constraints;
        private NativeArray<float3> vectorData;
        private NativeArray<float3> positions;
        private NativeArray<float3> velocities;
        private NativeArray<float3> forces;
        private NativeArray<float> masses;
        private NativeArray<int> flags;
        private NativeArray<SphereCollision> sphereCollisions;
        public float timeIntervalInSeconds;
        private float3 gravity;
        private float stiffness;
        public float restLength;
        private float dampingCoefficient;
        private float dragCoefficient;
        private float restitutionCoefficient;
        private float3 UnitVector;
        private int initialized;

        public float floorPositionY;

        public float3 rootOffset;
        public BaseAxis baseAxis;
        public int hangGameObjectFlag;

        public float elapsed;
        private float resolution;
        //        internal TransformSceneHandle targetEffector;

        public void SetUp()
        {
            initialized = 0;
            int length = joints.Length;
            edges = new NativeArray<Edge>(length, Allocator.Persistent);
            positions = new NativeArray<float3>(length, Allocator.Persistent);
            velocities = new NativeArray<float3>(length, Allocator.Persistent);
            forces = new NativeArray<float3>(length, Allocator.Persistent);
            masses = new NativeArray<float>(length, Allocator.Persistent);
            flags = new NativeArray<int>(length, Allocator.Persistent);
            constraints = new NativeArray<Constraint>(2, Allocator.Persistent);
            sphereCollisions = new NativeArray<SphereCollision>(1, Allocator.Persistent);


            for ( int i = 0; i < sphereCollisions.Length;i++)
            {
                sphereCollisions[i] = new SphereCollision()
                {
                    enabled = 0,
                    centerPosition = float3.zero,
                    radious = 0.0f
                };
            }
            for (int i = 0; i < flags.Length; i++)
            {
                flags[i] = 0;
            }
            flags[0] = 0x8000;
            constraints[0] = new Constraint()
            {
                pointIndex = 0,
                fixedPosition = rootOffset,
                fixedVelocity = float3.zero
            };
            constraints[1] = new Constraint()
            {
                pointIndex = -1,
                fixedPosition = rootOffset,
                fixedVelocity = float3.zero
            };
            vectorData = new NativeArray<float3>(3, Allocator.Persistent);
            gravity = new float3(0.0f, -9.8f, 0.0f);
#if false
            stiffness =  500.0f;//
                                  //            restLength = 1.0f;
            dampingCoefficient = 1.0f;
            dragCoefficient =  0.1f;

            restitutionCoefficient =   0.3f;
#else
            stiffness = 1300.0f;//
                                 //            restLength = 1.0f;
            dampingCoefficient = 7.0f;
            dragCoefficient = 2.1f;

            restitutionCoefficient = 0.3f;
#endif
            UnitVector = 
                new float3(baseAxis == BaseAxis.X ? 1.0f : 0.0f,
                baseAxis == BaseAxis.Y ? 1.0f : 0.0f,
                baseAxis == BaseAxis.Z ? 1.0f : 0.0f);

            MakeChain(length);
            baseAxis = AnimationJob.BaseAxis.Y;
            timeIntervalInSeconds = 1 / 30.0f; // Time.fixedDeltaTime;
            resolution = 0.5f;
            timeIntervalInSeconds *=  resolution;
            elapsed = 0.0f;


        }
        /*
        public void SetSpringParameter(
            float stiffness_,
            float dampingCoefficient_,
            float dragCoefficient_,
            float restitutionCoefficient_)
        {
            stiffness = stiffness_;
            dampingCoefficient = dampingCoefficient_;

            dragCoefficient = dragCoefficient_;
            restitutionCoefficient = restitutionCoefficient_;
            Debug.Log($"SetSpringParameter::dampingCoefficient = {dampingCoefficient}");

        }
        */

         
        public void SetVectorData(int index, Vector3 data)
        {
            if ( index < vectorData.Length)
                vectorData[index] = data;
        }
        public void SetEffectorMass(int index, float mass)
        {
            if (index < masses.Length)
                masses[index] = mass;
        }
        public void SetConstraint(int index,  Vector3 constraintPosition)
        {
            if (constraints.Length > index)
            {
                int pointIndex = 0;
                if ( index != 0)
                {
                    pointIndex = joints.Length - 1; // end position.
                }
                Constraint cons = constraints[index];
                cons.pointIndex = pointIndex;
                cons.fixedPosition = constraintPosition;
                constraints[index] = cons;
                flags[pointIndex] = 0x8000;
            }
        }

        public void ResetConstraint(int index)
        {
            if (constraints.Length > index)
            {
                Constraint cons = constraints[index];
                if (cons.pointIndex >= 0)
                    flags[cons.pointIndex] &= ~0x8000;
                cons.pointIndex = -1;
                constraints[index] = cons;

            }
        }

        void MakeChain(int numberOfPoints)
        {
            if (numberOfPoints == 0)
            {
                return;
            }

            int numberOfEdges = numberOfPoints - 1;
            float3 dir = rootOffset;
            if (math.lengthsq(dir) > 0.01f)
            {
                dir = math.normalize(dir);
            }
            else
            {
                dir.y = -1.0f;
            }
            for (int i = 0; i < numberOfPoints; ++i)
            {
                var pos = float3.zero;
                pos.x = dir.x * i * restLength;
                pos.y = rootOffset.y;
                pos.z = dir.z * i * restLength;
                positions[i] = pos+ rootOffset;

                velocities[i] = float3.zero;
                forces[i] = float3.zero;
                masses[i] = 2.0f;
            }
            masses[numberOfPoints - 1] = 5.0f;

            for (int i = 0; i < numberOfEdges; ++i)
            {
                edges[i] = new Edge() { first = i, second = i + 1 };
            }


        }
        public void Dispose()
        {
            edges.Dispose();
            positions.Dispose();
            velocities.Dispose();
            forces.Dispose();
            constraints.Dispose();
            vectorData.Dispose();
            masses.Dispose();
            flags.Dispose();
            sphereCollisions.Dispose();

        }
        private void UpdateChain()
        {
            int numberOfPoints = positions.Length;
            int numberOfEdges = edges.Length;
            // Compute forces
            for (int i = 0; i < numberOfPoints; ++i)
            {
                // Gravity force
                forces[i] = masses[i] * gravity;
                // Air drag force
                float3 relativeVel = velocities[i];
                //if (wind != nullptr)
                //{
                //    relativeVel -= wind->sample(positions[i]);
                //} // todo.
               // if (constraints.Length > 0 && constraints[0].pointIndex != -1)
               
               if (math.abs(positions[i].y - floorPositionY) > 0.03f) // todo. get it from field. using sample()
                    relativeVel -= vectorData[0]; // wind
                forces[i] += -dragCoefficient * relativeVel;
            }

            for (int i = 0; i < numberOfEdges; ++i)
            {
                int pointIndex0 = edges[i].first;
                int pointIndex1 = edges[i].second;

                // Compute spring force
                float3 pos0 = positions[pointIndex0];
                float3 pos1 = positions[pointIndex1];
                float3 r = pos0 - pos1;
                
                float distance = math.length(r);
                float3 normalized = math.normalize(r);
                if (distance > 0.0)
                {
                    float3 force = -stiffness * (distance - restLength) * normalized;
                    forces[pointIndex0] += force;
                    forces[pointIndex1] -= force;
                }

                // Add damping force
                float3 vel0 = velocities[pointIndex0];
                float3 vel1 = velocities[pointIndex1];
                float3 relativeVel0 = vel0 - vel1;
                float3 damping = -dampingCoefficient * relativeVel0;
                forces[pointIndex0] += damping;
                forces[pointIndex1] -= damping;
            }
            // Update states
            for (int i = 0; i < numberOfPoints; ++i)
            {
                // Compute new states
                float3 newAcceleration = forces[i] / masses[i];
                float3 newVelocity = velocities[i] + timeIntervalInSeconds * newAcceleration;
                float3 newPosition = positions[i] + timeIntervalInSeconds * newVelocity;

                // Collision , ready for sphere only.
               // if ( i > 0 && (numberOfPoints > 2) ?  (i <  numberOfPoints - 2): false )
                if( ((flags[i] & 0x8000) == 0) && ((i == numberOfEdges-1) ? (hangGameObjectFlag == 0) :true)) // not attached.
                {
                    for (int j = 0; j < sphereCollisions.Length; j++)
                    {
                        if (sphereCollisions[j].enabled == 0)
                        {
                            continue;
                        }
                        float3 dst = newPosition - sphereCollisions[j].centerPosition;
                        float radious = sphereCollisions[j].radious + restLength * 0.5f;
                        float radiosSq = (radious * radious);
                        if (math.lengthsq(dst) < radiosSq)
                        {
                            float length = math.length(dst);
                            float3 repulse = newPosition - sphereCollisions[j].centerPosition;
                            newPosition = sphereCollisions[j].centerPosition 
                                + math.normalize(repulse) * (sphereCollisions[j].radious+restLength * 0.5f);
                            repulse.y = 0;
                            repulse = math.normalize(repulse) * radious * 0.00001f; // experiment. avoid being kept in one certain point.
                            newVelocity += repulse;
                        }
                    }

                }
                // floor
                if (newPosition.y < floorPositionY)
                {
                    newPosition.y = floorPositionY;

                    if (newVelocity.y < 0.0)
                    {
                        newVelocity.y *= -restitutionCoefficient;
                        newPosition.y += timeIntervalInSeconds * newVelocity.y;
                    }
                }

                // Update states
                velocities[i] = newVelocity;
                positions[i] = newPosition;
            }
            // Apply constraints
            for (int i = 0; i < constraints.Length; ++i)
            {
                int pointIndex = constraints[i].pointIndex;
                if (pointIndex < 0)
                    continue;
                if (pointIndex > 0 && hangGameObjectFlag != 0)
                {
                    // ignore pointindex more than 0 if hangGameObjectFlag is enabled
                    continue;
                }
                positions[pointIndex] = constraints[i].fixedPosition;
                velocities[pointIndex] = constraints[i].fixedVelocity;
            }
        }

        public void UpdateSphereCollision(int index, SphereCollision col)
        {
            if ( index < sphereCollisions.Length)
                sphereCollisions[index] = col;
        }
        
        public void ProcessAnimation(UnityEngine.Animations.AnimationStream stream)
        {
            //elapsed += timeIntervalInSeconds;
            /*
            if (targetEffector.IsValid(stream))
            {
                var scale = targetEffector.GetLocalScale(stream);
                //    Debug.Log($"scale = {scale.x}");
            }*/
            //Debug.Log($"ProcessAnimation::dampingCoefficient = {dampingCoefficient}");
            
            for ( int ii = 0; ii < 2; ii++) // 2 = 1/0.5
            { 
                UpdateChain();
            }
            if ( initialized == 0)
            {
                for ( int ii = 0; ii < 360; ii++)
                {
                    UpdateChain();
                }
            }
            initialized = 1;
            for (int i = 0; i < joints.Length; i++)
            {
                Vector3 from = UnitVector;
//                var vec = effectors[i + 1] - effectors[i];
//                Quaternion qt = Quaternion.FromToRotation(from.normalized, vec.normalized);


//                var rotation = effectors[i].GetLocalRotation(stream);

                //                rotation.SetEulerAngles(0.0f,0.0f,1.0f);
                //Debug.Log($"positions[{i}] = {positions[i].x}");
                //joints[i].SetLocalRotation(stream, rotation);

                joints[i].SetLocalPosition(stream, positions[i]);
            }
        }

        public void ProcessRootMotion(UnityEngine.Animations.AnimationStream stream) { }
    }
}