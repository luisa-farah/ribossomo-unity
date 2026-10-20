using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// Controlador da cena Game: tradução do mRNA, cronômetro, vidas, HUD e fim de jogo.
public class ProteinSynthesis : MonoBehaviour
{
    public static ProteinSynthesis Instance { get; private set; }

    [Header("Arena")]
    public AminoAcid aminoPrefab;
    public Transform player;
    public Rect spawnArea = new Rect(-6.6f, -4.1f, 13.2f, 6.9f);
    public int perType = 2;
    public Virus[] viruses;
    public float virusSpeedUp = 1.08f;

    [Header("Tempo")]
    public float startTime = 40f;
    public float wrongPenalty = 3f;
    public float bonusPerAmino = 2f;

    [Header("HUD")]
    public Text timeText;
    public Text livesText;
    public Text proteinsText;
    public Text mrnaText;
    public Text chainText;

    [Header("Áudio")]
    public AudioSource sfx;
    public AudioClip collectClip, wrongClip, proteinClip, hitClip, endClip;

    readonly List<AminoType> target = new List<AminoType>();
    int index;
    bool ending;
    float wrongFlash;

    void Awake() => Instance = this;

    void Start()
    {
        GameManager.StartGame(startTime);
        foreach (var t in AminoData.All)
            for (int i = 0; i < perType; i++) Spawn(t);
        NewProtein();
    }

    void Update()
    {
        if (!ending)
        {
            GameManager.Tick(Time.deltaTime);
            if (GameManager.TimeLeft <= 0f || GameManager.Lives <= 0) End();
        }
        wrongFlash -= Time.deltaTime;

        // saídas sempre disponíveis (sem softlock)
        var kb = Keyboard.current;
        var gp = Gamepad.current;
        if ((kb != null && kb.rKey.wasPressedThisFrame) || (gp != null && gp.selectButton.wasPressedThisFrame))
            GameManager.LoadGame();
        if ((kb != null && kb.escapeKey.wasPressedThisFrame) || (gp != null && gp.startButton.wasPressedThisFrame))
            GameManager.LoadMenu();

        UpdateHud();
    }

    public void OnAminoCollected(AminoAcid amino)
    {
        if (ending) return;
        var type = amino.type;
        Destroy(amino.gameObject);
        Spawn(type); // sempre repõe: nunca falta o aminoácido necessário

        if (type == target[index])
        {
            index++;
            if (index >= target.Count)
            {
                GameManager.CompleteProtein(bonusPerAmino * target.Count);
                sfx.PlayOneShot(proteinClip);
                foreach (var v in viruses) v.speed *= virusSpeedUp;
                NewProtein();
            }
            else sfx.PlayOneShot(collectClip);
        }
        else
        {
            GameManager.AddTime(-wrongPenalty);
            wrongFlash = 0.5f;
            sfx.PlayOneShot(wrongClip);
        }
    }

    public void OnPlayerHit()
    {
        if (ending) return;
        GameManager.LoseLife();
        sfx.PlayOneShot(hitClip);
    }

    void NewProtein()
    {
        int len = Mathf.Min(3 + GameManager.Proteins / 2, 6); // fica maior com o tempo
        target.Clear();
        target.Add(AminoType.Met); // códon de início AUG
        for (int i = 1; i < len; i++)
            target.Add(AminoData.All[Random.Range(1, AminoData.All.Length)]);
        index = 0;
    }

    void Spawn(AminoType type)
    {
        var a = Instantiate(aminoPrefab, FindFreeSpot(), Quaternion.identity);
        a.SetType(type);
    }

    Vector2 FindFreeSpot()
    {
        Vector2 p = Vector2.zero;
        for (int i = 0; i < 40; i++)
        {
            p = new Vector2(Random.Range(spawnArea.xMin, spawnArea.xMax), Random.Range(spawnArea.yMin, spawnArea.yMax));
            bool farFromPlayer = player == null || Vector2.Distance(p, player.position) > 1.8f;
            if (farFromPlayer && Physics2D.OverlapCircle(p, 0.65f) == null) return p;
        }
        return p;
    }

    void End()
    {
        ending = true;
        GameManager.EndReason = GameManager.Lives <= 0 ? "Os vírus destruíram o ribossomo!" : "A célula ficou sem tempo!";
        sfx.PlayOneShot(endClip);
        Time.timeScale = 0.3f; // câmera lenta dramática
        Invoke(nameof(GoToEnd), 0.35f);
    }

    void GoToEnd()
    {
        Time.timeScale = 1f;
        GameManager.LoadEndGame();
    }

    void OnDestroy() => Time.timeScale = 1f;

    void UpdateHud()
    {
        float t = GameManager.TimeLeft;
        timeText.text = "Tempo " + GameManager.FormatTime(t);
        timeText.color = wrongFlash > 0f || t < 10f ? new Color(1f, 0.35f, 0.35f) : Color.white;

        var hearts = new StringBuilder("Vidas ");
        for (int i = 0; i < GameManager.MaxLives; i++)
            hearts.Append(i < GameManager.Lives ? "<color=#FF6B6B>♥</color>" : "<color=#555A6E>♥</color>");
        livesText.text = hearts.ToString();

        proteinsText.text = "Proteínas: " + GameManager.Proteins;

        var mrna = new StringBuilder("mRNA  5'  ");
        var chain = new StringBuilder();
        for (int i = 0; i < target.Count; i++)
        {
            var a = target[i];
            string codon = AminoData.Codon(a);
            if (i < index)
            {
                mrna.Append("<color=#6B7089>").Append(codon).Append("</color>");
                chain.Append("<color=").Append(AminoData.Hex(a)).Append(">").Append(a).Append("</color>");
            }
            else if (i == index)
            {
                mrna.Append("<b>[").Append(codon).Append("]</b>");
                chain.Append("<b><color=").Append(AminoData.Hex(a)).Append(">[").Append(a).Append("]</color></b>");
            }
            else
            {
                mrna.Append(codon);
                chain.Append("<color=#6B7089>").Append(a).Append("</color>");
            }
            if (i < target.Count - 1) { mrna.Append("  "); chain.Append("  →  "); }
        }
        mrna.Append("  3'");
        mrnaText.text = mrna.ToString();
        chainText.text = chain.ToString();
    }
}
