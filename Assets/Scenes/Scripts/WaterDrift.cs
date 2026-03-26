using UnityEngine;

public class WaterDrift : MonoBehaviour
{
    public float speed = 0.12f;
    public float wrapWidth = 8f;

    private Vector3 startPosition;
    private float lastRealtime;

    private void Awake()
    {
        startPosition = transform.localPosition;
        lastRealtime = Time.realtimeSinceStartup;
    }

    private void OnEnable()
    {
        startPosition = transform.localPosition;
        lastRealtime = Time.realtimeSinceStartup;
    }

    private void Update()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        float now = Time.realtimeSinceStartup;
        float deltaTime = now - lastRealtime;
        lastRealtime = now;

        if (deltaTime <= 0f || deltaTime > 0.25f)
        {
            deltaTime = 1f / 60f;
        }

        transform.localPosition += Vector3.right * (speed * deltaTime);
        float delta = transform.localPosition.x - startPosition.x;

        if (delta > wrapWidth)
        {
            transform.localPosition = startPosition;
        }
        else if (delta < -wrapWidth)
        {
            transform.localPosition = startPosition;
        }
    }
}
