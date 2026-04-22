using System.Collections;
using UnityEngine;

public class SombraMama : MonoBehaviour
{
    [Header("Referencias de escena")]
    [SerializeField] private Transform puntoOrigen;
    [SerializeField] private Transform puertoObjetivo;
    [SerializeField] private GameObject sombraPadre;
    [SerializeField] private GameObject puertaBlockeada;

    [Header("Movimiento")]
    [SerializeField] private float velocidadCaminar = 0.8f;
    [SerializeField] private float tiempoEsperaFinal = 2.5f;

    [Header("Partículas de polvo")]
    [SerializeField] private ParticleSystem particulasPolvo;

    [Header("Renderer para fade")]
    [SerializeField] private Renderer[] renderers;

    private Animator anim;
    private bool secuenciaIniciada = false;

    void Awake()
    {
        anim = GetComponentInChildren<Animator>();
    }

    // ─── API pública ──────────────────────────────────────────────────────────

    public void IniciarSecuencia()
    {
        if (secuenciaIniciada) return;
        secuenciaIniciada = true;

        // Activar el GameObject
        gameObject.SetActive(true);

        // Arrancar la corrutina desde un objeto que siempre está activo
        // usando el AticoManager como puente
        AticoManager.Instance?.StartCoroutine(SecuenciaProteccion());
    }

    // ─── Secuencia completa ───────────────────────────────────────────────────

    private IEnumerator SecuenciaProteccion()
    {
        Debug.Log("[SombraMama] PASO 1 - Iniciando secuencia");

        // Posicionar
        transform.position = puntoOrigen.position;

        Vector3 dir = puertoObjetivo.position - transform.position;
        dir.y = 0f;
        if (dir != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(dir);

        SetAlpha(0f);
        particulasPolvo?.Play();
        if (anim != null) anim.SetBool("Caminando", false);

        // Fade in
        yield return StartCoroutine(FadeAlpha(0f, 1f, 1.5f));
        Debug.Log("[SombraMama] PASO 2 - Apareci, narrando");

        // Narración de aparición
        NarracionManager.Instance?.Detener();
        yield return new WaitForSeconds(0.5f);

        NarracionManager.Instance?.Narrar(new string[]
        {
            "Hay algo en el fondo del ático.",
            "Una figura."
        });

        yield return StartCoroutine(EsperarNarracion(8f));
        Debug.Log("[SombraMama] PASO 3 - Caminando");

        // Caminar
        if (anim != null) anim.SetBool("Caminando", true);

        while (Vector3.Distance(transform.position, puertoObjetivo.position) > 0.2f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                puertoObjetivo.position,
                velocidadCaminar * Time.deltaTime);

            Vector3 direccion = puertoObjetivo.position - transform.position;
            direccion.y = 0f;
            if (direccion != Vector3.zero)
                transform.rotation = Quaternion.LookRotation(direccion);

            yield return null;
        }

        if (anim != null) anim.SetBool("Caminando", false);
        Debug.Log("[SombraMama] PASO 4 - Llego a la puerta");

        // Narración al llegar
        NarracionManager.Instance?.Narrar(new string[]
        {
            "Se puso entre mí y la puerta.",
            "No dijo nada. Nunca decía nada."
        });
        yield return StartCoroutine(EsperarNarracion(10f));
        Debug.Log("[SombraMama] PASO 5 - Alejando al padre");

        // Alejar padre
        if (sombraPadre != null)
            yield return StartCoroutine(AlejarSombra());

        // Abrir puerta
        if (puertaBlockeada != null)
            puertaBlockeada.SetActive(false);

        Debug.Log("[SombraMama] PASO 6 - Pausa final");
        yield return new WaitForSeconds(tiempoEsperaFinal);

        NarracionManager.Instance?.Narrar(new string[]
        {
            "Nunca supe si lo hacía a propósito.",
            "Pero siempre estaba."
        });
        yield return StartCoroutine(EsperarNarracion(10f));

        Debug.Log("[SombraMama] PASO 7 - Desvaneciendo");
        yield return StartCoroutine(FadeAlpha(1f, 0f, 2.5f));

        particulasPolvo?.Stop();
        gameObject.SetActive(false);

        Debug.Log("[SombraMama] PASO 8 - Secuencia terminada");
        AticoManager.Instance?.OnSecuenciaMamaTerminada();
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    private IEnumerator EsperarNarracion(float timeout)
    {
        yield return new WaitForSeconds(0.2f);
        float t = 0f;
        while (t < timeout)
        {
            if (NarracionManager.Instance == null || !NarracionManager.Instance.EstaActivo())
                yield break;
            t += Time.deltaTime;
            yield return null;
        }
        Debug.LogWarning("[SombraMama] Timeout — continuando");
    }

    private IEnumerator AlejarSombra()
    {
        if (sombraPadre == null) yield break;
        Vector3 posInicial = sombraPadre.transform.position;
        Vector3 posFinal   = posInicial + Vector3.back * 5f;
        float t = 0f;
        float duracion = 1.5f;
        while (t < duracion)
        {
            t += Time.deltaTime;
            sombraPadre.transform.position =
                Vector3.Lerp(posInicial, posFinal, t / duracion);
            yield return null;
        }
        sombraPadre.SetActive(false);
    }

    private IEnumerator FadeAlpha(float desde, float hasta, float duracion)
    {
        float t = 0f;
        while (t < duracion)
        {
            t += Time.deltaTime;
            SetAlpha(Mathf.Lerp(desde, hasta, t / duracion));
            yield return null;
        }
        SetAlpha(hasta);
    }

    private void SetAlpha(float alpha)
    {
        foreach (var r in renderers)
        {
            if (r == null) continue;
            if (r.material.HasProperty("_BaseColor"))
            {
                Color c = r.material.GetColor("_BaseColor");
                c.a = alpha;
                r.material.SetColor("_BaseColor", c);
            }
            else if (r.material.HasProperty("_Color"))
            {
                Color c = r.material.color;
                c.a = alpha;
                r.material.color = c;
            }
        }
    }
}