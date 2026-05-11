using UnityEngine;

/// <summary>
/// Helper global para leer la acción de interacción rebindeable y su etiqueta visual.
/// Fallback: tecla E si no hay PlayerInputHandler activo.
/// </summary>
public static class InteractInput
{
    public const string ActionName = "Interactuar";
    private static PlayerInputHandler cachedHandler;

    private static PlayerInputHandler GetHandler()
    {
        if (cachedHandler == null)
            cachedHandler = Object.FindObjectOfType<PlayerInputHandler>();

        return cachedHandler;
    }

    public static bool PressedThisFrame()
    {
        PlayerInputHandler handler = GetHandler();
        if (handler != null)
        {
            KeyCode key = handler.GetActionKey(ActionName);
            if (key != KeyCode.None)
                return Input.GetKeyDown(key);
        }

        return Input.GetKeyDown(KeyCode.E);
    }

    public static string GetDisplayKey()
    {
        PlayerInputHandler handler = GetHandler();
        if (handler != null)
        {
            string label = handler.GetDisplayName(ActionName);
            if (!string.IsNullOrEmpty(label))
                return label;
        }

        return "E";
    }

    public static string GetBracketedDisplayKey()
    {
        return "[" + GetDisplayKey() + "]";
    }

    public static string ReplaceInteractPlaceholder(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        string key = GetBracketedDisplayKey();
        return text.Replace("[E]", key).Replace("{INTERACT}", key);
    }
}
