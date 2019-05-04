using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace jp.geometry
{
    public class DemoController : MonoBehaviour
    {
        public AnimationJobTest controllerHangingGameObject;
        public AnimationJobTest controller0;
        public AnimationJobTest controller1;
        public AnimationJobTest controller2;
        public AnimationJobTest controller3;

        float elapsed = 0.0f;
        private Status status;
        private enum Status
        {
            started,
 
            drop0,
            drop1,
            drop2,

            dropDone,
        }

        public void Reset()
        {
            elapsed = 0;
            if (controllerHangingGameObject != null)
            {
                controllerHangingGameObject.Reset();
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
                    if (controller0 != null)
                    {
                        controller0.DetachCeiling();
                    }
                    status = Status.drop0;
                    break;
                
                case Status.drop0:
                    if (controllerHangingGameObject != null)
                    {
                        controllerHangingGameObject.SetLastEffectorMass(50.0f);
                    }

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
//
//                    status = Status.drop2;
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
                    if (controller2 != null)
                    {
                        controller2.DetachGameObject();
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
            if ( status == Status.drop1)
            {
                elapsed += Time.deltaTime;
                if ( elapsed >= 0.8f)
                {
                    if (controllerHangingGameObject != null)
                    {
                        controllerHangingGameObject.DetachCeiling();
                        controllerHangingGameObject.DetachGameObject();
                    }
                    status = Status.drop2;
                }
            }
        }
    }
}