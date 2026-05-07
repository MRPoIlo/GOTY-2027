using UnityEngine;
using System.Collections;

public class SombraMadreEvento : MonoBehaviour
{
    [Header("Configuración de fade")]
    [SerializeField] private float duracionFade = 2f;

    private SpriteRenderer sprite;
    private Animator animator;

    void Awake()
    {
        // Busca el SpriteRenderer en este objeto (solo si es 2D)
        sprite = GetComponent<SpriteRenderer>();

        // Busca el Animator en este objeto o en sus hijos
        animator = GetComponent<Animator>();
        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        // ⚠️ Ya no lo desactivamos aquí, para que Awake se ejecute
        if (sprite != null)
            sprite.color = new Color(0, 0, 0, 0);
    }

    // ─── ACTIVAR ─────────────────────────
    public void ActivarSombra()
    {
        gameObject.SetActive(true);

        if (animator != null)
            animator.Play("Llorar");

        if (sprite != null)
            StartCoroutine(FadeIn());
    }

    // ─── DESACTIVAR ─────────────────────────
    public void DesactivarSombra()
    {
        if (sprite != null)
            StartCoroutine(FadeOut());
        else
            gameObject.SetActive(false); // si es 3D, simplemente desactiva
    }

    // ─── FADE IN ─────────────────────────
    private IEnumerator FadeIn()
    {
        for (float t = 0; t < duracionFade; t += Time.deltaTime)
        {
            float alpha = t / duracionFade;
            sprite.color = new Color(0, 0, 0, alpha);
            yield return null;
        }
        sprite.color = new Color(0, 0, 0, 1);
    }

    // ─── FADE OUT ─────────────────────────
    private IEnumerator FadeOut()
    {
        for (float t = 0; t < duracionFade; t += Time.deltaTime)
        {
            float alpha = 1 - (t / duracionFade);
            sprite.color = new Color(0, 0, 0, alpha);
            yield return null;
        }

        sprite.color = new Color(0, 0, 0, 0);
        gameObject.SetActive(false);
    }
}
