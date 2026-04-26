using System.Collections;
using UnityEngine;

/// <summary>
/// GOTY — Nivel 4 (Vestíbulo)
/// Detecta cuando el jugador se acerca y alerta al VestibuloManager.
/// Usa distancia en vez de colisión para compatibilidad con CharacterController.
/// </summary>
public class ObjetoSonoro : MonoBehaviour
{
    public enum NivelRuido { Suave = 1, Medio = 2, Fuerte = 3 }

    [Header("Configuración")]
    public NivelRuido nivelRuido = NivelRuido.Suave;
    [SerializeField] private float distanciaDeteccion = 0.8f;
    [SerializeField] private AudioClip sonidoImpacto;

    private AudioSource audioSource;
    private Transform jugador;
    private bool activado = false;

    void Start()
    {
        jugador = FindObjectOfType<PlayerController>()?.transform;

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.spatialBlend = 1f;
        audioSource.playOnAwake  = false;
    }

    void Update()
    {
        if (activado || jugador == null) return;

        float dist = Vector3.Distance(transform.position, jugador.position);
        if (dist <= distanciaDeteccion)
        {
            activado = true;
            Debug.Log($"[ObjetoSonoro] {gameObject.name} activado — nivel {(int)nivelRuido}");

            if (sonidoImpacto != null)
                audioSource.PlayOneShot(sonidoImpacto);

            VestibuloManager.Instance?.RecibirRuido((int)nivelRuido);
        }
    }

    void OnEnable() => activado = false;

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, distanciaDeteccion);
    }
}