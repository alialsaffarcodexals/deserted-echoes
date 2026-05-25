using UnityEngine;

public class CameraFollow2D : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0f, 0f, -10f);
    public float smoothTime = 0.08f;
    Vector3 velocity = Vector3.zero;

    void LateUpdate()
    {
        if (target == null) return;
        Vector3 targetPos = target.position + offset;
        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref velocity, smoothTime);
    }

    // Call this after a teleport to instantly snap the camera and kill damping velocity.
    public void Warp(Vector2 pos)
    {
        transform.position = new Vector3(pos.x, pos.y, transform.position.z) + offset;
        velocity = Vector3.zero;
    }
}
