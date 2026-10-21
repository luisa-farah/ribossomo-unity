using UnityEngine;

// Membrana celular pulsando (onda de brilho que percorre as paredes).
public class MembranePulse : MonoBehaviour
{
    public Color highlight = new Color(0.78f, 0.6f, 0.9f);
    public float speed = 2f;

    SpriteRenderer sr;
    Color baseColor;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        baseColor = sr.color;
    }

    void Update()
    {
        float phase = (transform.position.x + transform.position.y) * 0.3f;
        float k = (Mathf.Sin(Time.time * speed + phase) + 1f) * 0.5f;
        sr.color = Color.Lerp(baseColor, highlight, k * 0.35f);
    }
}
