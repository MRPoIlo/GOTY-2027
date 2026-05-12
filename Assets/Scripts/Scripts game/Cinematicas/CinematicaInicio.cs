using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CinematicaInicio : MonoBehaviour
{
    [Header("Sprite principal")]
    [SerializeField] private Image panelPrincipal; // arrastra aquí el Image que mostrará los sprites
    [SerializeField] private Sprite[] spritesCinematica; // arrastra aquí los sprites en orden

    [Header("Transición")]
    [SerializeField] private CanvasGroup fadeCanvas;
    [SerializeField] private Image imagenTransicion; // imagen que se usa para el fade visual
    [SerializeField] private float duracionFade = 2f;

    [Header("Audio de la cinemática")]
    [SerializeField] private AudioSource audioCinematica;

    [Header("Siguiente escena")]
    [SerializeField] private string escenaPrologo = "Prologo";

    [Header("Duración por panel")]
    [SerializeField] private float duracionPorPanel = 4f;

    private void Start()
    {
        if (audioCinematica != null)
        {
            audioCinematica.loop = true; // 🔹 ahora el audio se repite
            audioCinematica.Play();
        }

        StartCoroutine(ReproducirCinematica());
    }

    private IEnumerator ReproducirCinematica()
    {
        // 🔹 Usar un solo Image y cambiarle el sprite
        for (int i = 0; i < spritesCinematica.Length; i++)
        {
            panelPrincipal.sprite = spritesCinematica[i];
            panelPrincipal.gameObject.SetActive(true);

            yield return StartCoroutine(FadeIn(panelPrincipal));
            yield return new WaitForSeconds(duracionPorPanel);
            yield return StartCoroutine(FadeOut(panelPrincipal));
        }

        // 🔹 Fade final con imagen de transición
        if (imagenTransicion != null)
        {
            imagenTransicion.gameObject.SetActive(true);
            yield return StartCoroutine(FadeIn(imagenTransicion));
        }

        yield return StartCoroutine(FadePantalla(1f, duracionFade));

        // 🔹 Detener audio al terminar
        if (audioCinematica != null)
            audioCinematica.Stop();

        SceneManager.LoadScene(escenaPrologo);
    }

    private IEnumerator FadeIn(Image panel)
    {
        Color color = panel.color;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / 1.5f;
            color.a = Mathf.Lerp(0f, 1f, t);
            panel.color = color;
            yield return null;
        }
    }

    private IEnumerator FadeOut(Image panel)
    {
        Color color = panel.color;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / 1.5f;
            color.a = Mathf.Lerp(1f, 0f, t);
            panel.color = color;
            yield return null;
        }
    }

    private IEnumerator FadePantalla(float objetivo, float duracion)
    {
        if (fadeCanvas == null) yield break;
        float inicio = fadeCanvas.alpha;
        float t = 0f;
        while (t < duracion)
        {
            t += Time.deltaTime;
            fadeCanvas.alpha = Mathf.Lerp(inicio, objetivo, t / duracion);
            yield return null;
        }
        fadeCanvas.alpha = objetivo;
    }
}
