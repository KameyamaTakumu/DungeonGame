using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CancelButtonUI : MonoBehaviour,
    ISelectHandler, IDeselectHandler,
    IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] Outline outline;
    [SerializeField] Button button;

    void Awake()
    {
        outline.enabled = false;
    }

    // ===== キーボード選択 =====
    public void OnSelect(BaseEventData eventData)
    {
        outline.enabled = true;
    }

    public void OnDeselect(BaseEventData eventData)
    {
        outline.enabled = false;
    }

    // ===== マウス操作 =====
    public void OnPointerEnter(PointerEventData eventData)
    {
        outline.enabled = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        outline.enabled = false;
    }
}
