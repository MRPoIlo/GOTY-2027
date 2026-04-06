/// <summary>
/// GOTY - Interfaz que deben implementar todos los objetos interactuables del juego.
/// Permite al InteractionSystem detectarlos sin acoplamiento directo.
/// </summary>
public interface IInteractuable
{
    /// <summary>Acción al presionar E.</summary>
    void Interactuar();

    /// <summary>Texto que aparece en el ícono (ej: "Examinar", "Abrir").</summary>
    string ObtenerTextoAccion();

    /// <summary>¿Puede interactuarse ahora? (false = invisible para el sistema).</summary>
    bool EstaActivo();
}