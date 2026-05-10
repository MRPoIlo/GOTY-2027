using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// GOTY — Nivel 6 (Sótano)
/// Puerta metálica de salida. Se activa cuando el jugador
/// tiene los 3 fragmentos. Al usarla inicia una cuenta regresiva
/// de 5 segundos — si el padre llega antes, captura al jugador.
/// </summary>
public class PuertaMetalica : MonoBehaviour, IInteractuable
{
    [Header("Estado")]
    private bool habilitada   = false;
    private bool abriendo     = false;

    [Header("Cuenta regresiva")]
    [SerializeField] private float tiempoCuentaRegresiva = 5f;
    [SerializeField] private TextMeshProUGUI textoCuenta; // UI opcional

    [Header("Luz de la puerta")]
    [SerializeField] private Light luzPuerta;

    // ─── IInteractuable ───────────────────────────────────────────────────────

    public void Interactuar()
    {
        if (!habilitada || abriendo) return;
        abriendo = true;
        StartCoroutine(AbrirPuerta());
    }

    public string ObtenerTextoAccion()
    {
        if (!habilitada) return "Bloqueada";
        if (abriendo)   return "Abriendo...";
        return "Insertar llave";
    }

    public bool EstaActivo() => habilitada && !abriendo;

    // ─── API pública ──────────────────────────────────────────────────────────

    public void Habilitar()
    {
        habilitada = true;

        // Cambiar color de la luz al habilitarse
        if (luzPuerta != null)
        {
            luzPuerta.color     = new Color(0.5f, 1f, 0.5f); // verde
            luzPuerta.intensity = 1f;
        }

        NarracionManager.Instance?.Narrar(new string[]
        {
            "La llave encaja.",
            "Tengo que salir antes de que llegue."
        });

        Debug.Log("[PuertaMetalica] Habilitada");
    }

    // ─── Secuencia de apertura ────────────────────────────────────────────────

    private IEnumerator AbrirPuerta()
    {
        Debug.Log("[PuertaMetalica] Iniciando cuenta regresiva");

        // Notificar al manager — el padre empieza a correr hacia la puerta
        SotanoManager.Instance?.OnPuertaActivada();

        float tiempoRestante = tiempoCuentaRegresiva;

        while (tiempoRestante > 0f)
        {
            tiempoRestante -= Time.deltaTime;

            if (textoCuenta != null)
                textoCuenta.text = Mathf.CeilToInt(tiempoRestante).ToString();

            yield return null;
        }

        if (textoCuenta != null)
            textoCuenta.text = "";

        // La puerta abrió — notificar victoria
        SotanoManager.Instance?.OnJugadorEscapo();
    }
}