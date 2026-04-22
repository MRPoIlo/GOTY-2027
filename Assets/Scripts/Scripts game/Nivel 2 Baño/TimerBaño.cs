using UnityEngine;
using TMPro;   // Importante: para TextMeshPro

public class TimerBaño : MonoBehaviour
{
    public float tiempoRestante = 120f; // 2 minutos
    public TMP_Text textoTimer;
    private bool activo = false;

    public void IniciarTimer()
    {
        activo = true;
    }

    void Update()
    {
        if (!activo) return;

        tiempoRestante -= Time.deltaTime;
        if (tiempoRestante < 0)
        {
            tiempoRestante = 0;
            activo = false;
            // Aquí puedes disparar un evento de "Game Over" o narración
        }

        int minutos = Mathf.FloorToInt(tiempoRestante / 60);
        int segundos = Mathf.FloorToInt(tiempoRestante % 60);
        textoTimer.text = $"{minutos:00}:{segundos:00}";
    }
}
