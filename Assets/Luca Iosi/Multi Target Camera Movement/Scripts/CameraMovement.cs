using System.Collections.Generic;
using UnityEngine;

namespace MultiTargetCameraMovement
{
    [RequireComponent(typeof(Camera))]
    public class CameraMovement : MonoBehaviour
    {
        public List<Transform> targetList;

        [Header("Position")]
        public Vector3 offset;

        [Header("Zoom")]
        public float smoothTime = 0.5f;
        public float minZoom = 40;
        public float maxZoom = 80;
        public float zoomLimiter = 50f;

        [Header("Rotation")]
        public bool automaticallyLookAtCenter = false;
        [HideInInspector]
        public float Pitch = 50;
        [HideInInspector]
        public float Yaw;
        [HideInInspector]
        public float Roll;

        private Vector3 velocity;
        private Camera camera;

        private void Start()
        {
            camera = GetComponent<Camera>();
            targetList = new List<Transform>();

            // Find all players tagged as "Player"
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            foreach (GameObject player in players)
            {
                targetList.Add(player.transform);
            }
        }

        private void LateUpdate()
        {
            if (targetList.Count == 0)
                return;
            MoveAndRotate();
            Zoom();
        }

        void MoveAndRotate()
        {
            Vector3 centerPoint = GetCenterPoint();
            transform.position = Vector3.SmoothDamp(transform.position, centerPoint + offset, ref velocity, smoothTime);
            if (automaticallyLookAtCenter)
            {
                camera.transform.LookAt(new Vector3(camera.transform.position.x, centerPoint.y, centerPoint.z));
            }
            else
            {
                var rotation = Quaternion.Euler(Pitch, Yaw, Roll);
                camera.transform.rotation = rotation;
            }
        }

        void Zoom()
        {
            float newZoom = Mathf.Lerp(maxZoom, minZoom, GetGreatestDistance() / zoomLimiter);
            camera.fieldOfView = Mathf.Lerp(camera.fieldOfView, newZoom, Time.deltaTime);
        }

        float GetGreatestDistance()
        {
            var bounds = new Bounds(targetList[0].position, Vector3.zero);
            for (int i = 0; i < targetList.Count; i++)
            {
                bounds.Encapsulate(targetList[i].position);
            }
            return bounds.size.x;
        }

        Vector3 GetCenterPoint()
        {
            if (targetList.Count == 1)
            {
                return targetList[0].position;
            }
            var bounds = new Bounds(targetList[0].position, Vector3.zero);
            for (int i = 0; i < targetList.Count; i++)
            {
                bounds.Encapsulate(targetList[i].position);
            }
            return bounds.center;
        }

        public void AddTarget(Transform target)
        {
            targetList.Add(target);
        }

        public void RemoveTarget(Transform target)
        {
            targetList.Remove(target);
        }

        private void ResetTargets()
        {
            targetList = new List<Transform>();
        }
    }
}

