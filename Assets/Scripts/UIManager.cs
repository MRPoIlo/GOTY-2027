using UnityEngine;
using TMPro; // ✅ necesario para usar TextMeshProUGUI

public class UIManager : MonoBehaviour
{
    public static UIManager inst; // Instancia global accesible

    // --- Referencias a textos de UI ---
    public TextMeshProUGUI TimeCounterGameplay; // Texto que muestra el tiempo durante el juego
    public TextMeshProUGUI TimeCounterWin;      // Texto que muestra el tiempo en la pantalla de victoria

    // --- Variables de tiempo ---
    private float TimeSeconds; // segundos acumulados
    private int TimeMinutes;   // minutos acumulados

    private bool Win; // controla si ya se ganó

    // --- Pantalla de victoria ---
    public GameObject WinScreen;

    void Awake()
    {
        inst = this; // ✅ guardamos esta instancia en la variable estática
    }

    public void ShowWinScreen()
    {
        // ✅ activa pantalla de victoria
        WinScreen.SetActive(true);
        Win = true;

        // ✅ muestra el tiempo final en la pantalla de victoria
        TimeCounterWin.text = "Tiempo: " + TimeMinutes + ":" + Mathf.Ceil(TimeSeconds);

        // ✅ oculta el contador de gameplay
        TimeCounterGameplay.gameObject.SetActive(false);
    }

    void Update()
    {
        if (!Win)
        {
            // ✅ acumula segundos
            TimeSeconds += Time.deltaTime;

            // ✅ cuando los segundos pasan de 60, aumenta minutos
            if (TimeSeconds >= 60f)
            {
                TimeMinutes++;
                TimeSeconds = 0f;
            }

            // ✅ actualiza el texto en gameplay
            TimeCounterGameplay.text = "Tiempo: " + TimeMinutes + ":" + Mathf.FloorToInt(TimeSeconds);
        }
    }
}