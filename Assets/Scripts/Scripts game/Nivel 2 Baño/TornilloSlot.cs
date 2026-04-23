using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TornilloSlot : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Radio de detección (en píxeles UI)")]
    [SerializeField] private float radioDeteccion = 60f;

    [Header("Efecto visual al pasar encima (opcional)")]
    [SerializeField] private Color colorHover = new Color(1f, 1f, 0.5f, 1f);

    public event System.Action OnTornilloQuitado;

    private Image imagen;
    private Color colorOriginal;
    private bool eliminado = false;

    private void Awake()
    {
        imagen = GetComponent<Image>();
        if (imagen != null) colorOriginal = imagen.color;
    }

    public void Reiniciar()
    {
        eliminado = false;
        gameObject.SetActive(true);
        if (imagen != null) imagen.color = colorOriginal;
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (eliminado) return;

        if (eventData.pointerDrag != null &&
            eventData.pointerDrag.GetComponent<DragDestornillador>() != null)
        {
            Eliminar();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!eliminado && imagen != null)
            imagen.color = colorHover;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!eliminado && imagen != null)
            imagen.color = colorOriginal;
    }

    private void Eliminar()
    {
        eliminado = true;
        if (imagen != null) imagen.color = colorOriginal;
        gameObject.SetActive(false);
        OnTornilloQuitado?.Invoke();
    }
}
