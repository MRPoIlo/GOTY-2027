using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PausaManager : MonoBehaviour
{
    [Header("Referencias UI")]
    [SerializeField] private GameObject panelMenu;
    [SerializeField] private GameObject panelOpciones;
    [SerializeField] private PlayerController player;
    [SerializeField] private Canvas canvasNarracion;

    [Header("MiniJuego UI")]
    [SerializeField] private GameObject miniJuegoRejillaUI;

    [Header("Estado del juego")]
    public bool juegoPausado = false;
    public bool tieneDestornillador = false;

    public bool EnOpciones { get; private set; } = false;

    [Header("Nombre escena menú principal")]
    [SerializeField]
    private string escenaMenuPrincipal =
        "MainMenu";

    private void Update()
    {
        // ✅ Bloquear pausa en Game Over
        if (GameOverManager.Instance != null &&
            GameOverManager.Instance.gameOverActivado)
            return;

        if (NivelManagerBaño.Instance != null &&
            NivelManagerBaño.Instance.enGameOver)
            return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (juegoPausado)
                Continuar();
            else
                PausarJuego();
        }
    }

    // ─────────────────────────────────────
    // PAUSAR
    // ─────────────────────────────────────

    public void PausarJuego()
    {
        if (panelMenu != null)
            panelMenu.SetActive(true);

        if (canvasNarracion != null)
            canvasNarracion.enabled = false;

        Time.timeScale = 0f;

        juegoPausado = true;
        EnOpciones = false;

        if (player != null)
            player.SetBloqueado(true);

        Cursor.lockState =
            CursorLockMode.None;

        Cursor.visible = true;
    }

    // ─────────────────────────────────────
    // CONTINUAR
    // ─────────────────────────────────────

    public void Continuar()
    {
        if (panelMenu != null)
            panelMenu.SetActive(false);

        if (panelOpciones != null)
            panelOpciones.SetActive(false);

        if (canvasNarracion != null)
            canvasNarracion.enabled = true;

        Time.timeScale = 1f;

        juegoPausado = false;
        EnOpciones = false;

        bool miniJuegoActivo =
            miniJuegoRejillaUI != null &&
            miniJuegoRejillaUI.activeSelf;

        // ✅ SI EL MINIJUEGO SIGUE ABIERTO
        if (miniJuegoActivo)
        {
            // 🔥 IMPORTANTE:
            // mantener jugador bloqueado

            if (player != null)
                player.SetBloqueado(true);

            StartCoroutine(
                ForzarCursorMiniJuego()
            );
        }
        else
        {
            Cursor.lockState =
                CursorLockMode.Locked;

            Cursor.visible = false;

            if (player != null)
                player.SetBloqueado(false);
        }
    }

    // ─────────────────────────────────────
    // FIX CURSOR MINIJUEGO
    // ─────────────────────────────────────

    private IEnumerator ForzarCursorMiniJuego()
    {
        // 🔥 Forzar varios frames
        // porque otro script roba el mouse

        for (int i = 0; i < 5; i++)
        {
            Cursor.lockState =
                CursorLockMode.None;

            Cursor.visible = true;

            yield return null;
        }
    }

    // ─────────────────────────────────────
    // OPCIONES
    // ─────────────────────────────────────

    public void AbrirOpciones()
    {
        if (panelOpciones != null)
            panelOpciones.SetActive(true);

        if (panelMenu != null)
            panelMenu.SetActive(false);

        EnOpciones = true;
    }

    public void CerrarOpciones()
    {
        if (panelOpciones != null)
            panelOpciones.SetActive(false);

        if (panelMenu != null)
            panelMenu.SetActive(true);

        EnOpciones = false;
    }

    // ─────────────────────────────────────
    // SALIR
    // ─────────────────────────────────────

    public void SalirJuego()
    {
        Time.timeScale = 1f;

        juegoPausado = false;

        Cursor.lockState =
            CursorLockMode.None;

        Cursor.visible = true;

        SceneManager.LoadScene(
            escenaMenuPrincipal
        );
    }
}