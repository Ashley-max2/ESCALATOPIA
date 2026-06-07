/// <summary>
/// Implementar en objetos que quieran recibir notificación cuando InteractuableHighlighter los resalta.
/// </summary>
public interface IHighlightable
{
    void OnHighlightStart();
    void OnHighlightEnd();
}
