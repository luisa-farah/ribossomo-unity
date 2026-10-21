using UnityEngine;
using UnityEngine.SceneManagement;

// Passo 17: GameManager estático — estado da partida que sobrevive à troca de cenas.
public static class GameManager
{
    public const int MaxLives = 3;
    const string BestProteinsKey = "ribossomo_best_proteins";
    const string BestTimeKey = "ribossomo_best_time";

    public static int Proteins { get; private set; }
    public static int Lives { get; private set; }
    public static float TimeLeft { get; private set; }
    public static float Elapsed { get; private set; }
    public static string LastProtein { get; private set; } = "";
    public static bool NewRecord { get; private set; }
    public static string EndReason { get; set; } = "";

    public static int BestProteins => PlayerPrefs.GetInt(BestProteinsKey, 0);
    public static float BestTime => PlayerPrefs.GetFloat(BestTimeKey, 0f);

    public static void StartGame(float startTime)
    {
        Proteins = 0;
        Lives = MaxLives;
        TimeLeft = startTime;
        Elapsed = 0f;
        LastProtein = "";
        NewRecord = false;
        EndReason = "";
    }

    public static void Tick(float dt)
    {
        TimeLeft = Mathf.Max(0f, TimeLeft - dt);
        Elapsed += dt;
    }

    public static void AddTime(float seconds) => TimeLeft = Mathf.Max(0f, TimeLeft + seconds);

    public static void CompleteProtein(float bonus, string protein)
    {
        Proteins++;
        LastProtein = protein;
        AddTime(bonus);
    }

    public static void LoseLife() => Lives = Mathf.Max(0, Lives - 1);

    // Recorde: mais proteínas; empate → mais tempo de sobrevivência. Salvo no navegador (PlayerPrefs).
    public static void SaveRecord()
    {
        bool better = Proteins > BestProteins || (Proteins == BestProteins && Proteins > 0 && Elapsed > BestTime);
        if (!better) return;
        PlayerPrefs.SetInt(BestProteinsKey, Proteins);
        PlayerPrefs.SetFloat(BestTimeKey, Elapsed);
        PlayerPrefs.Save();
        NewRecord = true;
    }

    public static string FormatTime(float t)
    {
        int s = Mathf.CeilToInt(t);
        return $"{s / 60}:{s % 60:00}";
    }

    public static void LoadMenu() => SceneManager.LoadScene("MainMenu");
    public static void LoadGame() => SceneManager.LoadScene("Game");
    public static void LoadEndGame() => SceneManager.LoadScene("EndGame");
}
