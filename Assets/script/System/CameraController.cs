using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float rotateSpeed = 100f;
    public float zoomSpeed = 10f;
    public float minY = 5f;
    public float maxY = 20f;

    private Transform target;

    // 平滑跟隨的速度
    public float smoothSpeed = 5f;
    // 偏移量
    public Vector3 offset = new Vector3(0, 10, -10);


    void Update()
    {
        // 旋轉相機
        if (Input.GetMouseButton(1)) // 右鍵拖動旋轉
        {
            float h = rotateSpeed * Input.GetAxis("Mouse X") * Time.deltaTime;
            float v = rotateSpeed * Input.GetAxis("Mouse Y") * Time.deltaTime;
            transform.Rotate(Vector3.up, h, Space.World);
            transform.Rotate(Vector3.right, -v, Space.Self);
        }

        // 縮放相機
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0.0f)
        {
            Vector3 pos = transform.position;
            pos.y -= scroll * zoomSpeed;
            pos.y = Mathf.Clamp(pos.y, minY, maxY);
            transform.position = pos;
        }
        if (target != null)
        {
            // 目標位置
            Vector3 desiredPosition = target.position + offset;
            // 平滑過渡
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
            transform.position = smoothedPosition;

            // 讓攝影機始終看向目標
            Quaternion desiredRotation = Quaternion.LookRotation(target.position - transform.position);
            Quaternion smoothedRotation = Quaternion.Lerp(transform.rotation, desiredRotation, smoothSpeed * Time.deltaTime);
            transform.rotation = smoothedRotation;
        }
    }

    // 設置新的目標
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}
