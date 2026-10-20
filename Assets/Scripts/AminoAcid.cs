using UnityEngine;

public enum AminoType { Met, Gly, Phe, Leu, Ala }

// Tabela simplificada do código genético (um códon por aminoácido).
public static class AminoData
{
    public static readonly AminoType[] All = { AminoType.Met, AminoType.Gly, AminoType.Phe, AminoType.Leu, AminoType.Ala };

    public static string Codon(AminoType t) => t switch
    {
        AminoType.Met => "AUG",
        AminoType.Gly => "GGC",
        AminoType.Phe => "UUU",
        AminoType.Leu => "CUG",
        _ => "GCC",
    };

    public static string Hex(AminoType t) => t switch
    {
        AminoType.Met => "#5BD98A",
        AminoType.Gly => "#4FC3F7",
        AminoType.Phe => "#FFA94D",
        AminoType.Leu => "#F06595",
        _ => "#FFD43B",
    };

    public static Color Color(AminoType t)
    {
        ColorUtility.TryParseHtmlString(Hex(t), out var c);
        return c;
    }
}

// Coletável (passo 12/13): prefab com trigger, o tipo define cor e rótulo.
public class AminoAcid : MonoBehaviour
{
    public AminoType type;
    public TextMesh label;
    [HideInInspector] public bool collected;

    Vector3 baseScale;
    float phase;

    void Awake()
    {
        baseScale = transform.localScale;
        phase = Random.value * 10f;
    }

    void Start() => SetType(type);

    public void SetType(AminoType t)
    {
        type = t;
        GetComponent<SpriteRenderer>().color = AminoData.Color(t);
        if (label != null) label.text = t.ToString();
    }

    void Update()
    {
        // leve "respiração" para dar vida ao coletável
        float s = 1f + 0.06f * Mathf.Sin((Time.time + phase) * 3f);
        transform.localScale = baseScale * s;
    }
}
