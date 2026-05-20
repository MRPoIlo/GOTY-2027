using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;

public class ConexionAPI : MonoBehaviour
{
    private string url = "http://goty.somee.com/api/Logros";

    void Start()
    {
        StartCoroutine(Subir());
    }

    IEnumerator Subir()
    {
        // JSON con la estructura exacta que pide tu validador 400.
        // Incluimos el objeto "tema" con sus datos para que deje de decir que es requerido.
        string json = "{\"nombreLogro\":\"Logro Final\",\"idTema\":1,\"tema\":{\"id\":1,\"nombreTema\":\"General\"}}";

        using (UnityWebRequest www = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();

            // Forzamos los headers para que el servidor no tenga dudas
            www.SetRequestHeader("Content-Type", "application/json");
            www.SetRequestHeader("Accept", "application/json");

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("=== FALLÓ EL ENVÍO ===");
                Debug.LogError("Código: " + www.responseCode);
                Debug.LogError("Causa: " + www.downloadHandler.text);
            }
            else
            {
                Debug.Log("¡POR FIN! Conexión exitosa.");
                Debug.Log("Respuesta: " + www.downloadHandler.text);
            }
        }
    }
}