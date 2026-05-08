using UnityEngine;
using UnityEngine.SceneManagement;

public class PausaManager : MonoBehaviour
{
    [Header("Referencias UI")]
    [SerializeField] private GameObject panelMenu;
    [SerializeField] private GameObject panelOpciones;
    [SerializeField] private PlayerController player;
    [SerializeField] private Canvas canvasNarracion;

    [Header("Estado del juego")]
    public bool juegoPausado = false;
    public bool tieneDestornillador = false;
    public bool EnOpciones { get; private set; } = false;

    [Header("Nombre de la escena del menú principal")]
    [SerializeField] private string escenaMenuPrincipal = "MainMenu";

    private void Update()
    {
        // ✅ Bloquear pausa si hay Game Over activo — compatible con cualquier nivel
        if (GameOverManager.Instance != null && GameOverManager.Instance.gameOverActivado)
            return;

        // ✅ Compatibilidad con NivelManagerBaño si existe en la escena
        if (NivelManagerBaño.Instance != null && NivelManagerBaño.Instance.enGameOver)
            return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (juegoPausado) Continuar();
            else PausarJuego();
        }
    }

    public void PausarJuego()
    {
        if (panelMenu != null) panelMenu.SetActive(true);
        if (canvasNarracion != null) canvasNarracion.enabled = false;

        Time.timeScale = 0f;
        juegoPausado = true;
        EnOpciones = false;

        // ✅ Usar SetBloqueado en lugar de .enabled = false
        if (player != null) player.SetBloqueado(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Continuar()
    {
        if (panelMenu != null) panelMenu.SetActive(false);
        if (panelOpciones != null) panelOpciones.SetActive(false);
        if (canvasNarracion != null) canvasNarracion.enabled = true;

        Time.timeScale = 1f;
        juegoPausado = false;
        EnOpciones = false;

        // ✅ Usar SetBloqueado en lugar de .enabled = true
        if (player != null) player.SetBloqueado(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void AbrirOpciones()
    {
        if (panelOpciones != null) panelOpciones.SetActive(true);
        if (panelMenu != null) panelMenu.SetActive(false);
        EnOpciones = true;
    }

    public void CerrarOpciones()
    {
        if (panelOpciones != null) panelOpciones.SetActive(false);
        if (panelMenu != null) panelMenu.SetActive(true);
        EnOpciones = false;
    }

    public void SalirJuego()
    {
        // ✅ Restaurar estado completo antes de salir
        Time.timeScale = 1f;
        juegoPausado = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SceneManager.LoadScene(escenaMenuPrincipal);
    }
}