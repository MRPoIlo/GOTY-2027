using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Animator))]
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
    [SerializeField] private Renderer[] renderers; // aquí arrastras el Renderer del hijo “CuerpoSombra”

    private Animator anim;
    private bool secuenciaIniciada = false;

    void Awake()
    {
        anim = GetComponent<Animator>();
        gameObject.SetActive(false);
        Debug.Log("[SombraMama] Awake: objeto raíz oculto.");
    }

    public void IniciarSecuencia()
    {
        if (secuenciaIniciada) return;
        secuenciaIniciada = true;
        gameObject.SetActive(true);
        Debug.Log("[SombraMama] IniciarSecuencia: activando sombra.");
        StartCoroutine(SecuenciaProteccion());
    }

    private IEnumerator SecuenciaProteccion()
    {
        Debug.Log("[SombraMama] SecuenciaProteccion: inicio.");

        // 1. Aparecer
        transform.position = puntoOrigen.position;
        transform.LookAt(puertoObjetivo);
        particulasPolvo?.Play();

        yield return StartCoroutine(FadeAlpha(0f, 1f, 1.5f));
        Debug.Log("[SombraMama] Fade-in completado.");

        // 2. Caminar hacia la puerta
        if (anim != null) anim.SetBool("Caminando", true);

        while (Vector3.Distance(transform.position, puertoObjetivo.position) > 0.2f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                puertoObjetivo.position,
                velocidadCaminar * Time.deltaTime);

            Debug.Log("[SombraMama] Moviéndose... posición actual: " + transform.position);
            transform.LookAt(puertoObjetivo.position);
            yield return null;
        }

        if (anim != null) anim.SetBool("Caminando", false);
        Debug.Log("[SombraMama] Llegó al puertoObjetivo.");

        // 3. Alejar sombra del padre
        if (sombraPadre != null)
            yield return StartCoroutine(AlejarSombra());

        // 4. Abrir puerta
        if (puertaBlockeada != null)
            puertaBlockeada.SetActive(false);

        // 5. Pausa
        yield return new WaitForSeconds(tiempoEsperaFinal);

        // 6. Fade out
        yield return StartCoroutine(FadeAlpha(1f, 0f, 2.5f));
        particulasPolvo?.Stop();
        gameObject.SetActive(false);

        Debug.Log("[SombraMama] Secuencia terminada.");
        AticoManager.Instance?.OnSecuenciaMamaTerminada();
    }

    private IEnumerator AlejarSombra()
    {
        if (sombraPadre == null) yield break;

        Vector3 posInicial = sombraPadre.transform.position;
        Vector3 posFinal = posInicial + Vector3.back * 5f;
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
            if (r.material.HasProperty("_Color"))
            {
                Color c = r.material.color;
                c.a = alpha;
                r.material.color = c;
            }
            else if (r.material.HasProperty("_BaseColor"))
            {
                Color c = r.material.GetColor("_BaseColor");
                c.a = alpha;
                r.material.SetColor("_BaseColor", c);
            }
        }
    }
}
