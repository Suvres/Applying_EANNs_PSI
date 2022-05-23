using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace PSI
{

    public class CarSensor : MonoBehaviour
    {
        [SerializeField]
        private int count;

        public int Count
        {
            get { return count; }
        }

        [SerializeField]
        private float angleCone;

        [SerializeField]
        private float maxDistance;

        [SerializeField]
        private LayerMask mask;

        private float[] values;

        public float[] Values
        {
            get { return values; }
        }

        public bool Active
        {
            get;
            set;
        }

        public void Start()
        {
            values = new float[count];
        }

        public void Update()
        {
            float startAngle = -angleCone / 2.0f;
            float diffAngle = angleCone / (count - 1);
            for (int i = 0; i < Count; i++)
            {
                float angle = startAngle + i * diffAngle;
                Vector3 direction = Quaternion.Euler(0.0f, angle, 0.0f) * transform.forward;

                RaycastHit hit;
                if(Physics.Raycast(transform.position, direction, out hit, maxDistance, mask))
                {
                    values[i] = hit.distance;
                }
            }
        }

        public void OnDrawGizmos()
        {
            if(Active || !EditorApplication.isPlaying)
            {
                Gizmos.color = Color.blue;

                float startAngle = -angleCone / 2.0f;
                float diffAngle = angleCone / (count-1);
                for(int i = 0; i < count; i++)
                {
                    float angle = startAngle + i*diffAngle;
                    Vector3 endPos = Quaternion.Euler(0.0f, angle, 0.0f) * transform.forward;
                    endPos *= maxDistance;
                    endPos += transform.position;
                    Gizmos.DrawLine(transform.position, endPos);
                }
            }
        }
    }

}