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

    // ���Ƹ��H���t��
    public float smoothSpeed = 5f;
    // �����q
    public Vector3 offset = new Vector3(0, 10, -10);


    void Update()
    {
        // ����۾�
        if (Input.GetMouseButton(1)) // �k���ʱ���
        {
            float h = rotateSpeed * Input.GetAxis("Mouse X") * Time.deltaTime;
            float v = rotateSpeed * Input.GetAxis("Mouse Y") * Time.deltaTime;
            transform.Rotate(Vector3.up, h, Space.World);
            transform.Rotate(Vector3.right, -v, Space.Self);
        }

        // �Y��۾�
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
            // �ؼЦ�m
            Vector3 desiredPosition = target.position + offset;
            // ���ƹL��
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
            transform.position = smoothedPosition;

            // ����v���l�׬ݦV�ؼ�
            Quaternion desiredRotation = Quaternion.LookRotation(target.position - transform.position);
            Quaternion smoothedRotation = Quaternion.Lerp(transform.rotation, desiredRotation, smoothSpeed * Time.deltaTime);
            transform.rotation = smoothedRotation;
        }
    }

    // �]�m�s���ؼ�
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}
