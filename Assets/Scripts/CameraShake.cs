using UnityEngine;

public class CameraShake : MonoBehaviour
{
    static CameraShake instance;
    Vector3 basePos;
    float duration, magnitude;

    void Awake()
    {
        instance = this;
        basePos = transform.localPosition;
    }

    public static void Shake(float dur, float mag)
    {
        if (instance == null) return;
        instance.duration = Mathf.Max(instance.duration, dur);
        instance.magnitude = Mathf.Max(instance.magnitude, mag);
    }

    void LateUpdate()
    {
        if (duration <= 0f) return;
        duration -= Time.unscaledDeltaTime;
        transform.localPosition = basePos + (Vector3)(Random.insideUnitCircle * magnitude);
        if (duration <= 0f)
        {
            magnitude = 0f;
            transform.localPosition = basePos;
        }
    }
}
