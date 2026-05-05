using UnityEngine;
using System.Collections;

public class SombraMadreEvento : MonoBehaviour
{
    [Header("Configuración de fade")]
    [SerializeField] private float duracionFade = 2f;
    [SerializeField] private float tiempoVisible = 1.5f;

    [Header("Sonido de aparición")]
    [SerializeField] private AudioSource sonidoSombra;

    private SpriteRenderer sprite;

    void Awake()
    {
        sprite = GetComponent<SpriteRenderer>();
        sprite.color = new Color(0, 0, 0, 0);
        gameObject.SetActive(false);
    }

    public void ActivarSombra()
    {
        gameObject.SetActive(true);
        if (sonidoSombra != null) sonidoSombra.Play();
        StartCoroutine(FadeInOut());
    }

    private IEnumerator FadeInOut()
    {
        // Fade in
        for (float t = 0; t < duracionFade; t += Time.deltaTime)
        {
            float alpha = t / duracionFade;
            sprite.color = new Color(0, 0, 0, alpha);
            yield return null;
        }

        yield return new WaitForSeconds(tiempoVisible);

        // Fade out
        for (float t = 0; t < duracionFade; t += Time.deltaTime)
        {
            float alpha = 1 - (t / duracionFade);
            sprite.color = new Color(0, 0, 0, alpha);
            yield return null;
        }

        gameObject.SetActive(false);
    }
}
