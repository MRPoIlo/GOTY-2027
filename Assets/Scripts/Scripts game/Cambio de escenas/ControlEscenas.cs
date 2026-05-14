using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[System.Serializable]
public class Nivel
{
    public string nombreNivel;          // Nombre del nivel (ej: "Sotano")
    public List<Sprite> imagenes;       // Lista de imágenes que tú eliges
}

public class ControlEscenas : MonoBehaviour
{
    public Image pantalla;              // Imagen del Canvas donde se mostrarán las escenas
    public List<Nivel> niveles;         // Lista de niveles configurables en el Inspector

    [Header("Control de flujo")]
    public string nivelActual;          // Nombre del nivel actual (lo escribes en el Inspector)
    public string siguienteNivel;       // Nombre del siguiente nivel (lo escribes en el Inspector)

    private int imagenActual = 0;
    private Dictionary<string, Nivel> mapaNiveles;

    void Start()
    {
        // Crear el diccionario para acceder rápido por nombre
        mapaNiveles = new Dictionary<string, Nivel>();
        foreach (var nivel in niveles)
        {
            mapaNiveles[nivel.nombreNivel] = nivel;
        }

        // Mostrar la primera imagen del nivel actual
        if (mapaNiveles.ContainsKey(nivelActual) && mapaNiveles[nivelActual].imagenes.Count > 0)
        {
            MostrarImagen(mapaNiveles[nivelActual].imagenes[imagenActual]);
        }
        else
        {
            Debug.LogWarning("Nivel actual no encontrado o sin imágenes: " + nivelActual);
        }
    }

    public void SiguienteEscena()
    {
        if (!mapaNiveles.ContainsKey(nivelActual)) return;

        imagenActual++;

        // Si se acabaron las imágenes del nivel actual, saltar al siguiente nivel
        if (imagenActual >= mapaNiveles[nivelActual].imagenes.Count)
        {
            nivelActual = siguienteNivel;
            imagenActual = 0;
        }

        if (mapaNiveles.ContainsKey(nivelActual) && mapaNiveles[nivelActual].imagenes.Count > 0)
        {
            MostrarImagen(mapaNiveles[nivelActual].imagenes[imagenActual]);
        }
        else
        {
            Debug.LogWarning("Nivel no encontrado o sin imágenes: " + nivelActual);
        }
    }

    void MostrarImagen(Sprite nuevaImagen)
    {
        pantalla.sprite = nuevaImagen;
    }
}