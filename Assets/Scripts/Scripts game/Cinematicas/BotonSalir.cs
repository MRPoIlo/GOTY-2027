using UnityEngine;
using UnityEngine.SceneManagement;

public class BotonSalir : MonoBehaviour
{
    public void SalirAlMenu()
    {
        SceneManager.LoadScene("MainMenu");
        // Cambia "MenuPrincipal" por el nombre exacto de tu escena de menú
    }

    public void CerrarJuego()
    {
        Application.Quit();
        Debug.Log("Juego cerrado.");
    }
}
