using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// Controlador da cena Game: tradução do mRNA, cronômetro, vidas, power-ups, HUD, feedback e fim de jogo.
public class ProteinSynthesis : MonoBehaviour
{
    public static ProteinSynthesis Instance { get; private set; }

    [Header("Arena")]
    public AminoAcid aminoPrefab;
    public PowerUp powerUpPrefab;
    public Transform player;
    public Rect spawnArea = new Rect(-6.6f, -4.1f, 13.2f, 6.9f);
    public int perType = 2;
    public Virus[] viruses;
    public float virusSpeedUp = 1.08f;

    [Header("Tempo")]
    public float startTime = 40f;
    public float wrongPenalty = 3f;
    public float bonusPerAmino = 2f;

    [Header("Dificuldade")]
    [Tooltip("A partir de quantas proteínas o HUD mostra só os códons")]
    public int codonsOnlyFrom = 5; // 6ª proteína em diante
    public float powerUpInterval = 11f;
    public float boostDuration = 4f;
    public float boostMultiplier = 1.6f;

    [Header("HUD")]
    public Text timeText;
    public Text livesText;
    public Text proteinsText;
    public Text mrnaText;
    public Text chainText;
    public Text statusText;
    public Image damageFlash;

    [Header("Áudio")]
    public AudioSource sfx;
    public AudioClip collectClip, wrongClip, proteinClip, hitClip, endClip, powerUpClip, shieldClip;

    static readonly Color Green = new Color(0.36f, 0.85f, 0.54f);
    static readonly Color Red = new Color(1f, 0.42f, 0.42f);
    static readonly Color Cyan = new Color(0.45f, 0.88f, 1f);
    static readonly Color Gold = new Color(1f, 0.85f, 0.2f);

    readonly List<AminoType> target = new List<AminoType>();
    int index;
    bool ending;
    float wrongFlash;
    float powerUpTimer;
    PowerUp currentPowerUp;
    PlayerMovement playerMove;
    PlayerHealth playerHealth;

    bool CodonsOnly => GameManager.Proteins >= codonsOnlyFrom;

    void Awake() => Instance = this;

    void Start()
    {
        GameManager.StartGame(startTime);
        playerMove = player.GetComponent<PlayerMovement>();
        playerHealth = player.GetComponent<PlayerHealth>();
        foreach (var t in AminoData.All)
            for (int i = 0; i < perType; i++) Spawn(t);
        NewProtein();
        powerUpTimer = powerUpInterval * 0.6f;
    }

    void Update()
    {
        if (!ending)
        {
            GameManager.Tick(Time.deltaTime);
            TickPowerUps();
            if (GameManager.TimeLeft <= 0f || GameManager.Lives <= 0) End();
        }
        wrongFlash -= Time.deltaTime;

        if (damageFlash != null)
        {
            var c = damageFlash.color;
            c.a = Mathf.MoveTowards(c.a, 0f, Time.unscaledDeltaTime * 1.5f);
            damageFlash.color = c;
        }

        // saídas sempre disponíveis (sem softlock)
        var kb = Keyboard.current;
        var gp = Gamepad.current;
        if ((kb != null && kb.rKey.wasPressedThisFrame) || (gp != null && gp.selectButton.wasPressedThisFrame))
            GameManager.LoadGame();
        if ((kb != null && kb.escapeKey.wasPressedThisFrame) || (gp != null && gp.startButton.wasPressedThisFrame))
            GameManager.LoadMenu();

        UpdateHud();
    }

    // ---------- Eventos do jogador ----------

    public void OnAminoCollected(AminoAcid amino)
    {
        if (ending) return;
        var type = amino.type;
        Vector2 pos = amino.transform.position;
        Destroy(amino.gameObject);
        Spawn(type); // sempre repõe: nunca falta o aminoácido necessário

        if (type == target[index])
        {
            index++;
            FX.Burst(pos, AminoData.Color(type));
            if (index >= target.Count) CompleteProtein(pos);
            else sfx.PlayOneShot(collectClip);
        }
        else
        {
            GameManager.AddTime(-wrongPenalty);
            wrongFlash = 0.5f;
            sfx.PlayOneShot(wrongClip);
            FX.Popup(pos, $"−{wrongPenalty:0}s", Red);
            CameraShake.Shake(0.12f, 0.06f);
        }
    }

    void CompleteProtein(Vector2 pos)
    {
        float bonus = bonusPerAmino * target.Count;
        var names = new List<string>();
        foreach (var a in target) if (a != AminoType.Stop) names.Add(a.ToString());
        GameManager.CompleteProtein(bonus, string.Join("-", names));

        sfx.PlayOneShot(proteinClip);
        FX.Popup(pos, $"Proteína! +{bonus:0}s", Green);
        FX.Burst(player.position, Color.white, 24, 5f);
        CameraShake.Shake(0.15f, 0.08f);
        foreach (var v in viruses) v.speed *= virusSpeedUp;

        if (GameManager.Proteins == codonsOnlyFrom)
            FX.Popup((Vector2)player.position + Vector2.up * 0.6f, "Modo avançado: leia os códons!", Gold);
        NewProtein();
    }

    public void OnPlayerHit()
    {
        if (ending) return;
        GameManager.LoseLife();
        sfx.PlayOneShot(hitClip);
        FX.Popup(player.position, "−1 ♥", Red);
        CameraShake.Shake(0.3f, 0.18f);
        if (damageFlash != null) { var c = damageFlash.color; c.a = 0.45f; damageFlash.color = c; }
    }

    public void OnShieldBlocked()
    {
        sfx.PlayOneShot(shieldClip);
        FX.Popup(player.position, "Escudo!", Cyan);
        FX.Burst(player.position, Cyan, 16, 4f);
    }

    public void OnPowerUp(PowerUp p)
    {
        if (ending) return;
        Vector2 pos = p.transform.position;
        if (p.type == PowerUpType.ATP)
        {
            playerMove.Boost(boostDuration, boostMultiplier);
            FX.Popup(pos, "ATP! Velocidade", Gold);
            FX.Burst(pos, Gold, 16);
        }
        else
        {
            playerHealth.GiveShield();
            FX.Popup(pos, "Anticorpo! Escudo", Cyan);
            FX.Burst(pos, Cyan, 16);
        }
        sfx.PlayOneShot(powerUpClip);
        Destroy(p.gameObject);
    }

    // ---------- Regras ----------

    void NewProtein()
    {
        int coding = Mathf.Min(1 + GameManager.Proteins / 2, 4); // proteínas crescem com o tempo
        target.Clear();
        target.Add(AminoType.Met); // códon de início AUG
        for (int i = 0; i < coding; i++)
            target.Add(AminoData.Coding[Random.Range(0, AminoData.Coding.Length)]);
        target.Add(AminoType.Stop); // códon de parada UAA libera a proteína
        index = 0;
    }

    void TickPowerUps()
    {
        if (currentPowerUp != null) return;
        powerUpTimer -= Time.deltaTime;
        if (powerUpTimer > 0f) return;
        powerUpTimer = powerUpInterval;
        currentPowerUp = Instantiate(powerUpPrefab, FindFreeSpot(), Quaternion.identity);
        currentPowerUp.SetType(Random.value < 0.5f ? PowerUpType.ATP : PowerUpType.Anticorpo);
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
        GameManager.SaveRecord();
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

    // ---------- HUD ----------

    void UpdateHud()
    {
        float t = GameManager.TimeLeft;
        timeText.text = "Tempo " + GameManager.FormatTime(t);
        timeText.color = wrongFlash > 0f || t < 10f ? Red : Color.white;

        var hearts = new StringBuilder("Vidas ");
        for (int i = 0; i < GameManager.MaxLives; i++)
            hearts.Append(i < GameManager.Lives ? "<color=#FF6B6B>♥</color>" : "<color=#555A6E>♥</color>");
        if (playerHealth != null && playerHealth.HasShield) hearts.Append(" <color=#73E0FF>+escudo</color>");
        livesText.text = hearts.ToString();

        proteinsText.text = "Proteínas: " + GameManager.Proteins;

        bool hide = CodonsOnly;
        var mrna = new StringBuilder("mRNA  5'  ");
        var chain = new StringBuilder();
        for (int i = 0; i < target.Count; i++)
        {
            var a = target[i];
            string codon = AminoData.Codon(a);
            string name = AminoData.Label(a);
            if (i < index)
            {
                mrna.Append("<color=#6B7089>").Append(codon).Append("</color>");
                chain.Append("<color=").Append(AminoData.Hex(a)).Append(">").Append(name).Append("</color>");
            }
            else if (i == index)
            {
                mrna.Append("<b><color=#FFFFFF>[").Append(codon).Append("]</color></b>");
                chain.Append(hide ? "<b>[ ? ]</b>" : $"<b><color={AminoData.Hex(a)}>[{name}]</color></b>");
            }
            else
            {
                mrna.Append(codon);
                chain.Append("<color=#6B7089>").Append(hide ? "?" : name).Append("</color>");
            }
            if (i < target.Count - 1) { mrna.Append("  "); chain.Append("  →  "); }
        }
        mrna.Append("  3'");
        mrnaText.text = mrna.ToString();
        chainText.text = chain.ToString();

        if (statusText != null)
        {
            var s = new StringBuilder();
            if (playerMove != null && playerMove.BoostRemaining > 0f)
                s.Append($"<color=#FFD43B>ATP: velocidade {playerMove.BoostRemaining:0.0}s</color>   ");
            if (hide) s.Append("<color=#FFFFFF>Modo avançado: leia o códon</color>");
            statusText.text = s.ToString();
        }
    }
}
