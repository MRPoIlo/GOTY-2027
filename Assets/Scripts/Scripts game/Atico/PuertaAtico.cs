using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PuertaAtico : MonoBehaviour, IInteractuable
{
    [SerializeField] private string textoAccion = "Abrir puerta";
    [SerializeField] private string nombreEscenaDestino = "Nivel4";

    [Header("Fade")]
    [SerializeField] private CanvasGroup pantallaFade;
    [SerializeField] private float duracionFade = 1.5f;

    private bool habilitada = false;
    private bool cargando = false;

    public void HabilitarPuerta()
    {
        habilitada = true;
    }

    public void Interactuar()
    {
        if (!habilitada || cargando) return;
        cargando = true;
        StartCoroutine(TransicionConFade());
    }

    private IEnumerator TransicionConFade()
    {
        // Narración opcional antes de salir
        NarracionManager.Instance?.Narrar(new string[]
        {
            "La puerta cede.",
            "Puedo seguir."
        });

        yield return new WaitUntil(() =>
            NarracionManager.Instance == null ||
            !NarracionManager.Instance.EstaActivo());

        // Fade a negro
        if (pantallaFade != null)
        {
            float t = 0f;
            while (t < duracionFade)
            {
                t += Time.deltaTime;
                pantallaFade.alpha = Mathf.Lerp(0f, 1f, t / duracionFade);
                yield return null;
            }
            pantallaFade.alpha = 1f;
        }

        SceneManager.LoadScene(nombreEscenaDestino);
    }

    public string ObtenerTextoAccion() => textoAccion;
    public bool EstaActivo() => habilitada && !cargando;
}