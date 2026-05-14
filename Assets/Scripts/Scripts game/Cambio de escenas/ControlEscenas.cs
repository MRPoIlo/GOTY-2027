using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

[System.Serializable]
public class Nivel
{
    public string nombreNivel;          // Nombre del nivel (ej: "Prologo")
    public List<Sprite> imagenes;       // Lista de imágenes que tú eliges
    public string siguienteNivel;       // Nombre del siguiente nivel
}

public class ControlEscenas : MonoBehaviour
{
    public Image pantalla;              // Imagen del Canvas donde se mostrarán las escenas
    public List<Nivel> niveles;         // Lista de niveles configurables en el Inspector

    [Header("Control de flujo")]
    public string nivelActual;          // Nombre del nivel actual (lo escribes en el Inspector)

    private Dictionary<string, Nivel> mapaNiveles;
    private static bool yaEjecutado = false;

    void Start()
    {
        Debug.Log("🚀 ControlEscenas iniciado en escena: " + SceneManager.GetActiveScene().name);

        if (yaEjecutado)
        {
            Debug.Log("⏭ Secuencia ya ejecutada, no se repite.");
            return;
        }
        yaEjecutado = true;

        mapaNiveles = new Dictionary<string, Nivel>();
        foreach (var nivel in niveles)
        {
            mapaNiveles[nivel.nombreNivel] = nivel;
        }

        StartCoroutine(MostrarSecuencia());
    }

    IEnumerator MostrarSecuencia()
    {
        if (!mapaNiveles.ContainsKey(nivelActual))
        {
            Debug.LogWarning("⚠ Nivel no encontrado: " + nivelActual);
            yield break;
        }

        Nivel nivel = mapaNiveles[nivelActual];

        // 🔹 Activar Canvas al inicio
        pantalla.canvas.gameObject.SetActive(true);
        Debug.Log("✅ Canvas activado para nivel: " + nivel.nombreNivel);

        // Mostrar todas las imágenes configuradas
        for (int i = 0; i < nivel.imagenes.Count; i++)
        {
            pantalla.sprite = nivel.imagenes[i];
            Debug.Log("🖼 Mostrando imagen " + i + " del nivel " + nivel.nombreNivel);

            yield return new WaitForSeconds(3f);
        }

        // 🔹 Desactivar Canvas COMPLETO al terminar todas las imágenes
        pantalla.sprite = null; // limpiar sprite
        pantalla.canvas.gameObject.SetActive(false);
        Debug.Log("❌ Canvas COMPLETO desactivado después de mostrar todas las imágenes");

        // 🔹 Cargar siguiente nivel
        if (!string.IsNullOrEmpty(nivel.siguienteNivel))
        {
            Debug.Log("➡ Intentando cargar siguiente nivel: " + nivel.siguienteNivel);

            if (Application.CanStreamedLevelBeLoaded(nivel.siguienteNivel))
            {
                SceneManager.LoadScene(nivel.siguienteNivel);
            }
            else
            {
                Debug.LogError("⚠ La escena '" + nivel.siguienteNivel + "' no está incluida en Build Settings. Agrega la escena para que pueda cargarse.");
            }
        }
        else
        {
            Debug.Log("🏁 Fin de la historia. No hay siguiente nivel configurado.");
        }

        yield break;
    }
}
