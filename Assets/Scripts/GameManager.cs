using UnityEngine;
using UnityEngine.SceneManagement;

// Passo 17: GameManager estático — estado da partida que sobrevive à troca de cenas.
public static class GameManager
{
    public const int MaxLives = 3;

    public static int Proteins { get; private set; }
    public static int Lives { get; private set; }
    public static float TimeLeft { get; private set; }
    public static float Elapsed { get; private set; }
    public static string EndReason { get; set; } = "";

    public static void StartGame(float startTime)
    {
        Proteins = 0;
        Lives = MaxLives;
        TimeLeft = startTime;
        Elapsed = 0f;
        EndReason = "";
    }

    public static void Tick(float dt)
    {
        TimeLeft = Mathf.Max(0f, TimeLeft - dt);
        Elapsed += dt;
    }

    public static void AddTime(float seconds) => TimeLeft = Mathf.Max(0f, TimeLeft + seconds);

    public static void CompleteProtein(float bonus)
    {
        Proteins++;
        AddTime(bonus);
    }

    public static void LoseLife() => Lives = Mathf.Max(0, Lives - 1);

    public static string FormatTime(float t)
    {
        int s = Mathf.CeilToInt(t);
        return $"{s / 60}:{s % 60:00}";
    }

    public static void LoadMenu() => SceneManager.LoadScene("MainMenu");
    public static void LoadGame() => SceneManager.LoadScene("Game");
    public static void LoadEndGame() => SceneManager.LoadScene("EndGame");
}
