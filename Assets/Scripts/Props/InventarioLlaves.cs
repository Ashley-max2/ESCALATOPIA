using System.Collections.Generic;
using UnityEngine;

public class InventarioLlaves : MonoBehaviour
{
    public static InventarioLlaves instancia;

    private HashSet<string> llaves = new HashSet<string>();

    private void Awake()
    {
        instancia = this;
    }

    public void AgregarLlave(string id)
    {
        llaves.Add(id);
        Debug.Log("Llave obtenida: " + id);
    }

    public bool TieneLlave(string id)
    {
        return llaves.Contains(id);
    }
}