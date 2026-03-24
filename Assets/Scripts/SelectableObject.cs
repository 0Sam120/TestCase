using UnityEngine;

// Режим выбора объекта: напрямую или с выбором цвета
public enum SelectionMode { Direct, RequiresColour }

[RequireComponent(typeof(Collider2D))] // Гарантируем, что на объекте есть коллайдер для кликов
public class SelectableItem : MonoBehaviour
{
    [SerializeField] private GameState associatedState; // Состояние, которое будет активировано при выборе этого объекта
    [SerializeField] private SelectionMode selectionMode = SelectionMode.Direct; // Режим выбора
    [SerializeField] public Sprite targetSprite; // Спрайт, который будет применён при выборе (для RequiresColour)

    // Метод для назначения спрайта (для режима RequiresColour)
    public void AssignSprite(Sprite sprite)
    {
        this.GetComponent<SpriteRenderer>().sprite = sprite;
    }

    // Метод для получения цвета (для режима RequiresColour)
    public void AssignColour(Sprite sprite)
    {
        targetSprite = sprite;
        InputHandler.Instance.OnItemSelected(gameObject, associatedState);
    }

    private void OnMouseDown()
    {
        // Игнорируем клики, если игра не в состоянии Idle
        if (InputHandler.Instance.CurrentState != GameState.Idle) return;

        // В режиме Direct сразу передаем событие выбора, в режиме RequiresColour - ждем назначения цвета
        if (selectionMode == SelectionMode.Direct)
            InputHandler.Instance.OnItemSelected(gameObject, associatedState);
    }
}