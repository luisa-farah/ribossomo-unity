using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Usado no Menu Inicial (com painel "Como jogar" e recorde) e na tela de Fim de Jogo.
public class MenuController : MonoBehaviour
{
    public Text reasonText;
    public Text resultText;
    public Text recordText;

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
        {
            string last = string.IsNullOrEmpty(GameManager.LastProtein)
                ? "Nenhuma proteína completa"
                : "Última proteína: " + GameManager.LastProtein;
            resultText.text = $"Proteínas sintetizadas: {GameManager.Proteins}\n" +
                              $"Tempo de sobrevivência: {GameManager.FormatTime(GameManager.Elapsed)}\n" +
                              last;
        }
        if (recordText != null)
        {
            if (GameManager.NewRecord && resultText != null)
                recordText.text = "<color=#FFD43B><b>NOVO RECORDE!</b></color>";
            else if (GameManager.BestProteins > 0)
                recordText.text = $"Recorde: {GameManager.BestProteins} proteínas ({GameManager.FormatTime(GameManager.BestTime)})";
            else
                recordText.text = "";
        }
    }

    public void Play() => GameManager.LoadGame();
    public void Menu() => GameManager.LoadMenu();

    public void ShowHowTo()
    {
        howToPanel.SetActive(true);
        Select(howToBackButton);
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
