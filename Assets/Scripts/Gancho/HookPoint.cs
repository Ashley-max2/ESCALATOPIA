using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicHookPoint : MonoBehaviour, IHookable
{
    [SerializeField] private bool isValid = true;

    public bool IsValidTarget => isValid;
    public Vector3 HookPoint => transform.position;

    public void OnHookAttach()
    {
        // Efectos cuando se conecta el gancho
        Debug.Log($"Gancho conectado a {gameObject.name}");
    }

    public void OnHookDetach()
    {
        // Efectos cuando se desconecta el gancho
        Debug.Log($"Gancho desconectado de {gameObject.name}");
    }

    void OnDrawGizmos()
    {
        Gizmos.color = isValid ? Color.green : Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
        Gizmos.DrawIcon(transform.position + Vector3.up, "HookPoint.png");
    }
}