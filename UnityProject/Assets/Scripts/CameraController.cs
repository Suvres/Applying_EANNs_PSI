using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PSI
{
    public class CameraController : MonoBehaviour
    {
        public void SetPosition(Vector3 NewPosition)
        {
            transform.position = new Vector3(NewPosition.x, transform.position.y, NewPosition.z);
        }
    }
}