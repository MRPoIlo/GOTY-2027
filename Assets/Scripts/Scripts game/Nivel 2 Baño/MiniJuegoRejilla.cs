using System.Collections.Generic;
using UnityEngine;

public class MiniJuegoRejilla : MonoBehaviour
{
    [Header("Tornillos (asignar los 4 en orden)")]
    [SerializeField]
    private List<TornilloSlot> tornillos =
        new List<TornilloSlot>();

    [Header("Imagen del destornillador (draggable)")]
    [SerializeField] private RectTransform imagenDestornillador;

    private int tornillosRestantes;

    private void OnEnable()
    {
        // ✅ Cursor libre al abrir minijuego
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        tornillosRestantes = tornillos.Count;

        foreach (var t in tornillos)
        {
            t.Reiniciar();
            t.OnTornilloQuitado += OnTornilloEliminado;
        }
    }

    private void OnDisable()
    {
        foreach (var t in tornillos)
        {
            t.OnTornilloQuitado -= OnTornilloEliminado;
        }
    }

    private void OnTornilloEliminado()
    {
        tornillosRestantes--;

        if (tornillosRestantes <= 0)
        {
            NivelManagerBaño.Instance
                ?.CompletarMiniJuego();
        }
    }
}