using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace jp.geometry
{
    public class DemoController : MonoBehaviour
    {
        public GameObject LargeSphere;
        public AnimationJobTest controller0;
        public AnimationJobTest controller1;
        public AnimationJobTest controller2;
        public AnimationJobTest controller3;


        private Status status;
        private enum Status
        {
            started,
            isKinmaticOff,
 
            drop0,
            drop1,
            drop2,

            dropDone,
        }

        public void Reset()
        {
            if (LargeSphere != null)
            {
                LargeSphere.GetComponent<Transform>().rotation = Quaternion.Euler(Vector3.zero);
                var rb = LargeSphere.GetComponent<Rigidbody>();
                rb.isKinematic = true;
            }
            if (controller0 != null)
            {
                controller0.Reset();
            }
            if (controller1 != null)
            {
                controller1.Reset();
            }
            if (controller2 != null)
            {
                controller2.Reset();
            }
            if (controller3 != null)
            {
                controller3.Reset();
            }
            status = Status.started;
        }

        public void NextStatus()
        {
            switch (status)
            {
                case Status.started:
                    if (LargeSphere != null)
                    {
                        LargeSphere.GetComponent<Transform>().rotation = Quaternion.Euler(Vector3.zero);
                        var rb = LargeSphere.GetComponent<Rigidbody>();
                        rb.isKinematic = false;
                    }
                    status = Status.isKinmaticOff;
                    break;
                
                case Status.isKinmaticOff:
                    if (controller0 != null)
                    {
                        controller0.DetachCeiling();
                    }
                    status = Status.drop0;
                    break;
                case Status.drop0:
                    if (controller1 != null)
                    {
                        controller1.DetachGameObject();
                    }
                    if (controller0 != null)
                    {
                        controller0.DetachGameObject();
                    }
                    status = Status.drop1;
                    break;
                case Status.drop1:
                    if (controller2 != null)
                    {
                        controller2.DetachGameObject();
                    }
                    status = Status.drop2;
                    break;
                case Status.drop2:
                    if (controller3 != null)
                    {
                        controller3.DetachCeiling();
                    }
                    if (controller2 != null)
                    {
                        controller2.DetachCeiling();
                    }
                    status = Status.dropDone;
                    break;

                
                case Status.dropDone:
                    Reset();
                    break;

            }

        }
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}