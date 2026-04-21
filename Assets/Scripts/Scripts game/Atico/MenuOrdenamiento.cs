using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// GOTY — Nivel 3 (Ático)
/// Menú de ordenamiento de recuerdos. El jugador elige en qué posición
/// (1°, 2°, 3°, 4°) va cada fragmento de recuerdo.
/// Se muestra cuando el jugador examina todos los cuadros.
/// </summary>
public class MenuOrdenamiento : MonoBehaviour
{
    public static MenuOrdenamiento Instance { get; private set; }

    [Header("Panel UI")]
    [SerializeField] private GameObject panelOrdenamiento;
    [SerializeField] private TextMeshProUGUI tituloPregunta;

    [Header("Slots de posición (4 botones de posición)")]
    [SerializeField] private Button[] botonesSlot;        // 4 botones: 1°, 2°, 3°, 4°
    [SerializeField] private TextMeshProUGUI[] textoSlots; // texto de cada slot

    [Header("Botones de recuerdo (4 botones, uno por cuadro)")]
    [SerializeField] private Button[] botonesRecuerdo;
    [SerializeField] private TextMeshProUGUI[] textoRecuerdos;

    [Header("Botón confirmar")]
    [SerializeField] private Button botonConfirmar;
    [SerializeField] private TextMeshProUGUI textoResultado;

    // Estado interno
    private List<CuadroRecuerdo> cuadros = new List<CuadroRecuerdo>();
    private int[] ordenElegido;           // ordenElegido[slot] = índice del cuadro asignado (-1 = vacío)
    private int cuadroSeleccionado = -1;  // índice del cuadro que el jugador tiene "en mano"

    private PlayerController player;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        player = FindObjectOfType<PlayerController>();
        panelOrdenamiento?.SetActive(false);
    }

    // ─── Apertura del menú ────────────────────────────────────────────────────

    /// <summary>Recibe los cuadros del AticoManager y abre el menú.</summary>
    public void AbrirMenu(List<CuadroRecuerdo> cuadrosExaminados)
    {
        cuadros = cuadrosExaminados;
        ordenElegido = new int[] { -1, -1, -1, -1 };
        cuadroSeleccionado = -1;

        player?.SetBloqueado(true);

        // Mostrar panel
        panelOrdenamiento?.SetActive(true);

        // Cursor visible para el menú
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (tituloPregunta != null)
            tituloPregunta.text = "¿En qué orden ocurrieron estos recuerdos?";

        if (textoResultado != null)
            textoResultado.text = "";

        // Inicializar botones de recuerdo
        for (int i = 0; i < botonesRecuerdo.Length; i++)
        {
            if (i < cuadros.Count)
            {
                int idx = i; // captura para lambda
                textoRecuerdos[i].text = cuadros[i].GetDescripcion();
                botonesRecuerdo[i].gameObject.SetActive(true);
                botonesRecuerdo[i].onClick.RemoveAllListeners();
                botonesRecuerdo[i].onClick.AddListener(() => SeleccionarRecuerdo(idx));
            }
            else
            {
                botonesRecuerdo[i].gameObject.SetActive(false);
            }
        }

        // Inicializar slots
        for (int i = 0; i < botonesSlot.Length; i++)
        {
            int slot = i;
            textoSlots[i].text = "(vacío)";
            botonesSlot[i].onClick.RemoveAllListeners();
            botonesSlot[i].onClick.AddListener(() => AsignarSlot(slot));
        }

        ActualizarBotones();
    }

    // ─── Lógica de selección ──────────────────────────────────────────────────

    private void SeleccionarRecuerdo(int idx)
    {
        cuadroSeleccionado = idx;
        ActualizarBotones();
    }

    private void AsignarSlot(int slot)
    {
        if (cuadroSeleccionado < 0) return;

        // Si el slot ya tenía algo, lo devolvemos
        if (ordenElegido[slot] >= 0)
        {
            // Quitar del slot y permitir reasignar
            ordenElegido[slot] = -1;
        }

        // Verificar que este cuadro no esté ya en otro slot
        for (int i = 0; i < ordenElegido.Length; i++)
        {
            if (ordenElegido[i] == cuadroSeleccionado)
                ordenElegido[i] = -1;
        }

        ordenElegido[slot] = cuadroSeleccionado;
        cuadroSeleccionado = -1;

        ActualizarBotones();
    }

    private void ActualizarBotones()
    {
        // Actualizar texto de slots
        for (int i = 0; i < textoSlots.Length; i++)
        {
            textoSlots[i].text = ordenElegido[i] >= 0
                ? cuadros[ordenElegido[i]].GetDescripcion()
                : "(vacío)";
        }

        // Resaltar recuerdo seleccionado
        for (int i = 0; i < botonesRecuerdo.Length; i++)
        {
            if (i < cuadros.Count)
            {
                var colors = botonesRecuerdo[i].colors;
                colors.normalColor = (i == cuadroSeleccionado)
                    ? new Color(0.8f, 0.7f, 0.3f) // amarillo seleccionado
                    : Color.white;
                botonesRecuerdo[i].colors = colors;
            }
        }

        // Habilitar confirmar solo si todos los slots están llenos
        bool todosLlenos = true;
        foreach (int v in ordenElegido)
            if (v < 0) { todosLlenos = false; break; }

        if (botonConfirmar != null)
        {
            botonConfirmar.interactable = todosLlenos;
            botonConfirmar.onClick.RemoveAllListeners();
            botonConfirmar.onClick.AddListener(ConfirmarOrden);
        }
    }

    // ─── Confirmación ─────────────────────────────────────────────────────────

    public void ConfirmarOrden()
    {
        // Verificar si el orden cronológico es correcto
        bool correcto = true;
        for (int slot = 0; slot < ordenElegido.Length; slot++)
        {
            int idxCuadro = ordenElegido[slot];
            // slot 0 = 1° posición → debe tener el cuadro con ordenCronologico == 1
            if (cuadros[idxCuadro].ordenCronologico != slot + 1)
            {
                correcto = false;
                break;
            }
        }

        if (correcto)
        {
            CerrarMenu();
            AticoManager.Instance?.OnOrdenCorrecto();
        }
        else
        {
            if (textoResultado != null)
                textoResultado.text = "No… así no fue. Intenta recordar de nuevo.";

            // Limpiar selecciones para reintentar
            ordenElegido = new int[] { -1, -1, -1, -1 };
            cuadroSeleccionado = -1;
            ActualizarBotones();
        }
    }

    // ─── Cierre ───────────────────────────────────────────────────────────────

    public void CerrarMenu()
    {
        panelOrdenamiento?.SetActive(false);
        player?.SetBloqueado(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}