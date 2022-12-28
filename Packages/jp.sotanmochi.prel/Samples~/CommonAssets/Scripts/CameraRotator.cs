using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Prel.Samples
{
    public class CameraRotator : MonoBehaviour
    {
        public float Speed = 1.0f;
        public Transform CenterPosition;
        public float InitialRotDegree;

        private float _Radius = 1.0f;
        private float _InitialRot = 0.0f;
        private Vector2 _InitPosXZ = Vector2.zero;

        void Start()
        {
            Vector2 centerPosXZ = new Vector2(CenterPosition.position.x, CenterPosition.position.z);
            _InitPosXZ = new Vector2(transform.position.x, transform.position.z);
            _Radius = Vector2.Distance(_InitPosXZ, Vector2.zero);
            _InitialRot = Mathf.Deg2Rad * InitialRotDegree;
        }

        void Update()
        {
            float t = Time.realtimeSinceStartup;
            float posX = _Radius * Mathf.Sin(t * Speed + _InitialRot);
            float posZ = _Radius * Mathf.Cos(t * Speed + _InitialRot);
            float posY = transform.position.y;

            transform.position = new Vector3(posX, posY, posZ);
            transform.LookAt(CenterPosition, Vector3.up);
        }
    }
}