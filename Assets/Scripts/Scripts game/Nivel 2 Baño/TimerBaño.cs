using UnityEngine;
using TMPro;

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
        if (tiempoRestante <= 0)
        {
            tiempoRestante = 0;
            activo = false;

            // 👉 Llamada al Game Over educativo
            NivelManagerBaño.Instance?.GameOverPorTiempo();
        }

        int minutos = Mathf.FloorToInt(tiempoRestante / 60);
        int segundos = Mathf.FloorToInt(tiempoRestante % 60);
        textoTimer.text = $"{minutos:00}:{segundos:00}";
    }
}
