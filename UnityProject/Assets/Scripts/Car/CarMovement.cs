using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PSI
{

    public class CarMovement : MonoBehaviour
    {

        private const float MAX_VEL = 10f;
        private const float ACCELERATION = 4f;
        private const float VEL_FRICT = 2f;
        private const float TURN_SPEED = 50;
        
        [SerializeField]
        private float verticalInput;
        [SerializeField]
        private float horizontalInput;

        private float Velocity;
        private Quaternion Rotation;

        private CarController Controller;

        public void Awake()
        {
            Controller = GetComponent<CarController>();
        }

        public void FixedUpdate()
        {
            if (!Controller.Agent.Running)
            {
                Velocity = 0.0f;
                return;
            }

            ApplyInput();
            ApplyVelocity();
            ApplyFriction();
        }

        public void SetInputs(float[] inputs)
        {
            verticalInput = Mathf.Clamp(inputs[0], -1.0f, 1.0f);
            horizontalInput = Mathf.Clamp(inputs[1], -1.0f, 1.0f);
        }

        private void ApplyInput()
        {
            //Car can only accelerate further if velocity is lower than engineForce * MAX_VEL
            bool canAccelerate = false;
            if (verticalInput < 0)
                canAccelerate = Velocity > verticalInput * MAX_VEL;
            else if (verticalInput > 0)
                canAccelerate = Velocity < verticalInput * MAX_VEL;

            //Set velocity
            if (canAccelerate)
            {
                Velocity += (float)verticalInput * ACCELERATION * Time.deltaTime;

                //Cap velocity
                if (Velocity > MAX_VEL)
                    Velocity = MAX_VEL;
                else if (Velocity < -MAX_VEL)
                    Velocity = -MAX_VEL;
            }

            //Set rotation
            Rotation = transform.rotation;
            Rotation *= Quaternion.AngleAxis((float)-horizontalInput * TURN_SPEED * Time.deltaTime, new Vector3(0, 1, 0));
        }

        private void ApplyVelocity()
        {
            Vector3 direction = new Vector3(0, 0, 1);
            transform.rotation = Rotation;
            direction = Rotation * direction;

            this.transform.position += direction * Velocity * Time.deltaTime;
        }

        // Applies some friction to velocity
        private void ApplyFriction()
        {
            if (verticalInput == 0)
            {
                if (Velocity > 0)
                {
                    Velocity -= VEL_FRICT * Time.deltaTime;
                    if (Velocity < 0)
                        Velocity = 0;
                }
                else if (Velocity < 0)
                {
                    Velocity += VEL_FRICT * Time.deltaTime;
                    if (Velocity > 0)
                        Velocity = 0;
                }
            }
        }

        public void Reset()
        {
            Velocity = 0.0f;
            Rotation = Quaternion.identity;
        }
    }

}