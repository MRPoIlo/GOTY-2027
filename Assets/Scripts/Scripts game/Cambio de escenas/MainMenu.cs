using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public GameObject mainMenu;
    public GameObject optionsMenu;
    public GameObject optionsCreditos;

    public void OpenOptionsPanel()
    {
        mainMenu.SetActive(false);
        optionsMenu.SetActive(true);
        optionsCreditos.SetActive(false);

    }

    public void OpenMainMenuPanel()
    {
        mainMenu.SetActive(true);
        optionsMenu.SetActive(false);
        optionsCreditos.SetActive(false);

    }

    public void AbrirCreditos()
    {
        mainMenu.SetActive(false);
        optionsCreditos.SetActive(true);
        optionsMenu.SetActive(false);
    }


    public void QuitGame()
    {
        Application.Quit();
    }

    public void PlayGame()
    {
        SceneManager.LoadScene("CinematicaInicial");
    }
}    
