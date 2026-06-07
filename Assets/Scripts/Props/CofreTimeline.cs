using UnityEngine;
using UnityEngine.Playables;
using FMODUnity;

public class CofreTimeline : MonoBehaviour
{
    [SerializeField] private string idLlaveRequerida;
    [SerializeField] private GameObject timelineObject;
    [SerializeField] private EventReference sonidoAbrirCofre;

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
            if (!sonidoAbrirCofre.IsNull)
                RuntimeManager.PlayOneShot(sonidoAbrirCofre, transform.position);

            director.Play();
            activado = true;
            Debug.Log("Timeline activada");
        }
    }
}