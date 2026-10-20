using UnityEngine;

// Inimigo: patrulha entre dois pontos (ou fica parado se A == B).
[RequireComponent(typeof(Rigidbody2D))]
public class Virus : MonoBehaviour
{
    public Vector2 pointA;
    public Vector2 pointB;
    public float speed = 2f;
    public float spin = 60f;

    Rigidbody2D rb;
    float t;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.position = pointA;
    }

    void FixedUpdate()
    {
        float dist = Vector2.Distance(pointA, pointB);
        if (dist > 0.01f)
        {
            t += speed * Time.fixedDeltaTime / dist;
            float k = Mathf.SmoothStep(0f, 1f, Mathf.PingPong(t, 1f));
            rb.MovePosition(Vector2.Lerp(pointA, pointB, k));
        }
        rb.MoveRotation(rb.rotation + spin * Time.fixedDeltaTime);
    }
}
