using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// Puerta metálica del sótano.
///
/// NUEVA LÓGICA:
/// 1. El jugador debe interactuar por primera vez con la puerta.
/// 2. Eso activa el nivel y libera los fragmentos.
/// 3. El padre comienza a patrullar.
/// 4. Cuando el jugador consigue los 3 fragmentos, la puerta se habilita.
/// 5. Al interactuar nuevamente, inicia la cuenta regresiva para escapar.
/// </summary>
public class PuertaMetalica : MonoBehaviour, IInteractuable
{
    [Header("Estado")]
    private bool activada = false;   // Ya se examinó la puerta por primera vez
    private bool habilitada = false; // Ya tiene los 3 fragmentos
    private bool abriendo = false;

    [Header("Configuración")]
    [SerializeField] private int fragmentosNecesarios = 3;

    [Header("Cuenta regresiva")]
    [SerializeField] private float tiempoCuentaRegresiva = 5f;
    [SerializeField] private TextMeshProUGUI textoCuenta;

    [Header("Luz de la puerta")]
    [SerializeField] private Light luzPuerta;

    // ─────────────────────────────────────────────
    // Inicialización
    // ─────────────────────────────────────────────
    private void Start()
    {
        // Luz roja inicial
        if (luzPuerta != null)
        {
            luzPuerta.color = Color.red;
            luzPuerta.intensity = 1f;
        }

        if (textoCuenta != null)
            textoCuenta.text = "";
    }

    // ─────────────────────────────────────────────
    // Verificación automática de fragmentos
    // Solo después de activar la puerta.
    // ─────────────────────────────────────────────
    private void Update()
    {
        if (!activada)
            return;

        if (!habilitada &&
            SotanoManager.Instance != null &&
            SotanoManager.Instance.FragmentosRecogidos >= fragmentosNecesarios)
        {
            Habilitar();
        }
    }

    // ─────────────────────────────────────────────
    // IInteractuable
    // ─────────────────────────────────────────────
    public void Interactuar()
    {
        if (abriendo)
            return;

        // PRIMERA INTERACCIÓN:
        // activa el nivel y libera fragmentos
        if (!activada)
        {
            activada = true;

            NarracionManager.Instance?.Narrar(new string[]
            {
                "La puerta está cerrada.",
                "Necesito encontrar la llave."
            });

            // Avisar al manager que el nivel comienza
            SotanoManager.Instance?.OnPuertaExaminada();

            Debug.Log("[PuertaMetalica] Puerta examinada por primera vez.");

            return;
        }

        // Ya fue examinada, pero aún no tiene los fragmentos
        if (!habilitada)
        {
            Debug.Log("[PuertaMetalica] Aún faltan fragmentos.");
            return;
        }

        // Ya está habilitada: escapar
        abriendo = true;
        StartCoroutine(AbrirPuerta());
    }

    public string ObtenerTextoAccion()
    {
        if (abriendo)
            return "Abriendo...";

        if (!activada)
            return "Examinar puerta";

        int actuales = 0;

        if (SotanoManager.Instance != null)
            actuales = SotanoManager.Instance.FragmentosRecogidos;

        if (habilitada)
            return "Presiona E para escapar";

        return $"Fragmentos: {actuales}/{fragmentosNecesarios}";
    }

    public bool EstaActivo()
    {
        return !abriendo;
    }

    // ─────────────────────────────────────────────
    // Habilitar puerta
    // ─────────────────────────────────────────────
    public void Habilitar()
    {
        if (habilitada)
            return;

        habilitada = true;

        Debug.Log("[PuertaMetalica] Habilitada.");

        // Luz verde
        if (luzPuerta != null)
        {
            luzPuerta.color = Color.green;
            luzPuerta.intensity = 1.5f;
        }

        NarracionManager.Instance?.Narrar(new string[]
        {
            "Los fragmentos encajan.",
            "La puerta está desbloqueada."
        });
    }

    // ─────────────────────────────────────────────
    // Secuencia de apertura
    // ─────────────────────────────────────────────
    private IEnumerator AbrirPuerta()
    {
        Debug.Log("[PuertaMetalica] Iniciando cuenta regresiva.");

        // Aquí sí el padre corre hacia la puerta
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

        SotanoManager.Instance?.OnJugadorEscapo();
    }
}