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
    public bool juegoPausado = false;       // ← público
    public bool tieneDestornillador = false; // ← público
    public bool EnOpciones { get; private set; } = false; // ← propiedad pública

    [Header("Nombre de la escena del menú principal")]
    [SerializeField] private string escenaMenuPrincipal = "MainMenu";

    private void Update()
    {
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

        if (player != null) player.enabled = false;

        // Liberar cursor para poder usar botones
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

        if (player != null) player.enabled = true;

        // Volver a bloquear cursor al jugar
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void AbrirOpciones()
    {
        if (panelOpciones != null) panelOpciones.SetActive(true);
        EnOpciones = true;
    }

    public void CerrarOpciones()
    {
        if (panelOpciones != null) panelOpciones.SetActive(false);
        EnOpciones = false;
    }

    public void SalirJuego()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(escenaMenuPrincipal);
    }
}
