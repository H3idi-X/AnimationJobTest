using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

using UnityEngine.Playables;
using UnityEngine.Animations;
namespace jp.geometry
{

    public class AnimationJobTest : MonoBehaviour
    {
        public int m_maxSpringJoint;
        public float floorPositionY = 0.0f;
        public Vector3 rootOffset;
        public float restLength = 0.5f;
        public Material GameObjectMaterial;
        public bool HangGameObject = false;
        public WindGenerator windGenerator;
        public GameObject[] Collisions;
        // controll spring
        /*
        public float stiffness = 500.0f;
        public float dampingCoefficient = 1.0f;
        public float dragCoefficient = 0.1f;
        public float restitutionCoefficient = 0.3f;
        */

        public GameObject m_ceilingGameObject;
        public bool m_ceilingGameObjectAttached = true;
        public GameObject m_targetGameObject;
        public bool m_targetGameObjectAttached = true;
        GameObject m_rootGameObject;
        GameObject[] m_SphereGameObjects;
        GameObject[] m_CapsuleGameObjects;
        private AnimationJob job;
        private NativeArray<TransformStreamHandle> joints;
 //       private TransformSceneHandle targetEffector;
        //        private NativeArray<TransformSceneHandle> effectors;
        private PlayableGraph graph;

        // Start is called before the first frame update
        void Start()
        {
            graph = PlayableGraph.Create(name);

            if (m_maxSpringJoint <= 0)
            {
                m_maxSpringJoint = 4;
            }

            // points.
            m_SphereGameObjects = new GameObject[m_maxSpringJoint];
            for ( int ii = 0; ii < m_maxSpringJoint;ii++)
            {
                m_SphereGameObjects[ii] = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                m_SphereGameObjects[ii].layer = 8; // todo.
                m_SphereGameObjects[ii].transform.position = 
                    new Vector3(0.0f, floorPositionY - (float) ii, 0.0f);
                m_SphereGameObjects[ii].name = "joint" + ii;
                if (GameObjectMaterial != null)
                {
                    var renderer =
                        m_SphereGameObjects[ii].GetComponent<MeshRenderer>();
                    renderer.material = GameObjectMaterial;
                }

            }
#if false
            // capusles.
            m_CapsuleGameObjects = new GameObject[m_maxSpringJoint - 1];
            for (int ii = 0; ii < m_maxSpringJoint - 1; ii++)
            {
                m_CapsuleGameObjects[ii] = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                m_CapsuleGameObjects[ii].transform.position = new Vector3(0.0f, -0.5f + (float)-ii, 0.0f);
            }
#endif
            m_rootGameObject = new GameObject();
            // Parent-child relationship.
#if false
            for (int ii = 1; ii < m_maxSpringJoint; ii++)
            {
                m_SphereGameObjects[ii].transform.parent = m_SphereGameObjects[ii - 1].transform;
            }
            if (m_CapsuleGameObjects != null)
            {
                for (int ii = 0; ii < m_maxSpringJoint - 1; ii++)
                {
                    m_CapsuleGameObjects[ii].transform.parent = m_SphereGameObjects[ii].transform;

                }
            }
            var animator = m_SphereGameObjects[0].AddComponent<Animator>();

            UnityEngine.AvatarBuilder.BuildGenericAvatar(
                m_SphereGameObjects[0], m_SphereGameObjects[0].name);

#else
            for (int ii = 0; ii < m_maxSpringJoint; ii++)
            {
                m_SphereGameObjects[ii].transform.parent = m_rootGameObject.transform;
            }
            var animator = m_rootGameObject.AddComponent<Animator>();

            UnityEngine.AvatarBuilder.BuildGenericAvatar(
                m_rootGameObject, m_rootGameObject.name);

#endif
            joints = new NativeArray<TransformStreamHandle>(m_maxSpringJoint, Allocator.Persistent);
            for ( int ii = 0; ii< m_maxSpringJoint;ii++)
            {
                joints[ii] = animator.BindStreamTransform(m_SphereGameObjects[ii].transform);
            }


            //           AnimatorUtility.OptimizeTransformHierarchy(m_SphereGameObjects[0], null);
#if false

            effectors = new NativeArray<TransformSceneHandle>(m_maxSpringJoint, Allocator.Persistent);
            for ( int ii = 0; ii< m_maxSpringJoint;ii++)
            {
                effectors[ii] = animator.BindSceneTransform(new GameObject().transform);
            };
#endif
            // targetEffector = animator.BindSceneTransform(m_targetGameObject.transform);

            job = new AnimationJob()
            {
                //                effectors = effectors,
                joints = joints,
                floorPositionY = floorPositionY,
                rootOffset = rootOffset,
                restLength = restLength,
                hangGameObjectFlag = HangGameObject ? 1:0,
                //    targetEffector = targetEffector,

            };
            job.SetUp();
            var playable = AnimationScriptPlayable.Create(graph, job);


            AnimationPlayableUtilities.Play(animator, playable, graph);
         }

        // Update is called once per frame
        void Update()
        {
            if (m_ceilingGameObject != null)
            {
                if ( m_ceilingGameObjectAttached )
                    job.SetConstraint(0, m_ceilingGameObject.transform.position);
                else
                    job.ResetConstraint(0);
            }
            else 
            {
                if (m_ceilingGameObjectAttached)
                    job.SetConstraint(0, rootOffset);
                else
                    job.ResetConstraint(0);
            }
            if (m_targetGameObject != null )
            {
                if ( HangGameObject == false)
                {
                    if (m_targetGameObjectAttached)
                        job.SetConstraint(1, m_targetGameObject.transform.position);
                    else
                        job.ResetConstraint(1);
                }
                else
                {
                    
                }
            }
            if (windGenerator != null)
            {
                job.SetVectorData(0, windGenerator.result);
            }

            if (Collisions != null)
            {
                for ( int i = 0; i < Collisions.Length;i++)
                {
                    job.UpdateSphereCollision(
                        i,
                        new AnimationJob.SphereCollision()
                        {
                            enabled = 1,
                            centerPosition = Collisions[i].transform.position,
                            radious = Collisions[i].transform.localScale.x * 0.5f
                        }
                        );
                }
            }
            /*
            job.SetSpringParameter(
                            stiffness,
                            dampingCoefficient,
                            dragCoefficient,
                            restitutionCoefficient);*/
        }
        private void LateUpdate()
        {
            if (HangGameObject)
            {
                int length = m_SphereGameObjects.Length;
                if ( length > 0)
                {
                    var srcGo = m_SphereGameObjects[length - 1];
                    var dstGo = m_targetGameObject;
                    Vector3 offset = Vector3.zero;
                    if (dstGo.transform.parent != null)
                    {
                        offset = dstGo.transform.localPosition;
                        dstGo = dstGo.transform.parent.gameObject;
                        // go = go.transform.parent.gameObject;
                    }
                    var rb = dstGo.GetComponent<Rigidbody>();

                    offset.x *= dstGo.transform.localScale.x;
                    offset.y *= dstGo.transform.localScale.y;
                    offset.z *= dstGo.transform.localScale.z;
                    
                    if ( (rb != null  && rb.isKinematic) || rb == null)
                    {
                        dstGo.transform.position =
                            srcGo.transform.position - offset;
                    }

                }
            }

        }

        public void SetLastEffectorMass(float mass)
        {
            int length = m_SphereGameObjects.Length;
            if (length < 2)
                return;
            job.SetEffectorMass(length-1,mass);
        }

        public void Reset()
        {
            m_targetGameObjectAttached = true;
            m_ceilingGameObjectAttached = true;
            if (HangGameObject)
            {
                if (m_targetGameObject != null)
                {
                    SetIsKinmatic(m_targetGameObject,true);
                }
            }
            SetLastEffectorMass(5.0f);
        }

        public void DetachCeiling()
        {
            m_ceilingGameObjectAttached = false;
        }

        public void DetachGameObject()
        {
            m_targetGameObjectAttached = false;
            if ( HangGameObject )
            {
                if (m_targetGameObject != null)
                {
                    SetIsKinmatic(m_targetGameObject, false);

                }
            }
        }
        void SetIsKinmatic(GameObject go, bool flag)
        {
            var rb = go.GetComponent<Rigidbody>();
            while (rb == null)
            {
                var parent = go.transform.parent;
                if (parent == null)
                {
                    return;
                }
                go = parent.gameObject;
                rb = go.GetComponent<Rigidbody>();
            }
            go.GetComponent<Transform>().rotation = Quaternion.Euler(Vector3.zero);
            rb.isKinematic = flag;
        }
        void OnDestroy()
        {
//            AnimatorUtility.DeoptimizeTransformHierarchy(m_SphereGameObjects[0]);
            job.Dispose();
            graph.Destroy();
//            effectors.Dispose();
            joints.Dispose();
        }
    }
}