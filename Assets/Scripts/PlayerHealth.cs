using UnityEngine;

// Dano por vírus + invencibilidade temporária (pisca).
public class PlayerHealth : MonoBehaviour
{
    public float invulnerableTime = 1.5f;

    SpriteRenderer sr;
    float invul;

    void Start() => sr = GetComponent<SpriteRenderer>();

    void Update()
    {
        if (invul <= 0f) return;
        invul -= Time.deltaTime;
        sr.enabled = invul <= 0f || Mathf.Repeat(invul, 0.2f) > 0.1f;
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (invul > 0f || !other.GetComponent<Virus>()) return;
        invul = invulnerableTime;
        ProteinSynthesis.Instance.OnPlayerHit();
    }
}
