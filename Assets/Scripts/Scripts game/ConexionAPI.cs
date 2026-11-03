using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;

[System.Serializable]
public class LogroData
{
    // Estos nombres deben ser idénticos a lo que el error en la consola te pidió
    public string NombreJugador;
    public string Descripcion;
    public int idTema;
}

public class ConexionAPI : MonoBehaviour
{
    // Asegúrate de que esta URL sea la que te respondió en el navegador
    private string url = "http://goty.somee.com/api/Logroes";

    // Segundos máximos de espera antes de abandonar la petición
    private const int TimeoutSegundos = 10;

    // Los logros ahora son locales (LogrosManager). El servidor queda desactivado.
    [Header("Servidor de logros (desactivado)")]
    [SerializeField] private bool usarServidor = false;

    // Llama a este método cuando el jugador consiga un logro real.
    public void EnviarLogro(string nombreJugador, string descripcion, int idTema)
    {
        LogrosManager.Instance?.Desbloquear("tema_" + idTema, descripcion);

        if (!usarServidor) return;

        LogroData data = new LogroData
        {
            NombreJugador = nombreJugador,
            Descripcion = descripcion,
            idTema = idTema
        };
        StartCoroutine(EnviarLogroCoroutine(data));
    }

    IEnumerator EnviarLogroCoroutine(LogroData data)
    {
        string json = JsonUtility.ToJson(data);
        Debug.Log("Enviando JSON: " + json);

        using (UnityWebRequest www = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.timeout = TimeoutSegundos;

            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("<color=green>¡ÉXITO!</color> Respuesta: " + www.downloadHandler.text);
            }
            else
            {
                // El servidor no respondió o devolvió un fallo: avisamos sin romper el juego.
                string detalle = string.IsNullOrEmpty(www.downloadHandler.text) ? "(sin respuesta)" : www.downloadHandler.text;
                Debug.LogWarning($"No se pudo enviar el logro ({www.error}). Detalle del servidor: {detalle}");
            }
        }
    }
}
