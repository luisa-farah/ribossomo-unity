#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using Object = UnityEngine.Object;

// Menu "Tutorial" na barra do Unity: monta o jogo "Ribossomo" e gera o build WebGL.
// Todos os sprites, efeitos sonoros e a música são gerados por código aqui (sem assets de terceiros).
public static class TutorialBuilder
{
    const string Title = "Ribossomo";
    static readonly Color Bg = new Color(0.06f, 0.07f, 0.13f);
    static readonly Color Membrane = new Color(0.55f, 0.36f, 0.66f);
    static readonly Color PlayerColor = new Color(0.80f, 0.74f, 1.00f);
    static readonly Color VirusColor = new Color(0.62f, 0.90f, 0.35f);
    static readonly Color ButtonColor = new Color(0.36f, 0.85f, 0.54f);
    static readonly Color PanelColor = new Color(0.03f, 0.04f, 0.08f, 0.85f);

    // arena interna: x [-7.1, 7.1], y [-4.6, 3.3]; HUD ocupa a faixa de cima
    static readonly Rect SpawnArea = new Rect(-6.6f, -4.1f, 13.2f, 6.9f);

    class Assets_
    {
        public Sprite circle, square, ribosome, amino, virus, atp, antibody, ring;
        public AudioClip collect, wrong, protein, hit, end, music, powerUp, shield;
    }

    [MenuItem("Tutorial/1. Montar projeto")]
    public static void BuildAll()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");

        foreach (var d in new[] { "Assets/Sprites", "Assets/Audio", "Assets/Prefabs", "Assets/Scenes" })
            Directory.CreateDirectory(d);

        var a = new Assets_
        {
            circle = MakeSprite("Assets/Sprites/Circle.png", 128, CircleShape),
            square = MakeSprite("Assets/Sprites/Square.png", 32, (x, y) => (1f, 1f)),
            ribosome = MakeSprite("Assets/Sprites/Ribossomo.png", 128, RibosomeShape),
            amino = MakeSprite("Assets/Sprites/Aminoacido.png", 128, AminoShape),
            virus = MakeSprite("Assets/Sprites/Virus.png", 128, VirusShape),
            atp = MakeSprite("Assets/Sprites/ATP.png", 128, StarShape),
            antibody = MakeSprite("Assets/Sprites/Anticorpo.png", 128, AntibodyShape),
            ring = MakeSprite("Assets/Sprites/Anel.png", 128, RingShape),
            powerUp = WriteWav("Assets/Audio/powerup.wav", SfxPowerUp()),
            shield = WriteWav("Assets/Audio/escudo.wav", SfxShield()),
            collect = WriteWav("Assets/Audio/coletar.wav", SfxCollect()),
            wrong = WriteWav("Assets/Audio/errado.wav", SfxWrong()),
            protein = WriteWav("Assets/Audio/proteina.wav", SfxProtein()),
            hit = WriteWav("Assets/Audio/dano.wav", SfxHit()),
            end = WriteWav("Assets/Audio/fim.wav", SfxEnd()),
            music = WriteWav("Assets/Audio/musica.wav", Music()),
        };

        var prefab = MakeAminoPrefab(a);
        var powerPrefab = MakePowerUpPrefab(a);
        BuildGameScene(a, prefab, powerPrefab);
        BuildMenuScene(a, "Assets/Scenes/MainMenu.unity", false);
        BuildMenuScene(a, "Assets/Scenes/EndGame.unity", true);

        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene("Assets/Scenes/MainMenu.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/Game.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/EndGame.unity", true),
        };
        PlayerSettings.productName = Title;
        PlayerSettings.defaultWebScreenWidth = 900;
        PlayerSettings.defaultWebScreenHeight = 600;
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;

        AssetDatabase.SaveAssets();
        EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");
        Debug.Log("[Tutorial] Projeto Ribossomo montado.");
    }

    [MenuItem("Tutorial/2. Build WebGL")]
    public static void BuildWebGL()
    {
        var opts = new BuildPlayerOptions
        {
            scenes = new[] { "Assets/Scenes/MainMenu.unity", "Assets/Scenes/Game.unity", "Assets/Scenes/EndGame.unity" },
            locationPathName = "Builds/WebGL",
            target = BuildTarget.WebGL,
            options = BuildOptions.None,
        };
        var report = BuildPipeline.BuildPlayer(opts);
        Debug.Log("[Tutorial] Build WebGL: " + report.summary.result);
    }

    // ================= Cenas =================

    static void BuildGameScene(Assets_ a, AminoAcid prefab, PowerUp powerPrefab)
    {
        var scene = NewSceneFromSample("Assets/Scenes/Game.unity");
        Decor(a);
        Camera.main.gameObject.AddComponent<CameraShake>();
        var fx = new GameObject("FX").AddComponent<FX>();
        fx.dot = a.circle; fx.font = UIFont;

        // Membrana celular (paredes) — passo 8
        MakeWall(a, "Membrana Topo", new Vector2(0, 3.45f), new Vector2(15f, 0.3f));
        MakeWall(a, "Membrana Base", new Vector2(0, -4.8f), new Vector2(15f, 0.4f));
        MakeWall(a, "Membrana Esq", new Vector2(-7.3f, -0.7f), new Vector2(0.4f, 8.6f));
        MakeWall(a, "Membrana Dir", new Vector2(7.3f, -0.7f), new Vector2(0.4f, 8.6f));

        // Jogador: ribossomo
        var player = new GameObject("Player (Ribossomo)");
        player.transform.position = new Vector2(0, -2.4f);
        player.transform.localScale = Vector3.one * 0.95f;
        var sr = player.AddComponent<SpriteRenderer>();
        sr.sprite = a.ribosome; sr.color = PlayerColor; sr.sortingOrder = 5;
        var rb = player.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f; rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.sleepMode = RigidbodySleepMode2D.NeverSleep;
        player.AddComponent<CircleCollider2D>().radius = 0.42f;
        player.AddComponent<PlayerMovement>();
        var health = player.AddComponent<PlayerHealth>();
        var ring = new GameObject("Escudo (anticorpo)");
        ring.transform.SetParent(player.transform, false);
        ring.transform.localScale = Vector3.one * 1.35f;
        var rsr = ring.AddComponent<SpriteRenderer>();
        rsr.sprite = a.ring; rsr.color = new Color(0.45f, 0.88f, 1f, 0.9f); rsr.sortingOrder = 6;
        health.shieldRing = ring;

        // Inimigos: vírus
        var viruses = new List<Virus>
        {
            MakeVirus(a, "Virus A", new Vector2(-6.0f, 0.8f), new Vector2(6.0f, 0.8f), 2.4f),
            MakeVirus(a, "Virus B", new Vector2(-3.2f, -4.0f), new Vector2(-3.2f, 2.6f), 1.8f),
            MakeVirus(a, "Virus C", new Vector2(3.4f, 2.6f), new Vector2(3.4f, -4.0f), 2.0f),
            MakeVirus(a, "Virus D (parado)", new Vector2(5.8f, -3.4f), new Vector2(5.8f, -3.4f), 0f),
        };

        // HUD
        var canvas = MakeCanvas();
        var panel = new GameObject("HUD Panel", typeof(RectTransform));
        panel.transform.SetParent(canvas, false);
        var prt = (RectTransform)panel.transform;
        prt.anchorMin = new Vector2(0, 1); prt.anchorMax = new Vector2(1, 1); prt.pivot = new Vector2(0.5f, 1);
        prt.sizeDelta = new Vector2(0, 84); prt.anchoredPosition = Vector2.zero;
        panel.AddComponent<Image>().color = PanelColor;

        var timeText = MakeText(canvas, "Tempo", "Tempo 0:40", 24, TextAnchor.UpperLeft, new Vector2(0, 1), new Vector2(20, -8), new Vector2(280, 32));
        var livesText = MakeText(canvas, "Vidas", "Vidas ♥♥♥", 24, TextAnchor.UpperCenter, new Vector2(0.5f, 1), new Vector2(0, -8), new Vector2(300, 32));
        var protText = MakeText(canvas, "Proteinas", "Proteínas: 0", 24, TextAnchor.UpperRight, new Vector2(1, 1), new Vector2(-20, -8), new Vector2(280, 32));
        var mrnaText = MakeText(canvas, "mRNA", "", 18, TextAnchor.UpperCenter, new Vector2(0.5f, 1), new Vector2(0, -38), new Vector2(860, 24));
        var chainText = MakeText(canvas, "Cadeia", "", 19, TextAnchor.UpperCenter, new Vector2(0.5f, 1), new Vector2(0, -59), new Vector2(860, 24));

        var statusText = MakeText(canvas, "Status", "", 15, TextAnchor.LowerLeft, new Vector2(0, 0), new Vector2(26, 3), new Vector2(700, 22));
        var flashGo = new GameObject("Flash de dano", typeof(RectTransform));
        flashGo.transform.SetParent(canvas, false);
        var frt = (RectTransform)flashGo.transform;
        frt.anchorMin = Vector2.zero; frt.anchorMax = Vector2.one; frt.offsetMin = Vector2.zero; frt.offsetMax = Vector2.zero;
        var flash = flashGo.AddComponent<Image>();
        flash.color = new Color(1f, 0.15f, 0.2f, 0f); flash.raycastTarget = false;

        // Controlador + áudio
        var ctrlGo = new GameObject("ProteinSynthesis (GameController)");
        var ctrl = ctrlGo.AddComponent<ProteinSynthesis>();
        var sfx = ctrlGo.AddComponent<AudioSource>();
        sfx.playOnAwake = false;
        ctrl.aminoPrefab = prefab;
        ctrl.powerUpPrefab = powerPrefab;
        ctrl.statusText = statusText;
        ctrl.damageFlash = flash;
        ctrl.powerUpClip = a.powerUp; ctrl.shieldClip = a.shield;
        ctrl.player = player.transform;
        ctrl.spawnArea = SpawnArea;
        ctrl.viruses = viruses.ToArray();
        ctrl.timeText = timeText; ctrl.livesText = livesText; ctrl.proteinsText = protText;
        ctrl.mrnaText = mrnaText; ctrl.chainText = chainText;
        ctrl.sfx = sfx;
        ctrl.collectClip = a.collect; ctrl.wrongClip = a.wrong; ctrl.proteinClip = a.protein;
        ctrl.hitClip = a.hit; ctrl.endClip = a.end;
        MakeMusic(a);
        MakeEventSystem(null);

        EditorSceneManager.SaveScene(scene, "Assets/Scenes/Game.unity");
    }

    static void BuildMenuScene(Assets_ a, string path, bool isEnd)
    {
        var scene = NewSceneFromSample(path);
        Decor(a);
        MakeMusic(a);

        // Ribossomo decorativo no menu
        var deco = new GameObject("Ribossomo (decor)");
        var dsr = deco.AddComponent<SpriteRenderer>();
        dsr.sprite = a.ribosome; dsr.color = PlayerColor; dsr.sortingOrder = 1;
        deco.transform.position = new Vector3(0, 2.6f, 0);
        deco.transform.localScale = Vector3.one * 1.4f;

        var canvas = MakeCanvas();
        var ctrl = new GameObject("MenuController").AddComponent<MenuController>();

        if (isEnd)
        {
            MakeText(canvas, "Titulo", "Fim de Jogo!", 54, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0, 75), new Vector2(800, 80));
            ctrl.reasonText = MakeText(canvas, "Motivo", "", 24, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0, 25), new Vector2(800, 36));
            ctrl.reasonText.color = new Color(1f, 0.55f, 0.55f);
            ctrl.resultText = MakeText(canvas, "Resultado", "", 22, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0, -40), new Vector2(800, 90));
            ctrl.recordText = MakeText(canvas, "Recorde", "", 24, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0, -105), new Vector2(800, 34));
            var again = MakeButton(canvas, a.square, "Jogar de novo", new Vector2(-140, -175));
            UnityEventTools.AddPersistentListener(again.onClick, ctrl.Play);
            var menu = MakeButton(canvas, a.square, "Menu", new Vector2(140, -175));
            UnityEventTools.AddPersistentListener(menu.onClick, ctrl.Menu);
            MakeEventSystem(again.gameObject);
        }
        else
        {
            MakeText(canvas, "Titulo", Title, 60, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0, 40), new Vector2(800, 80));
            MakeText(canvas, "Sub", "Traduza o mRNA: colete os aminoácidos na ordem certa e sintetize proteínas.", 20,
                TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0, -8), new Vector2(860, 30));
            var legend = "";
            foreach (var t in AminoData.All)
                legend += $"<color={AminoData.Hex(t)}><b>{t}</b></color> {AminoData.Codon(t)}     ";
            MakeText(canvas, "Codons", legend.TrimEnd(), 18, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0, -42), new Vector2(860, 28));
            var play = MakeButton(canvas, a.square, "Jogar", new Vector2(-135, -110));
            UnityEventTools.AddPersistentListener(play.onClick, ctrl.Play);
            var how = MakeButton(canvas, a.square, "Como jogar", new Vector2(135, -110));
            how.GetComponent<Image>().color = new Color(0.31f, 0.76f, 0.97f);
            UnityEventTools.AddPersistentListener(how.onClick, ctrl.ShowHowTo);
            ctrl.recordText = MakeText(canvas, "Recorde", "", 18, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0, -165), new Vector2(860, 26));
            ctrl.recordText.color = new Color(1f, 0.83f, 0.23f);
            MakeText(canvas, "Controles",
                "Mover: WASD / setas / controle      R / Select: reiniciar      Esc / Start: menu",
                16, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0, -200), new Vector2(860, 30));

            // ---- Painel "Como jogar" ----
            var panel = new GameObject("Painel Como Jogar", typeof(RectTransform));
            panel.transform.SetParent(canvas, false);
            var prt = (RectTransform)panel.transform;
            prt.anchorMin = Vector2.zero; prt.anchorMax = Vector2.one;
            prt.offsetMin = Vector2.zero; prt.offsetMax = Vector2.zero;
            panel.AddComponent<Image>().color = new Color(0.04f, 0.05f, 0.10f, 1f);
            var pt = panel.transform;

            string C(AminoType t) => $"<color={AminoData.Hex(t)}><b>{AminoData.Label(t)}</b></color>";
            const string ok = "<color=#5BD98A><b>✓</b></color>";
            const string no = "<color=#FF6B6B><b>✗</b></color>";

            MakeText(pt, "Titulo", "Como montar uma proteína", 34, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 1), new Vector2(0, -40), new Vector2(860, 50));

            var rules =
                $"1.  Toda proteína começa com {C(AminoType.Met)} (AUG) e termina com {C(AminoType.Stop)} (UAA), que a libera.\n" +
                "2.  Leia o mRNA no topo da tela da esquerda (5') para a direita (3'): cada códon = 1 peça.\n" +
                "3.  Colete na ordem. A próxima peça aparece destacada entre [colchetes].\n" +
                "4.  Errou a ordem? Perde 3 s, mas a proteína não recomeça — continue do mesmo ponto.\n" +
                "5.  Proteína pronta = +2 s por peça. Da 6ª em diante o HUD mostra só os códons: decore a tabela!";
            var rt = MakeText(pt, "Regras", rules, 18, TextAnchor.UpperLeft,
                new Vector2(0.5f, 1), new Vector2(0, -85), new Vector2(800, 150));
            rt.lineSpacing = 1.25f;

            string table = "";
            foreach (var t in AminoData.All) table += $"{C(t)} = {AminoData.Codon(t)}      ";
            MakeText(pt, "Tabela", "Códons:   " + table.TrimEnd(), 18, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 1), new Vector2(0, -250), new Vector2(860, 28));

            var examples =
                $"{ok}  AUG · GGC · UAA                →  {C(AminoType.Met)} → {C(AminoType.Gly)} → {C(AminoType.Stop)}\n" +
                $"{ok}  AUG · CUG · GCC · UAA        →  {C(AminoType.Met)} → {C(AminoType.Leu)} → {C(AminoType.Ala)} → {C(AminoType.Stop)}\n" +
                $"{no}  AUG · GGC · UUU                →  sem {C(AminoType.Stop)}, a proteína não é liberada\n" +
                $"{no}  GGC · AUG · UAA                →  não começa com {C(AminoType.Met)}";
            MakeText(pt, "Exemplos Titulo", "Exemplos", 22, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 1), new Vector2(0, -292), new Vector2(860, 30));
            var ex = MakeText(pt, "Exemplos", examples, 18, TextAnchor.UpperLeft,
                new Vector2(0.5f, 1), new Vector2(0, -320), new Vector2(620, 120));
            ex.lineSpacing = 1.3f;

            MakeText(pt, "Dica", "Vírus tiram 1 vida ♥ e aceleram a cada proteína.   Power-ups: ATP = velocidade · Anticorpo = escudo.", 16,
                TextAnchor.MiddleCenter, new Vector2(0.5f, 1), new Vector2(0, -455), new Vector2(860, 26)).color = new Color(1f, 0.75f, 0.75f);

            var back = MakeButton(pt, a.square, "Voltar (Esc)", new Vector2(0, -230));
            UnityEventTools.AddPersistentListener(back.onClick, ctrl.HideHowTo);

            ctrl.howToPanel = panel;
            ctrl.mainFirstButton = play.gameObject;
            ctrl.howToBackButton = back.gameObject;
            MakeEventSystem(play.gameObject);
        }
        EditorSceneManager.SaveScene(scene, path);
    }

    static UnityEngine.SceneManagement.Scene NewSceneFromSample(string path)
    {
        // herda Main Camera + Global Light 2D (sem luz os sprites ficam pretos no URP 2D)
        AssetDatabase.DeleteAsset(path);
        AssetDatabase.CopyAsset("Assets/Scenes/SampleScene.unity", path);
        var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
        var cam = Camera.main;
        if (cam != null)
        {
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Bg;
            cam.orthographicSize = 5f;
        }
        return scene;
    }

    // ================= Objetos =================

    static void Decor(Assets_ a)
    {
        // organelas translúcidas ao fundo (citoplasma)
        var parent = new GameObject("Citoplasma (decor)").transform;
        void Blob(Vector2 p, Vector2 s, Color c)
        {
            var g = new GameObject("Organela");
            g.transform.SetParent(parent);
            g.transform.position = p;
            g.transform.localScale = new Vector3(s.x, s.y, 1);
            var r = g.AddComponent<SpriteRenderer>();
            r.sprite = a.circle; r.color = c; r.sortingOrder = -10;
            var f = g.AddComponent<Floater>();
            f.amplitude = Mathf.Clamp(s.x * 0.08f, 0.1f, 0.4f);
        }
        Blob(new Vector2(-8.5f, -5.5f), new Vector2(9f, 9f), new Color(0.45f, 0.30f, 0.75f, 0.05f)); // núcleo
        Blob(new Vector2(5.2f, 1.9f), new Vector2(3.2f, 1.4f), new Color(1f, 0.55f, 0.30f, 0.025f));   // mitocôndria
        Blob(new Vector2(1.2f, -3.6f), new Vector2(2.6f, 1.1f), new Color(1f, 0.55f, 0.30f, 0.025f));
        Blob(new Vector2(-1.5f, 2.2f), new Vector2(1.2f, 1.2f), new Color(0.3f, 0.8f, 1f, 0.03f));     // vesículas
        Blob(new Vector2(6.0f, -1.8f), new Vector2(0.9f, 0.9f), new Color(0.3f, 0.8f, 1f, 0.03f));
        Blob(new Vector2(-5.0f, 1.3f), new Vector2(0.7f, 0.7f), new Color(0.3f, 0.8f, 1f, 0.03f));
    }

    static void MakeWall(Assets_ a, string name, Vector2 pos, Vector2 size)
    {
        var w = new GameObject(name);
        var sr = w.AddComponent<SpriteRenderer>();
        sr.sprite = a.square; sr.color = Membrane;
        w.transform.position = pos;
        w.transform.localScale = new Vector3(size.x, size.y, 1);
        w.AddComponent<BoxCollider2D>();
        w.AddComponent<MembranePulse>();
    }

    static Virus MakeVirus(Assets_ a, string name, Vector2 from, Vector2 to, float speed)
    {
        var g = new GameObject(name);
        g.transform.position = from;
        g.transform.localScale = Vector3.one * 0.9f;
        var sr = g.AddComponent<SpriteRenderer>();
        sr.sprite = a.virus; sr.color = VirusColor; sr.sortingOrder = 4;
        var rb = g.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        var col = g.AddComponent<CircleCollider2D>();
        col.isTrigger = true; col.radius = 0.32f;
        var v = g.AddComponent<Virus>();
        v.pointA = from; v.pointB = to; v.speed = speed;
        v.spin = speed > 0 ? 90f : 30f;
        return v;
    }

    static AminoAcid MakeAminoPrefab(Assets_ a)
    {
        var go = new GameObject("Aminoacido");
        go.transform.localScale = Vector3.one * 0.8f;
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = a.amino; sr.sortingOrder = 2;
        var col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true; col.radius = 0.5f;

        var label = new GameObject("Label");
        label.transform.SetParent(go.transform, false);
        var tm = label.AddComponent<TextMesh>();
        tm.font = UIFont; tm.fontSize = 64; tm.characterSize = 0.055f;
        tm.anchor = TextAnchor.MiddleCenter; tm.alignment = TextAlignment.Center;
        tm.fontStyle = FontStyle.Bold;
        tm.color = new Color(0.08f, 0.08f, 0.14f);
        tm.text = "Met";
        var mr = label.GetComponent<MeshRenderer>();
        mr.sharedMaterial = UIFont.material;
        mr.sortingOrder = 3;

        var amino = go.AddComponent<AminoAcid>();
        amino.label = tm;
        var prefab = PrefabUtility.SaveAsPrefabAsset(go, "Assets/Prefabs/Aminoacido.prefab");
        Object.DestroyImmediate(go);
        return prefab.GetComponent<AminoAcid>();
    }

    static PowerUp MakePowerUpPrefab(Assets_ a)
    {
        var go = new GameObject("PowerUp");
        go.transform.localScale = Vector3.one * 0.75f;
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = a.atp; sr.sortingOrder = 3;
        var col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true; col.radius = 0.45f;
        var pu = go.AddComponent<PowerUp>();
        pu.atpSprite = a.atp; pu.antibodySprite = a.antibody;
        var prefab = PrefabUtility.SaveAsPrefabAsset(go, "Assets/Prefabs/PowerUp.prefab");
        Object.DestroyImmediate(go);
        return prefab.GetComponent<PowerUp>();
    }

    static void MakeMusic(Assets_ a)
    {
        var g = new GameObject("Musica");
        var src = g.AddComponent<AudioSource>();
        src.clip = a.music; src.loop = true; src.playOnAwake = true; src.volume = 0.45f;
    }

    // ================= UI =================

    static Font UIFont => Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

    static Transform MakeCanvas()
    {
        var go = new GameObject("Canvas");
        var canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = go.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(900, 600);
        scaler.matchWidthOrHeight = 0.5f;
        go.AddComponent<GraphicRaycaster>();
        return go.transform;
    }

    static Text MakeText(Transform parent, string name, string content, int size, TextAnchor align,
        Vector2 anchor, Vector2 pos, Vector2 box)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rt = (RectTransform)go.transform;
        rt.anchorMin = anchor; rt.anchorMax = anchor; rt.pivot = anchor;
        rt.anchoredPosition = pos; rt.sizeDelta = box;
        var t = go.AddComponent<Text>();
        t.font = UIFont; t.fontSize = size; t.alignment = align; t.color = Color.white;
        t.supportRichText = true; t.text = content;
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        t.raycastTarget = false;
        return t;
    }

    static Button MakeButton(Transform parent, Sprite square, string label, Vector2 pos)
    {
        var go = new GameObject("Botao " + label, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rt = (RectTransform)go.transform;
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos; rt.sizeDelta = new Vector2(240, 58);
        var img = go.AddComponent<Image>();
        img.sprite = square; img.color = ButtonColor;
        var btn = go.AddComponent<Button>();
        var colors = btn.colors;
        colors.highlightedColor = new Color(0.85f, 1f, 0.9f);
        colors.selectedColor = new Color(0.85f, 1f, 0.9f);
        btn.colors = colors;
        var txt = MakeText(go.transform, "Label", label, 26, TextAnchor.MiddleCenter,
            new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(240, 58));
        txt.color = new Color(0.05f, 0.08f, 0.1f);
        return btn;
    }

    static void MakeEventSystem(GameObject firstSelected)
    {
        var es = new GameObject("EventSystem");
        var sys = es.AddComponent<EventSystem>();
        var module = es.AddComponent<InputSystemUIInputModule>(); // teclado, mouse e controle
        module.AssignDefaultActions();
        if (firstSelected != null) sys.firstSelectedGameObject = firstSelected;
    }

    // ================= Sprites gerados =================
    // cada função devolve (cinza, alpha) para o pixel normalizado (x,y em 0..1); a cor vem do SpriteRenderer.

    static (float, float) CircleShape(float x, float y)
    {
        float d = Vector2.Distance(new Vector2(x, y), new Vector2(0.5f, 0.5f));
        return (1f, Mathf.Clamp01((0.5f - d) * 128f));
    }

    static (float, float) AminoShape(float x, float y)
    {
        float d = Vector2.Distance(new Vector2(x, y), new Vector2(0.5f, 0.5f));
        float alpha = Mathf.Clamp01((0.47f - d) * 128f);
        float gray = d > 0.41f ? 0.5f : 0.88f;                                            // contorno
        if (Vector2.Distance(new Vector2(x, y), new Vector2(0.36f, 0.68f)) < 0.07f) gray = 1f; // brilho
        return (gray, alpha);
    }

    static float Ellipse(float x, float y, float cx, float cy, float rx, float ry)
    {
        float dx = (x - cx) / rx, dy = (y - cy) / ry;
        return Mathf.Sqrt(dx * dx + dy * dy);
    }

    static (float, float) RibosomeShape(float x, float y)
    {
        // subunidade maior (60S) embaixo, menor (40S) em cima, fita de mRNA atravessando
        float big = Ellipse(x, y, 0.5f, 0.38f, 0.45f, 0.31f);
        float small = Ellipse(x, y, 0.5f, 0.72f, 0.33f, 0.20f);
        bool mrna = Mathf.Abs(y - 0.55f) < 0.025f;
        if (big > 1f && small > 1f) return mrna ? (0.45f, 1f) : (0f, 0f);
        float gray = 1f;
        if ((big <= 1f && big > 0.88f) || (small <= 1f && small > 0.84f && big > 0.92f)) gray = 0.55f;
        if (mrna) gray = 0.45f;
        return (gray, 1f);
    }

    static (float, float) VirusShape(float x, float y)
    {
        var p = new Vector2(x, y) - new Vector2(0.5f, 0.5f);
        float r = p.magnitude;
        if (r < 0.27f) return (r > 0.23f ? 0.55f : 1f, 1f); // capsídeo
        for (int i = 0; i < 9; i++)                          // espículas
        {
            float ang = i * Mathf.PI * 2f / 9f;
            var dir = new Vector2(Mathf.Cos(ang), Mathf.Sin(ang));
            var knob = dir * 0.42f;
            if (Vector2.Distance(p, knob) < 0.06f) return (0.8f, 1f);
            float along = Vector2.Dot(p, dir);
            float perp = Mathf.Abs(p.x * dir.y - p.y * dir.x);
            if (along > 0.2f && along < 0.4f && perp < 0.022f) return (0.7f, 1f);
        }
        return (0f, 0f);
    }

    static (float, float) StarShape(float x, float y)
    {
        // estrela de 6 pontas (molécula de energia ATP estilizada)
        var p = new Vector2(x, y) - new Vector2(0.5f, 0.5f);
        float ang = Mathf.Atan2(p.y, p.x);
        float r = 0.3f + 0.16f * Mathf.Cos(ang * 6f);
        float d = p.magnitude;
        if (d > r) return (0f, 0f);
        return (d > r - 0.05f ? 0.6f : 1f, 1f);
    }

    static float SegDist(Vector2 p, Vector2 a, Vector2 b)
    {
        var ab = b - a;
        float t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / ab.sqrMagnitude);
        return Vector2.Distance(p, a + ab * t);
    }

    static (float, float) AntibodyShape(float x, float y)
    {
        // anticorpo em "Y"
        var p = new Vector2(x, y);
        var c = new Vector2(0.5f, 0.45f);
        float d = Mathf.Min(SegDist(p, new Vector2(0.5f, 0.1f), c),
                  Mathf.Min(SegDist(p, c, new Vector2(0.2f, 0.85f)), SegDist(p, c, new Vector2(0.8f, 0.85f))));
        if (d > 0.1f) return (0f, 0f);
        return (d > 0.07f ? 0.6f : 1f, 1f);
    }

    static (float, float) RingShape(float x, float y)
    {
        float d = Vector2.Distance(new Vector2(x, y), new Vector2(0.5f, 0.5f));
        return Mathf.Abs(d - 0.44f) < 0.035f ? (1f, 1f) : (0f, 0f);
    }

    static Sprite MakeSprite(string path, int size, Func<float, float, (float, float)> shape)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        var px = new Color32[size * size];
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                var (g, al) = shape((x + 0.5f) / size, (y + 0.5f) / size);
                byte gb = (byte)(Mathf.Clamp01(g) * 255), ab = (byte)(Mathf.Clamp01(al) * 255);
                px[y * size + x] = new Color32(gb, gb, gb, ab);
            }
        tex.SetPixels32(px);
        File.WriteAllBytes(path, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);
        AssetDatabase.ImportAsset(path);
        var imp = (TextureImporter)AssetImporter.GetAtPath(path);
        imp.textureType = TextureImporterType.Sprite;
        imp.spriteImportMode = SpriteImportMode.Single;
        imp.spritePixelsPerUnit = size;
        imp.alphaIsTransparency = true;
        imp.mipmapEnabled = false;
        imp.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    // ================= Áudio gerado =================

    const int Rate = 22050;
    static float Midi(int n) => 440f * Mathf.Pow(2f, (n - 69) / 12f);
    static float Sq(float ph) => Mathf.Repeat(ph, 1f) < 0.5f ? 1f : -1f;
    static float Tri(float ph) => 4f * Mathf.Abs(Mathf.Repeat(ph, 1f) - 0.5f) - 1f;

    static float[] Tone(float[] freqs, float noteLen, float vol, Func<float, float> wave, float decay)
    {
        int per = (int)(Rate * noteLen);
        var s = new float[per * freqs.Length];
        for (int k = 0; k < freqs.Length; k++)
            for (int i = 0; i < per; i++)
            {
                float t = (float)i / Rate;
                s[k * per + i] = wave(freqs[k] * t) * vol * Mathf.Exp(-t * decay);
            }
        return s;
    }

    static float[] SfxCollect() => Tone(new[] { Midi(79), Midi(84) }, 0.07f, 0.35f, Sq, 14f);
    static float[] SfxProtein() => Tone(new[] { Midi(72), Midi(76), Midi(79), Midi(84), Midi(88) }, 0.08f, 0.35f, Sq, 6f);
    static float[] SfxEnd() => Tone(new[] { Midi(67), Midi(63), Midi(60), Midi(55) }, 0.16f, 0.35f, Tri, 3f);

    static float[] SfxPowerUp() => Tone(new[] { Midi(67), Midi(71), Midi(74), Midi(79), Midi(83), Midi(86) }, 0.05f, 0.3f, Tri, 4f);
    static float[] SfxShield() => Tone(new[] { Midi(88), Midi(81), Midi(76) }, 0.07f, 0.3f, Sq, 8f);

    static float[] SfxWrong()
    {
        int n = (int)(Rate * 0.28f);
        var s = new float[n];
        for (int i = 0; i < n; i++)
        {
            float t = (float)i / Rate;
            float f = Mathf.Lerp(180f, 90f, t / 0.28f);
            s[i] = Sq(f * t) * 0.3f * (1f - t / 0.28f);
        }
        return s;
    }

    static float[] SfxHit()
    {
        int n = (int)(Rate * 0.35f);
        var s = new float[n];
        var rng = new System.Random(7);
        for (int i = 0; i < n; i++)
        {
            float t = (float)i / Rate;
            float noise = (float)(rng.NextDouble() * 2 - 1);
            s[i] = (noise * 0.5f + Mathf.Sin(2 * Mathf.PI * Mathf.Lerp(220f, 60f, t / 0.35f) * t) * 0.5f) * 0.45f * Mathf.Exp(-t * 9f);
        }
        return s;
    }

    static float[] Music()
    {
        // Lá menor: Am – F – C – G (x2), 110 bpm; baixo triangular + arpejo quadrado suave
        const float bpm = 110f;
        float eighth = 60f / bpm / 2f;
        int[] roots = { 45, 41, 48, 43 };
        int[][] arps = { new[] { 69, 72, 76, 81 }, new[] { 65, 69, 72, 77 }, new[] { 67, 72, 76, 79 }, new[] { 67, 71, 74, 79 } };
        int[] pattern = { 0, 1, 2, 3, 2, 1, 2, 1 };
        int bars = 8;
        int perEighth = (int)(Rate * eighth);
        var s = new float[perEighth * 8 * bars];
        for (int bar = 0; bar < bars; bar++)
        {
            int c = bar % 4;
            for (int e = 0; e < 8; e++)
            {
                int start = (bar * 8 + e) * perEighth;
                float fa = Midi(arps[c][pattern[e]]);
                float fb = Midi(roots[c]);
                for (int i = 0; i < perEighth; i++)
                {
                    float t = (float)i / Rate;
                    float tb = (float)((e % 2) * perEighth + i) / Rate; // baixo em semínimas
                    float arp = Sq(fa * t) * 0.07f * Mathf.Exp(-t * 7f);
                    float bass = Tri(fb * tb) * 0.22f * Mathf.Exp(-tb * 2.5f);
                    float hat = (e % 2 == 1 && i < 300) ? (float)(new System.Random(start + i).NextDouble() * 2 - 1) * 0.03f : 0f;
                    s[start + i] = arp + bass + hat;
                }
            }
        }
        return s;
    }

    static AudioClip WriteWav(string path, float[] samples)
    {
        using (var bw = new BinaryWriter(File.Create(path)))
        {
            int dataLen = samples.Length * 2;
            bw.Write(System.Text.Encoding.ASCII.GetBytes("RIFF")); bw.Write(36 + dataLen);
            bw.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt ")); bw.Write(16);
            bw.Write((short)1); bw.Write((short)1); bw.Write(Rate); bw.Write(Rate * 2);
            bw.Write((short)2); bw.Write((short)16);
            bw.Write(System.Text.Encoding.ASCII.GetBytes("data")); bw.Write(dataLen);
            foreach (var v in samples) bw.Write((short)(Mathf.Clamp(v, -1f, 1f) * short.MaxValue));
        }
        AssetDatabase.ImportAsset(path);
        return AssetDatabase.LoadAssetAtPath<AudioClip>(path);
    }
}
#endif
