using UnityEngine;

public enum PowerUpType { ATP, Anticorpo }

// Power-up temporário: ATP (velocidade) ou anticorpo (escudo). Some depois de alguns segundos.
public class PowerUp : MonoBehaviour
{
    public PowerUpType type;
    public Sprite atpSprite;
    public Sprite antibodySprite;
    public float lifetime = 8f;
    [HideInInspector] public bool collected;

    SpriteRenderer sr;
    Vector3 baseScale;
    float t;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        baseScale = transform.localScale;
    }

    public void SetType(PowerUpType p)
    {
        type = p;
        sr.sprite = p == PowerUpType.ATP ? atpSprite : antibodySprite;
        sr.color = p == PowerUpType.ATP ? new Color(1f, 0.85f, 0.2f) : new Color(0.45f, 0.88f, 1f);
    }

    void Update()
    {
        t += Time.deltaTime;
        transform.localScale = baseScale * (1f + 0.12f * Mathf.Sin(t * 6f));
        transform.Rotate(0f, 0f, 45f * Time.deltaTime);
        if (t > lifetime - 2f) sr.enabled = Mathf.Repeat(t, 0.2f) > 0.1f; // pisca antes de sumir
        if (t >= lifetime) Destroy(gameObject);
    }
}
