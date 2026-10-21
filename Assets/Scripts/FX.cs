using UnityEngine;

// Efeitos visuais simples: partículas e textos flutuantes (sem assets externos).
public class FX : MonoBehaviour
{
    public static FX Instance { get; private set; }

    public Sprite dot;
    public Font font;

    void Awake() => Instance = this;

    public static void Burst(Vector2 pos, Color color, int count = 12, float speed = 3.5f)
    {
        if (Instance == null) return;
        for (int i = 0; i < count; i++)
        {
            var g = new GameObject("Particula");
            g.transform.position = pos;
            g.transform.localScale = Vector3.one * Random.Range(0.1f, 0.2f);
            var sr = g.AddComponent<SpriteRenderer>();
            sr.sprite = Instance.dot; sr.color = color; sr.sortingOrder = 8;
            var p = g.AddComponent<FXParticle>();
            p.velocity = Random.insideUnitCircle.normalized * Random.Range(speed * 0.5f, speed);
        }
    }

    public static void Popup(Vector2 pos, string text, Color color)
    {
        if (Instance == null) return;
        var g = new GameObject("Popup");
        g.transform.position = pos + Vector2.up * 0.4f;
        var tm = g.AddComponent<TextMesh>();
        tm.font = Instance.font; tm.fontSize = 64; tm.characterSize = 0.06f;
        tm.anchor = TextAnchor.MiddleCenter; tm.fontStyle = FontStyle.Bold;
        tm.text = text; tm.color = color;
        var mr = g.GetComponent<MeshRenderer>();
        mr.sharedMaterial = Instance.font.material;
        mr.sortingOrder = 9;
        g.AddComponent<FloatingText>();
    }
}
