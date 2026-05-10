using UnityEngine;
using UnityEngine.Playables;

public class CofreTimeline : MonoBehaviour
{
    [SerializeField] private string idLlaveRequerida;
    [SerializeField] private GameObject timelineObject;

    private bool activado = false;

    public void Interactuar()
    {
        if (activado) return;

        if (InventarioLlaves.instancia == null)
            return;

        if (!InventarioLlaves.instancia.TieneLlave(idLlaveRequerida))
        {
            Debug.Log("Te falta la llave: " + idLlaveRequerida);
            return;
        }

        PlayableDirector director = timelineObject.GetComponent<PlayableDirector>();

        if (director != null)
        {
            director.Play();
            activado = true;
            Debug.Log("Timeline activada");
        }
    }
}