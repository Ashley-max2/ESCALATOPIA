using UnityEngine;
using UnityEngine.UI;

public class punteia : MonoBehaviour
{
    public Image imagen;
    public Color colorEnganchable = Color.green;
    private Color colorOriginal;

    void Start()
    {
        if (imagen != null)
        {
            colorOriginal = imagen.color;
        }
    }

    // Llama a este método cuando el jugador esté apuntando a un HookPoint válido
    public void ActivarColorHookpoint()
    {
        if (imagen != null)
        {
            imagen.color = colorEnganchable;
        }
    }

    // Llama a este método cuando ya no se pueda enganchar
    public void DesactivarColorHookpoint()
    {
        if (imagen != null)
        {
            imagen.color = colorOriginal;
        }
    }
}
