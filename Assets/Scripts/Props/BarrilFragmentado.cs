using UnityEngine;

public class BarrilFragmentado : MonoBehaviour
{
    public GameObject objetoFragmentado;

    public void ActivarFragmentos()
    {
        if (objetoFragmentado != null)
            objetoFragmentado.SetActive(true);

        FragmentoDestruible[] frags =
            objetoFragmentado.GetComponentsInChildren<FragmentoDestruible>();

        foreach (var f in frags)
        {
            f.Lanzar(transform.position);
        }
    }
}