using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Reproduc el sonido de botón cuando presionas el handle de un slider.
/// Coloca este script en el GameObject que tiene el componente Slider.
/// </summary>
public class SliderHandleSound : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private Slider _slider;
    private bool _hasPlayedSound = false;

    private void Start()
    {
        _slider = GetComponent<Slider>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // Solo reproducir sonido si se presiona sobre el handle del slider
        if (_slider != null && !_hasPlayedSound)
        {
            MusicManager.PlayButton();
            _hasPlayedSound = true;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _hasPlayedSound = false;
    }
}
