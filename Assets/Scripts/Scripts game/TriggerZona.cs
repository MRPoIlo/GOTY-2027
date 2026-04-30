using System.Collections;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// GOTY - Zona de trigger para activar narración u otros eventos
/// al entrar a un área específica.
/// </summary>
[RequireComponent(typeof(Collider))]
public class TriggerZona : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private bool usoUnico = true;

    [Header("Jugador a detectar")]
    [SerializeField] private GameObject jugador;

    [Header("Narración opcional")]
    [SerializeField, TextArea(2, 5)]
    private string[] narracionAlEntrar;

    [Header("Evento")]
    public UnityEvent OnZonaActivada;

    [Header("Objeto asociado")]
    [Tooltip("Aparece al entrar al trigger y desaparece después del tiempo indicado")]
    [SerializeField] private GameObject objetoMensaje;
    [Tooltip("Segundos antes de que el objeto desaparezca. 0 = no desaparece solo")]
    [SerializeField] private float tiempoDesaparecer = 1f;

    private bool activado = false;

    void Awake()
    {
        GetComponent<Collider>().isTrigger = true;

        if (objetoMensaje != null)
            objetoMensaje.SetActive(false);
    }

    void OnTriggerEnter(Collider otro)
    {
        if (usoUnico && activado) return;
        if (jugador != null && otro.gameObject != jugador) return;

        activado = true;

        // Narración
        if (narracionAlEntrar != null && narracionAlEntrar.Length > 0)
            NarracionManager.Instance?.Narrar(narracionAlEntrar);

        // Mostrar objeto y ocultarlo después del tiempo
        if (objetoMensaje != null)
        {
            objetoMensaje.SetActive(true);

            if (tiempoDesaparecer > 0f)
                StartCoroutine(OcultarDespuesDe(tiempoDesaparecer));
        }

        OnZonaActivada?.Invoke();
    }

    private IEnumerator OcultarDespuesDe(float segundos)
    {
        yield return new WaitForSeconds(segundos);

        if (objetoMensaje != null)
            objetoMensaje.SetActive(false);
    }

    public void OcultarMensaje()
    {
        NarracionManager.Instance?.Detener();
        if (objetoMensaje != null)
            objetoMensaje.SetActive(false);
    }

    public void Reactivar() => activado = false;

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.2f, 0.6f, 1f, 0.25f);
        var col = GetComponent<BoxCollider>();
        if (col != null)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(col.center, col.size);
        }
    }
}