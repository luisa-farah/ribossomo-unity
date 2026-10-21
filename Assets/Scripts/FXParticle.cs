using UnityEngine;

public class FXParticle : MonoBehaviour
{
    public Vector2 velocity;
    public float life = 0.5f;

    SpriteRenderer sr;
    Vector3 baseScale;
    float t;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        baseScale = transform.localScale;
    }

    void Update()
    {
        t += Time.deltaTime;
        transform.position += (Vector3)(velocity * Time.deltaTime);
        velocity *= 0.92f;
        float k = 1f - t / life;
        var c = sr.color; c.a = k; sr.color = c;
        transform.localScale = baseScale * Mathf.Max(k, 0.1f);
        if (t >= life) Destroy(gameObject);
    }
}
