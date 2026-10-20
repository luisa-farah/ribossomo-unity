using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Usado no Menu Inicial (com painel "Como jogar") e na tela de Fim de Jogo.
public class MenuController : MonoBehaviour
{
    public Text reasonText;
    public Text resultText;

    [Header("Como jogar")]
    public GameObject howToPanel;
    public GameObject mainFirstButton;
    public GameObject howToBackButton;

    void Start()
    {
        Time.timeScale = 1f;
        if (howToPanel != null) howToPanel.SetActive(false);
        if (reasonText != null) reasonText.text = GameManager.EndReason;
        if (resultText != null)
            resultText.text = $"Proteínas sintetizadas: {GameManager.Proteins}\n" +
                              $"Tempo de sobrevivência: {GameManager.FormatTime(GameManager.Elapsed)}";
    }

    public void Play() => GameManager.LoadGame();
    public void Menu() => GameManager.LoadMenu();

    public void ShowHowTo()
    {
        howToPanel.SetActive(true);
        Select(howToBackButton); // permite fechar com controle
    }

    public void HideHowTo()
    {
        howToPanel.SetActive(false);
        Select(mainFirstButton);
    }

    static void Select(GameObject go)
    {
        if (EventSystem.current != null && go != null) EventSystem.current.SetSelectedGameObject(go);
    }
}
