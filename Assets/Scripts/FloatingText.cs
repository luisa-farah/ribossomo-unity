using UnityEngine;

public class FloatingText : MonoBehaviour
{
    public float life = 0.9f;
    public float rise = 1.2f;

    TextMesh tm;
    Color baseColor;
    float t;

    void Start()
    {
        tm = GetComponent<TextMesh>();
        baseColor = tm.color;
    }

    void Update()
    {
        t += Time.deltaTime;
        transform.position += Vector3.up * rise * Time.deltaTime;
        var c = baseColor; c.a = 1f - t / life; tm.color = c;
        if (t >= life) Destroy(gameObject);
    }
}
