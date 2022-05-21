using UnityEngine;
using UnityEngine.EventSystems;

public class MicButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("References")]
    [SerializeField] private AudioManager audioManager;

    public void OnPointerDown(PointerEventData eventData)
    {
        audioManager.NewRecord();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        audioManager.NewRecord();
    }
}
