using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;

[System.Serializable]
public class LogroData
{
    public string nombreLogro;
    public int idTema;
}

public class ConexionAPI : MonoBehaviour
{
    private string url = "http://goty.somee.com/api/Logros";

    void Start()
    {
        StartCoroutine(EnviarLogro());
    }

    IEnumerator EnviarLogro()
    {
        Debug.Log(">>> Intentando conexión con la API...");

        // CREAR OBJETO
        LogroData data = new LogroData();

        data.nombreLogro = "Logro GOTY-2027";
        data.idTema = 1;

        // CONVERTIR A JSON
        string json = JsonUtility.ToJson(data);

        Debug.Log("JSON ENVIADO:");
        Debug.Log(json);

        using (UnityWebRequest www =
            new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw =
                Encoding.UTF8.GetBytes(json);

            www.uploadHandler =
                new UploadHandlerRaw(bodyRaw);

            www.downloadHandler =
                new DownloadHandlerBuffer();

            www.SetRequestHeader(
                "Content-Type",
                "application/json"
            );

            www.timeout = 60;

            yield return www.SendWebRequest();

            // RESPUESTA EXITOSA
            if (www.result ==
                UnityWebRequest.Result.Success)
            {
                Debug.Log("¡ÉXITO!");

                Debug.Log(www.downloadHandler.text);
            }
            else
            {
                Debug.LogError(
                    "Error HTTP: " +
                    www.responseCode
                );

                Debug.LogError(
                    "Error Unity: " +
                    www.error
                );

                Debug.LogError(
                    "Respuesta servidor:"
                );

                Debug.LogError(
                    www.downloadHandler.text
                );
            }
        }
    }
}