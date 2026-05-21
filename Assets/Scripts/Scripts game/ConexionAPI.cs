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

    void Start()
    {
        StartCoroutine(EnviarLogro());
    }

    IEnumerator EnviarLogro()
    {
        LogroData data = new LogroData();
        data.NombreJugador = "Luis German";
        data.Descripcion = "Logro GOTY-2027";
        data.idTema = 1;

        string json = JsonUtility.ToJson(data);
        Debug.Log("Enviando JSON: " + json);

        using (UnityWebRequest www = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();

            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("<color=green>¡ÉXITO!</color> Respuesta: " + www.downloadHandler.text);
            }
            else
            {
                Debug.LogError("ERROR: " + www.error);
                Debug.LogError("Detalle del servidor: " + www.downloadHandler.text);
            }
        }
    }
}