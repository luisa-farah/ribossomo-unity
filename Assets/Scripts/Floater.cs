using UnityEngine;

// Fundo vivo: organelas flutuam e "respiram".
public class Floater : MonoBehaviour
{
    public float amplitude = 0.25f;
    public float speed = 0.3f;
    public float pulse = 0.04f;

    Vector3 startPos, startScale;
    float phase;

    void Start()
    {
        startPos = transform.position;
        startScale = transform.localScale;
        phase = Random.value * 10f;
    }

    void Update()
    {
        float t = Time.time * speed + phase;
        transform.position = startPos + new Vector3(Mathf.Sin(t) * amplitude, Mathf.Cos(t * 0.8f) * amplitude * 0.7f, 0f);
        transform.localScale = startScale * (1f + Mathf.Sin(t * 1.7f) * pulse);
    }
}
