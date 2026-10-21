using UnityEngine;

public enum AminoType { Met, Gly, Phe, Leu, Ala, Stop }

// Tabela simplificada do código genético (um códon por aminoácido + códon de parada).
public static class AminoData
{
    public static readonly AminoType[] All = { AminoType.Met, AminoType.Gly, AminoType.Phe, AminoType.Leu, AminoType.Ala, AminoType.Stop };
    public static readonly AminoType[] Coding = { AminoType.Gly, AminoType.Phe, AminoType.Leu, AminoType.Ala };

    public static string Codon(AminoType t) => t switch
    {
        AminoType.Met => "AUG",
        AminoType.Gly => "GGC",
        AminoType.Phe => "UUU",
        AminoType.Leu => "CUG",
        AminoType.Ala => "GCC",
        _ => "UAA",
    };

    public static string Hex(AminoType t) => t switch
    {
        AminoType.Met => "#5BD98A",
        AminoType.Gly => "#4FC3F7",
        AminoType.Phe => "#FFA94D",
        AminoType.Leu => "#F06595",
        AminoType.Ala => "#FFD43B",
        _ => "#E9ECEF",
    };

    public static string Label(AminoType t) => t == AminoType.Stop ? "STOP" : t.ToString();

    public static Color Color(AminoType t)
    {
        ColorUtility.TryParseHtmlString(Hex(t), out var c);
        return c;
    }
}

// Coletável (passos 12/13): prefab com trigger; o tipo define cor e rótulo.
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
        if (label != null)
        {
            label.text = AminoData.Label(t);
            label.characterSize = t == AminoType.Stop ? 0.042f : 0.055f;
        }
    }

    void Update()
    {
        float s = 1f + 0.06f * Mathf.Sin((Time.time + phase) * 3f);
        transform.localScale = baseScale * s;
    }
}
