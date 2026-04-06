using UnityEngine;

/// <summary>
/// GOTY — Constructor procedural de la geometría del Prólogo.
/// Ejecuta en el Editor con el botón del Inspector (ContextMenu).
/// También funciona en Awake para prototipo rápido.
///
/// USO:
///   1. Crea un GameObject vacío → "SceneBuilder"
///   2. Añade este script
///   3. Clic derecho en el componente → "Construir Escena Prólogo"
///   4. Los objetos aparecen en el Hierarchy listos para configurar
/// </summary>
public class SceneBuilder_Prologo : MonoBehaviour
{
    [Header("Materiales (asignar en Inspector)")]
    [Tooltip("Material oscuro para paredes/suelo — color #2C2420")]
    public Material matParedes;
    [Tooltip("Material de madera desgastada para muebles")]
    public Material matMadera;
    [Tooltip("Material de tela vieja para la cama")]
    public Material matTela;

    [Header("Opciones")]
    public bool construirAlIniciar = false;

    void Awake()
    {
        if (construirAlIniciar)
            ConstruirEscena();
    }

    [ContextMenu("Construir Escena Prólogo")]
    public void ConstruirEscena()
    {
        LimpiarEscena();

        GameObject raiz = new GameObject("=== PROLOGO ===");

        ConstruirHabitacion(raiz);
        ConstruirPasillo(raiz);
        ConstruirIluminacion(raiz);

        Debug.Log("[SceneBuilder] Prólogo construido. Recuerda asignar materiales y colliders.");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // HABITACIÓN PRINCIPAL
    // ─────────────────────────────────────────────────────────────────────────

    void ConstruirHabitacion(GameObject raiz)
    {
        GameObject hab = new GameObject("Habitacion");
        hab.transform.parent = raiz.transform;

        // Dimensiones: 8m ancho × 3m alto × 6m prof
        float w = 8f, h = 3f, d = 6f;

        // Suelo
        CrearPlano("Suelo", hab, new Vector3(0, 0, 0), new Vector3(w, 1, d), matParedes);

        // Techo
        CrearPlano("Techo", hab, new Vector3(0, h, 0), new Vector3(w, 1, d), matParedes);

        // Paredes
        CrearCubo("Pared_Norte", hab, new Vector3(0, h / 2, d / 2), new Vector3(w, h, 0.2f), matParedes);
        CrearCubo("Pared_Sur",   hab, new Vector3(0, h / 2, -d / 2), new Vector3(w, h, 0.2f), matParedes);
        CrearCubo("Pared_Oeste", hab, new Vector3(-w / 2, h / 2, 0), new Vector3(0.2f, h, d), matParedes);
        // Pared este: dos segmentos con hueco para la puerta (1m ancho × 2.1m alto)
        CrearCubo("Pared_Este_Arriba",  hab, new Vector3(w / 2, 2.55f, 0),     new Vector3(0.2f, 0.9f, d),     matParedes);
        CrearCubo("Pared_Este_IzqBajo", hab, new Vector3(w / 2, 1.05f, -1.55f), new Vector3(0.2f, 2.1f, 2.9f), matParedes);
        CrearCubo("Pared_Este_DerBajo", hab, new Vector3(w / 2, 1.05f,  1.55f), new Vector3(0.2f, 2.1f, 2.9f), matParedes);

        // ── Mobiliario ──

        // Cama (esquina noroeste)
        GameObject cama = CrearCubo("Cama", hab,
            new Vector3(-3f, 0.3f, 2f), new Vector3(1.4f, 0.6f, 2f), matTela);
        // Cabecera
        CrearCubo("Cama_Cabecera", cama, new Vector3(0, 0.5f, 1f), new Vector3(1.4f, 1f, 0.1f), matMadera);
        // Almohada (objeto interactuable — trigger de inicio)
        GameObject almohada = CrearCubo("Almohada", cama,
            new Vector3(0, 0.35f, 0.6f), new Vector3(0.5f, 0.1f, 0.35f), matTela);
        almohada.tag = "Interactuable";

        // Mesita de noche
        GameObject mesita = CrearCubo("Mesita_Noche", hab,
            new Vector3(-1.5f, 0.35f, 2f), new Vector3(0.5f, 0.7f, 0.5f), matMadera);
        // Cajón (objeto narrativo principal)
        GameObject cajon = CrearCubo("Cajon_Mesita", mesita,
            new Vector3(0, 0.1f, 0), new Vector3(0.45f, 0.2f, 0.45f), matMadera);
        cajon.tag = "Interactuable";
        AgregarObjetoInteractuable(cajon,
            "Abrir",
            new string[]
            {
                "Vacío. Siempre estuvo vacío.",
                "O quizás lo vaciaron antes de que yo llegara."
            });

        // Escritorio (pared sur)
        GameObject escritorio = CrearCubo("Escritorio", hab,
            new Vector3(-2.5f, 0.75f, -2.6f), new Vector3(1.6f, 0.05f, 0.7f), matMadera);
        // Patas
        CrearCubo("Pata_EscA", escritorio, new Vector3(-0.75f, -0.75f, 0.3f),  new Vector3(0.06f, 1.5f, 0.06f), matMadera);
        CrearCubo("Pata_EscB", escritorio, new Vector3( 0.75f, -0.75f, 0.3f),  new Vector3(0.06f, 1.5f, 0.06f), matMadera);
        CrearCubo("Pata_EscC", escritorio, new Vector3(-0.75f, -0.75f, -0.3f), new Vector3(0.06f, 1.5f, 0.06f), matMadera);
        CrearCubo("Pata_EscD", escritorio, new Vector3( 0.75f, -0.75f, -0.3f), new Vector3(0.06f, 1.5f, 0.06f), matMadera);

        // Teléfono desconectado (sobre escritorio)
        GameObject telefono = CrearCubo("Telefono", escritorio,
            new Vector3(-0.55f, 0.08f, 0), new Vector3(0.18f, 0.1f, 0.12f), matMadera);
        telefono.tag = "Interactuable";
        AgregarObjetoInteractuable(telefono,
            "Examinar",
            new string[]
            {
                "No tiene tono. Nunca lo tuvo después de esa noche.",
                "Recuerdo haberlo querido usar. No pude."
            });

        // Fotografía familiar (sobre escritorio)
        GameObject foto = CrearCubo("Fotografia", escritorio,
            new Vector3(0.4f, 0.08f, -0.1f), new Vector3(0.18f, 0.24f, 0.02f), matParedes);
        foto.tag = "Interactuable";
        AgregarObjetoInteractuable(foto,
            "Mirar",
            new string[]
            {
                "Somos nosotros. Antes de que todo cambiara.",
                "Yo tenía unos seis años aquí."
            });

        // Silla del escritorio
        GameObject silla = CrearCubo("Silla", hab,
            new Vector3(-2.5f, 0.45f, -1.8f), new Vector3(0.5f, 0.05f, 0.5f), matMadera);
        CrearCubo("Silla_Respaldo", silla, new Vector3(0, 0.4f, -0.22f), new Vector3(0.5f, 0.8f, 0.05f), matMadera);
        foreach (var pos in new[] {
            new Vector3(-0.22f,-0.45f, 0.22f), new Vector3(0.22f,-0.45f, 0.22f),
            new Vector3(-0.22f,-0.45f,-0.22f), new Vector3(0.22f,-0.45f,-0.22f) })
            CrearCubo("PataS", silla, pos, new Vector3(0.05f, 0.9f, 0.05f), matMadera);

        // Armario (pared este, interior cuarto)
        GameObject armario = CrearCubo("Armario", hab,
            new Vector3(3.3f, 1.1f, 2f), new Vector3(1.0f, 2.2f, 0.6f), matMadera);
        // Puerta del armario (cerrada, collider sólido)
        CrearCubo("Armario_Puerta", armario,
            new Vector3(-0.5f, 0f, 0.31f), new Vector3(0.95f, 2.1f, 0.05f), matMadera);

        // Dibujo infantil en la pared norte
        GameObject dibujo = CrearCubo("Dibujo_Pared", hab,
            new Vector3(1.5f, 1.4f, 2.89f), new Vector3(0.4f, 0.3f, 0.02f), matParedes);
        dibujo.tag = "Interactuable";
        AgregarObjetoInteractuable(dibujo,
            "Mirar",
            new string[]
            {
                "Lo dibujé yo. Una casa con sol.",
                "Qué curioso que nunca puse una puerta."
            });

        // Espejo roto (pared este)
        GameObject espejo = CrearCubo("Espejo_Roto", hab,
            new Vector3(3.89f, 1.3f, -0.5f), new Vector3(0.02f, 0.8f, 0.5f), matParedes);
        espejo.tag = "Interactuable";
        AgregarObjetoInteractuable(espejo,
            "Mirar",
            new string[]
            {
                "Hay algo mal en el reflejo.",
                "O en mí."
            });

        // Ventana tapiada (pared norte)
        CrearCubo("Ventana_Tapiada", hab,
            new Vector3(0, 1.6f, 2.89f), new Vector3(1.2f, 0.8f, 0.05f), matParedes);

        // Alfombra desgastada (suelo, zona cama-escritorio)
        CrearPlano("Alfombra", hab,
            new Vector3(-2f, 0.01f, 0), new Vector3(4f, 1, 4f), matTela);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // PASILLO
    // ─────────────────────────────────────────────────────────────────────────

    void ConstruirPasillo(GameObject raiz)
    {
        GameObject pasillo = new GameObject("Pasillo");
        pasillo.transform.parent = raiz.transform;
        pasillo.transform.position = new Vector3(8.1f, 0, 0); // contiguo a la habitación

        float w = 1.8f, h = 3f, d = 6f;

        CrearPlano("Suelo_P",   pasillo, Vector3.zero,                new Vector3(w, 1, d),           matParedes);
        CrearPlano("Techo_P",   pasillo, new Vector3(0, h, 0),        new Vector3(w, 1, d),           matParedes);
        CrearCubo("Pared_P_N", pasillo, new Vector3(0, h/2,  d/2),   new Vector3(w, h, 0.2f),       matParedes);
        CrearCubo("Pared_P_O", pasillo, new Vector3(-w/2, h/2, 0),   new Vector3(0.2f, h, d),       matParedes);
        CrearCubo("Pared_P_E", pasillo, new Vector3( w/2, h/2, 0),   new Vector3(0.2f, h, d),       matParedes);
        // Pared sur: hueco de puerta (salida bloqueada)
        CrearCubo("Pared_P_S_Iz",    pasillo, new Vector3(-0.55f, h/2, -d/2), new Vector3(0.7f, h, 0.2f),    matParedes);
        CrearCubo("Pared_P_S_Der",   pasillo, new Vector3( 0.55f, h/2, -d/2), new Vector3(0.7f, h, 0.2f),    matParedes);
        CrearCubo("Pared_P_S_Arriba",pasillo, new Vector3(0, 2.55f, -d/2),    new Vector3(w, 0.9f, 0.2f),    matParedes);

        // Cuadro torcido
        GameObject cuadro = CrearCubo("Cuadro_Torcido", pasillo,
            new Vector3(-0.7f, 1.5f, 1.5f), new Vector3(0.02f, 0.4f, 0.3f), matParedes);
        cuadro.transform.Rotate(0, 0, 8f); // ligeramente inclinado
        cuadro.tag = "Interactuable";
        AgregarObjetoInteractuable(cuadro,
            "Mirar",
            new string[]
            {
                "Nunca estuvo recto. Nadie lo arregló nunca."
            });

        // Teléfono de pared
        GameObject telPared = CrearCubo("Telefono_Pared", pasillo,
            new Vector3(0.7f, 1.5f, 0.5f), new Vector3(0.05f, 0.18f, 0.12f), matMadera);
        telPared.tag = "Interactuable";
        AgregarObjetoInteractuable(telPared,
            "Examinar",
            new string[]
            {
                "Cuántas veces escuché sonar desde mi cuarto.",
                "Y cuántas veces nadie contestó."
            });

        // Puerta de salida (bloqueada, objeto sólido)
        GameObject puertaSal = CrearCubo("Puerta_Salida", pasillo,
            new Vector3(0, 1.05f, -d / 2 + 0.1f), new Vector3(1f, 2.1f, 0.08f), matMadera);
        puertaSal.tag = "Interactuable";
        AgregarObjetoInteractuable(puertaSal,
            "Abrir",
            new string[]
            {
                "No se mueve.",
                "Hay algo que no me deja salir."
            },
            usoUnico: false); // puede intentarse varias veces
    }

    // ─────────────────────────────────────────────────────────────────────────
    // ILUMINACIÓN
    // ─────────────────────────────────────────────────────────────────────────

    void ConstruirIluminacion(GameObject raiz)
    {
        GameObject luces = new GameObject("Iluminacion");
        luces.transform.parent = raiz.transform;

        // Luz ambiente global (muy oscura, cálida-apagada)
        RenderSettings.ambientLight = new Color(0.06f, 0.05f, 0.04f);

        // Lámpara del cuarto (spot tenue)
        GameObject lampara = new GameObject("Lampara_Cuarto");
        lampara.transform.parent = luces.transform;
        lampara.transform.position = new Vector3(0, 2.8f, 0);
        Light luz = lampara.AddComponent<Light>();
        luz.type      = LightType.Point;
        luz.color     = new Color(0.78f, 0.65f, 0.45f); // cálida-apagada
        luz.intensity = 0.5f;
        luz.range     = 6f;

        // Foco del pasillo (aún más oscuro)
        GameObject fPasillo = new GameObject("Foco_Pasillo");
        fPasillo.transform.parent = luces.transform;
        fPasillo.transform.position = new Vector3(8.1f, 2.8f, 0);
        Light lp = fPasillo.AddComponent<Light>();
        lp.type      = LightType.Point;
        lp.color     = new Color(0.5f, 0.48f, 0.42f);
        lp.intensity = 0.25f;
        lp.range     = 4f;

        // Luz de relleno muy débil (evita negro absoluto)
        GameObject relleno = new GameObject("Luz_Relleno");
        relleno.transform.parent = luces.transform;
        relleno.transform.position = new Vector3(0, 2.5f, 0);
        Light lr = relleno.AddComponent<Light>();
        lr.type      = LightType.Directional;
        lr.color     = new Color(0.15f, 0.12f, 0.1f);
        lr.intensity = 0.08f;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // HELPERS
    // ─────────────────────────────────────────────────────────────────────────

    GameObject CrearCubo(string nombre, GameObject padre, Vector3 posLocal, Vector3 escala, Material mat)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = nombre;
        go.transform.parent        = padre.transform;
        go.transform.localPosition = posLocal;
        go.transform.localScale    = escala;
        if (mat != null)
            go.GetComponent<Renderer>().material = mat;
        return go;
    }

    GameObject CrearPlano(string nombre, GameObject padre, Vector3 posLocal, Vector3 escala, Material mat)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Plane);
        go.name = nombre;
        go.transform.parent        = padre.transform;
        go.transform.localPosition = posLocal;
        // Plane de Unity es 10x10 por defecto → escala real = escala / 10
        go.transform.localScale    = new Vector3(escala.x / 10f, 1, escala.z / 10f);
        if (mat != null)
            go.GetComponent<Renderer>().material = mat;
        return go;
    }

    /// <summary>
    /// Marca el objeto con Tag e Layer "Interactuable" y añade el texto
    /// de acción al nombre para orientarse al asignar ObjetoInteractuable
    /// manualmente desde el Inspector después de construir la escena.
    /// </summary>
    void AgregarObjetoInteractuable(GameObject go, string textoAccion, string[] narracion,
                                    bool usoUnico = true)
    {
        // Renombrar con la acción para identificarlo fácil en el Hierarchy
        go.name += " [" + textoAccion + "]";

        // Tag "Interactuable" (créalo en Edit > Project Settings > Tags & Layers)
        go.tag = "Interactuable";

        // Layer "Interactuable" (mismo sitio)
        int layer = LayerMask.NameToLayer("Interactuable");
        if (layer >= 0) go.layer = layer;

        // SIGUIENTE PASO: selecciona cada objeto marcado con [Acción] en el
        // Hierarchy y añade manualmente el componente ObjetoInteractuable
        // con Add Component > Scripts > ObjetoInteractuable
    }

    void LimpiarEscena()
    {
        var viejo = GameObject.Find("=== PROLOGO ===");
        if (viejo != null) DestroyImmediate(viejo);
    }
}