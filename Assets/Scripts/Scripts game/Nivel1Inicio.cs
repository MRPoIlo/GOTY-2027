using UnityEngine;

public class Nivel1Inicio : MonoBehaviour
{
    void Start()
    {
        Debug.Log("Nivel1Inicio ejecutado");

        if (NarracionManager.Instance != null)
        {
            Debug.Log("NarracionManager encontrado, mostrando mensaje inicial...");
            NarracionManager.Instance.Narrar(new string[] {
                "La habitación de mis padres.",
                "Presiona E para continuar..."
            });
        }
        else
        {
            Debug.LogError("No se encontró NarracionManager en la escena.");
        }
    }
}
