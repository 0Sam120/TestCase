using UnityEngine;
using UnityEngine.EventSystems;


public class ColourButton : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private SelectableItem targetItem; // Ссылка на объект, которому будет назначен цвет
    [SerializeField] private Sprite itemSprite; // Необязательный спрайт для назначения вместе с цветом (для режима RequiresColour)
    [SerializeField] private Sprite colourSprite; // Спрайт, который будет применён на кукле (для режима RequiresColour)

    public void OnPointerClick(PointerEventData eventData)
    {
        // Игнорируем клики, если игра не в состоянии Idle
        if (InputHandler.Instance.CurrentState != GameState.Idle) return;

        // Назначаем цвет и спрайт (если есть) целевому объекту
        targetItem.AssignColour(colourSprite);
        if(itemSprite != null)
            targetItem.AssignSprite(itemSprite);
    }
}
