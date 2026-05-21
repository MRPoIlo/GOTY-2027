using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using System.Text;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;

    private string apiURL =
        "http://goty.somee.com/api/Guardado";

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;

            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ─────────────────────────────────────
    // GUARDAR PARTIDA
    // ─────────────────────────────────────

    public void GuardarPartida()
    {
        string escenaActual =
            SceneManager.GetActiveScene().name;

        // GUARDADO LOCAL
        PlayerPrefs.SetString(
            "EscenaGuardada",
            escenaActual);

        PlayerPrefs.Save();

        Debug.Log(
            "💾 Partida guardada: " +
            escenaActual);

        // API
        StartCoroutine(
            EnviarGuardadoAPI(escenaActual));
    }

    // ─────────────────────────────────────
    // CARGAR PARTIDA
    // ─────────────────────────────────────

    public void CargarPartida()
    {
        if (PlayerPrefs.HasKey("EscenaGuardada"))
        {
            string escena =
                PlayerPrefs.GetString(
                    "EscenaGuardada");

            Debug.Log(
                "📂 Cargando partida: " +
                escena);

            SceneManager.LoadScene(escena);
        }
        else
        {
            Debug.Log(
                "❌ No existe partida guardada");
        }
    }

    // ─────────────────────────────────────
    // BORRAR PARTIDA
    // ─────────────────────────────────────

    public void BorrarPartida()
    {
        PlayerPrefs.DeleteKey(
            "EscenaGuardada");

        Debug.Log(
            "🗑 Partida eliminada");
    }

    // ─────────────────────────────────────
    // API SOMEE
    // ─────────────────────────────────────

    private IEnumerator EnviarGuardadoAPI(
        string escena)
    {
        string json =
            "{\"escena\":\"" + escena + "\"}";

        byte[] bodyRaw =
            Encoding.UTF8.GetBytes(json);

        UnityWebRequest request =
            new UnityWebRequest(apiURL, "POST");

        request.uploadHandler =
            new UploadHandlerRaw(bodyRaw);

        request.downloadHandler =
            new DownloadHandlerBuffer();

        request.SetRequestHeader(
            "Content-Type",
            "application/json");

        Debug.Log(
            "🌐 Enviando guardado a API...");

        yield return request.SendWebRequest();

        if (request.result ==
            UnityWebRequest.Result.Success)
        {
            Debug.Log(
                "✅ Guardado enviado a API");

            Debug.Log(
                request.downloadHandler.text);
        }
        else
        {
            Debug.LogError(
                "❌ Error API");

            Debug.LogError(
                request.error);
        }
    }
}