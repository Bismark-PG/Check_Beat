using UnityEngine;

public class CameraTopDownView : MonoBehaviour
{
    public Transform target;
    public float smoothSpeed = 0.125f;
    public Vector3 offset;

    void Start()
    {
    }

    void LateUpdate()
    {
        if (target == null)
        {
            if (GameManager.Instance != null && GameManager.Instance.playerTransform != null)
            {
                target = GameManager.Instance.playerTransform;

                if (offset == Vector3.zero)
                {
                    offset = new Vector3(0, 5f, -3.5f);
                }
            }
            else
            {
                return;
            }
        }

        Vector3 desiredPosition = target.position + offset;

        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;

        transform.LookAt(target);
    }
}