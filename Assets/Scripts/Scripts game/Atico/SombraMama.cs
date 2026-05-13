using System.Collections;
using UnityEngine;

public class SombraMama : MonoBehaviour
{
    [Header("Referencias de escena")]
    [SerializeField] private Transform puntoOrigen;
    [SerializeField] private Transform puertoObjetivo;
    [SerializeField] private GameObject sombraPadre;
    [SerializeField] private PuertaAtico puerta; // 🔹 referencia al script PuertaNivel

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

        gameObject.SetActive(true);

        AticoManager.Instance?.StartCoroutine(SecuenciaProteccion());
    }

    // ─── Secuencia completa ───────────────────────────────────────────────────
    private IEnumerator SecuenciaProteccion()
    {
        Debug.Log("[SombraMama] PASO 1 - Iniciando secuencia");

        transform.position = puntoOrigen.position;

        Vector3 dir = puertoObjetivo.position - transform.position;
        dir.y = 0f;
        if (dir != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(dir);

        SetAlpha(0f);
        particulasPolvo?.Play();
        if (anim != null) anim.SetBool("Caminando", false);

        yield return StartCoroutine(FadeAlpha(0f, 1f, 1.5f));

        NarracionManager.Instance?.Detener();
        yield return new WaitForSeconds(0.5f);

        NarracionManager.Instance?.Narrar(new string[]
        {
            "Hay algo en el fondo del ático.",
            "Una figura."
        });

        yield return StartCoroutine(EsperarNarracion(8f));

        if (anim != null) anim.SetBool("Caminando", true);

        while (Vector3.Distance(transform.position, puertoObjetivo.position) > 0.2f)
        {
            // 🔹 Guardar posición anterior
            Vector3 posAnterior = transform.position;

            // 🔹 Mover hacia el objetivo
            transform.position = Vector3.MoveTowards(
                transform.position,
                puertoObjetivo.position,
                velocidadCaminar * Time.deltaTime);

            // 🔹 Calcular dirección y rotación
            Vector3 direccion = puertoObjetivo.position - transform.position;
            direccion.y = 0f;
            if (direccion != Vector3.zero)
                transform.rotation = Quaternion.LookRotation(direccion);

            // 🔹 Calcular velocidad real del frame
            float velocidadFrame = (transform.position - posAnterior).magnitude / Time.deltaTime;

            // 🔹 Actualizar parámetro "speed" en el Animator
            if (anim != null)
                anim.SetFloat("Speed", velocidadFrame);

            yield return null;
        }

        if (anim != null)
        {
            anim.SetBool("Caminando", false);
            anim.SetFloat("Speed", 0f);
        }

        NarracionManager.Instance?.Narrar(new string[]
        {
            "Se puso entre mí y la puerta.",
            "No dijo nada. Nunca decía nada."
        });
        yield return StartCoroutine(EsperarNarracion(10f));

        if (sombraPadre != null)
            yield return StartCoroutine(AlejarSombra());

        // 🔹 En vez de desactivar la puerta, la habilitamos
        if (puerta != null)
        {
            puerta.HabilitarPuerta();
            Debug.Log("[SombraMama] Puerta habilitada, ahora puede usarse para salir.");
        }

        yield return new WaitForSeconds(tiempoEsperaFinal);

        NarracionManager.Instance?.Narrar(new string[]
        {
            "Nunca supe si lo hacía a propósito.",
            "Pero siempre estaba."
        });
        yield return StartCoroutine(EsperarNarracion(10f));

        yield return StartCoroutine(FadeAlpha(1f, 0f, 2.5f));

        particulasPolvo?.Stop();
        gameObject.SetActive(false);

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
    }

    private IEnumerator AlejarSombra()
    {
        if (sombraPadre == null) yield break;

        Animator anim = sombraPadre.GetComponent<Animator>();
        Vector3 posInicial = sombraPadre.transform.position;
        Vector3 posFinal = posInicial + Vector3.back * 5f;
        float duracion = 3f;
        float t = 0f;

        while (t < duracion)
        {
            t += Time.deltaTime;

            // 🔹 Guardar posición anterior
            Vector3 posAnterior = sombraPadre.transform.position;

            // 🔹 Calcular nueva posición
            Vector3 nuevaPos = Vector3.Lerp(posInicial, posFinal, t / duracion);

            // 🔹 Calcular velocidad real del frame
            float velocidadFrame = (nuevaPos - posAnterior).magnitude / Time.deltaTime;

            // 🔹 Actualizar posición
            sombraPadre.transform.position = nuevaPos;

            // 🔹 Actualizar parámetro "speed" en el Animator
            if (anim != null)
                anim.SetFloat("Speed", velocidadFrame);

            yield return null;
        }

        // 🔹 Al terminar, poner speed en 0 para volver a Idle
        if (anim != null)
            anim.SetFloat("Speed", 0f);

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
