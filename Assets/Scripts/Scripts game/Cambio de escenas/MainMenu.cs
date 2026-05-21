using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Canvas completo del menú")]
    public GameObject canvasMenu;

    [Header("Paneles")]
    public GameObject mainMenu;
    public GameObject optionsMenu;
    public GameObject optionsCreditos;
    public GameObject panelJugar;

    // ─────────────────────────────
    // OPCIONES
    // ─────────────────────────────

    public void OpenOptionsPanel()
    {
        mainMenu.SetActive(false);
        optionsMenu.SetActive(true);
        optionsCreditos.SetActive(false);
        panelJugar.SetActive(false);
    }

    public void OpenMainMenuPanel()
    {
        mainMenu.SetActive(true);
        optionsMenu.SetActive(false);
        optionsCreditos.SetActive(false);
        panelJugar.SetActive(false);
    }

    public void AbrirCreditos()
    {
        mainMenu.SetActive(false);
        optionsCreditos.SetActive(true);
        optionsMenu.SetActive(false);
        panelJugar.SetActive(false);
    }

    // ─────────────────────────────
    // BOTÓN JUGAR
    // ─────────────────────────────

    public void PlayGame()
    {
        // OCULTA TODO EL MENÚ COMPLETO
        canvasMenu.SetActive(false);

        // MUESTRA PANEL DE JUGAR
        panelJugar.SetActive(true);
    }

    // ─────────────────────────────
    // NUEVA PARTIDA
    // ─────────────────────────────

    public void NuevaPartida()
    {
        PlayerPrefs.DeleteKey("EscenaGuardada");

        Debug.Log("🆕 Nueva partida iniciada");

        SceneManager.LoadScene("CinematicaInicial");
    }

    // ─────────────────────────────
    // CARGAR PARTIDA
    // ─────────────────────────────

    public void CargarPartida()
    {
        if (PlayerPrefs.HasKey("EscenaGuardada"))
        {
            string escena = PlayerPrefs.GetString("EscenaGuardada");

            Debug.Log("📂 Cargando partida: " + escena);

            SceneManager.LoadScene(escena);
        }
        else
        {
            Debug.Log("❌ No hay partida guardada");
        }
    }

    // ─────────────────────────────
    // SALIR
    // ─────────────────────────────

    public void QuitGame()
    {
        Application.Quit();
    }
}