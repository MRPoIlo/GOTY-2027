using UnityEngine;
using UnityEngine.SceneManagement;

public class Menudepartidas : MonoBehaviour
{
    [Header("Panel Jugar")]
    public GameObject panelJugar;

    // ABRIR PANEL JUGAR
    public void AbrirJugar()
    {
        panelJugar.SetActive(true);
    }

    // NUEVA PARTIDA
    public void NuevaPartida()
    {
        PlayerPrefs.DeleteKey("EscenaGuardada");

        Debug.Log("🆕 Nueva partida iniciada");

        SceneManager.LoadScene("Prologo");
    }

    // CARGAR PARTIDA
    public void CargarPartida()
    {
        if (PlayerPrefs.HasKey("EscenaGuardada"))
        {
            string escena =
                PlayerPrefs.GetString("EscenaGuardada");

            Debug.Log("📂 Cargando partida: " + escena);

            SceneManager.LoadScene(escena);
        }
        else
        {
            Debug.Log("❌ No hay partida guardada");
        }
    }
}