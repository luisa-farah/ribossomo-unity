using UnityEngine;

// Dano por vírus, invencibilidade temporária (pisca) e escudo do anticorpo.
public class PlayerHealth : MonoBehaviour
{
    public float invulnerableTime = 1.5f;
    public GameObject shieldRing;

    public bool HasShield { get; private set; }

    SpriteRenderer sr;
    float invul;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        if (shieldRing != null) shieldRing.SetActive(false);
    }

    public void GiveShield()
    {
        HasShield = true;
        if (shieldRing != null) shieldRing.SetActive(true);
    }

    void Update()
    {
        if (shieldRing != null && HasShield)
            shieldRing.transform.localScale = Vector3.one * (1.35f + 0.06f * Mathf.Sin(Time.time * 8f));

        if (invul <= 0f) return;
        invul -= Time.deltaTime;
        sr.enabled = invul <= 0f || Mathf.Repeat(invul, 0.2f) > 0.1f;
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (invul > 0f || !other.GetComponent<Virus>()) return;

        if (HasShield)
        {
            HasShield = false;
            if (shieldRing != null) shieldRing.SetActive(false);
            invul = 1f;
            ProteinSynthesis.Instance.OnShieldBlocked();
            return;
        }
        invul = invulnerableTime;
        ProteinSynthesis.Instance.OnPlayerHit();
    }
}
