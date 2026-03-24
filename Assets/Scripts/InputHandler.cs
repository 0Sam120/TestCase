using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

// Состояния игры — что сейчас делает игрок
public enum GameState
{
    Idle,               // Ничего не делает
    ApplyingCream,      // Наносит крем
    ApplyingShadows,    // Наносит тени
    ApplyingLipstick    // Наносит помаду
}

public class InputHandler : MonoBehaviour
{
    // Центр экрана в мировых координатах (куда притягивается объект)
    [SerializeField] private Vector3 centreScreenWorldPosition;

    // Скорость следования объекта за пальцем/мышью
    [SerializeField] private float dragFollowSpeed = 20f;

    // Скорость "возврата" в центр
    [SerializeField] private float slideToCentreSpeed = 8f;

    // Слои персонажа под разные действия
    [SerializeField] private CharacterLayer acneLayer;
    [SerializeField] private CharacterLayer shadowLayer;
    [SerializeField] private CharacterLayer lipstickLayer;

    // Текущее состояние игры
    public GameState CurrentState { get; private set; } = GameState.Idle;

    // Input System
    private InputActions inputActions;
    private InputAction pressAction;
    private InputAction screenValueAction;
    private InputAction touchAction;

    // Текущий выбранный объект и его данные
    private GameObject currentObject;
    private SelectableItem currentItem;
    public CharacterLayer currentLayer;

    private bool isDragging = false; // сейчас тянем объект
    private bool isSliding = false;  // сейчас анимация к центру

    private Vector3 dragWorldPos;    // позиция курсора в мире
    private Vector3 objectOrigin;    // изначальная позиция объекта

    private Camera cam;

    // Singleton (один инпут на всю сцену)
    public static InputHandler Instance { get; private set; }

    private void Awake()
    {
        // Защита от дубликатов
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        cam = Camera.main;

        // Инициализация Input System
        inputActions = new InputActions();

        pressAction = inputActions.ScreenInput.Press;
        screenValueAction = inputActions.ScreenInput.ScreenValue;
        touchAction = inputActions.ScreenInput.Touch;
    }

    private void OnEnable()
    {
        // Подписка на события нажатия
        pressAction.started += OnPressStarted;
        pressAction.canceled += OnPressReleased;

        // Поддержка тача
        touchAction.started += OnPressStarted;
        touchAction.canceled += OnPressReleased;

        pressAction.Enable();
        screenValueAction.Enable();
        touchAction.Enable();
    }

    private void OnDisable()
    {
        // Отписка от событий
        pressAction.started -= OnPressStarted;
        pressAction.canceled -= OnPressReleased;

        touchAction.started -= OnPressStarted;
        touchAction.canceled -= OnPressReleased;

        pressAction.Disable();
        screenValueAction.Disable();
        touchAction.Disable();
    }

    private void Update()
    {
        // Пока тащим объект — обновляем его позицию
        if (isDragging && currentObject != null)
            UpdateDragPosition();
    }

    // Вызывается, когда игрок кликает на предмет
    public void OnItemSelected(GameObject item, GameState newState)
    {
        CurrentState = newState;
        currentObject = item;
        currentItem = item.GetComponent<SelectableItem>();

        // Получаем нужный слой под текущее действие
        currentLayer = LayerForState(newState);

        // Включаем коллайдер слоя (чтобы можно было попадать)
        if (currentLayer != null)
            currentLayer.SetColliderActive(true);

        // Запоминаем начальную позицию
        objectOrigin = item.transform.position;

        // Плавно двигаем предмет в центр
        StartCoroutine(SlideToCentre(item));
    }

    private void OnPressStarted(InputAction.CallbackContext context)
    {
        // Пока объект "летит" в центр — не даём начать drag
        if (isSliding) return;

        // Можно тащить только если есть активный объект
        if (CurrentState != GameState.Idle && currentObject != null)
        {
            Vector2 screenPos = screenValueAction.ReadValue<Vector2>();
            BeginDrag(ScreenToWorld(screenPos));
        }
    }

    private void OnPressReleased(InputAction.CallbackContext context)
    {
        // Завершаем перетаскивание
        if (isDragging)
        {
            Vector2 screenPos = screenValueAction.ReadValue<Vector2>();
            EndDrag(screenPos);
        }
    }

    private void BeginDrag(Vector3 worldPos)
    {
        isDragging = true;
        dragWorldPos = worldPos;
    }

    private void UpdateDragPosition()
    {
        // Получаем текущую позицию курсора
        Vector2 screenPos = screenValueAction.ReadValue<Vector2>();
        dragWorldPos = ScreenToWorld(screenPos);

        // Плавно двигаем объект за курсором
        currentObject.transform.position = Vector3.Lerp(
            currentObject.transform.position,
            dragWorldPos,
            Time.deltaTime * dragFollowSpeed);
    }

    private void EndDrag(Vector2 screenPos)
    {
        isDragging = false;

        // Если попали по нужному слою
        if (IsOverTargetLayer(screenPos))
        {
            // Применяем эффект (например, меняем спрайт)
            currentLayer.Apply(currentItem != null ? currentItem.targetSprite : null);

            ReturnToIdle();
        }
        else
        {
            // Если промазали — возвращаем в центр
            StartCoroutine(SlideToCentre(currentObject));
        }
    }

    private void ReturnToIdle()
    {
        // Возвращаем объект на исходную позицию
        currentObject.transform.position = objectOrigin;

        // Сбрасываем всё
        currentObject = null;
        currentItem = null;
        currentLayer = null;
        CurrentState = GameState.Idle;
    }

    private IEnumerator SlideToCentre(GameObject item)
    {
        isSliding = true;

        // Пока не доехали до центра
        while (Vector3.Distance(item.transform.position, centreScreenWorldPosition) > 0.01f)
        {
            item.transform.position = Vector3.Lerp(
                item.transform.position,
                centreScreenWorldPosition,
                Time.deltaTime * slideToCentreSpeed);

            yield return null;
        }

        // Фиксируем позицию
        item.transform.position = centreScreenWorldPosition;
        isSliding = false;
    }

    private bool IsOverTargetLayer(Vector2 screenPos)
    {
        if (currentLayer == null) return false;

        Collider2D col = currentLayer.GetComponent<Collider2D>();
        if (col == null) return false;

        // Если у предмета есть коллайдер — проверяем пересечение
        Collider2D itemCol = currentObject.GetComponent<Collider2D>();
        if (itemCol != null)
            return itemCol.bounds.Intersects(col.bounds);

        // Иначе проверяем точку курсора
        Vector2 worldPos = cam.ScreenToWorldPoint(screenPos);
        return col.OverlapPoint(worldPos);
    }

    // Связывает состояние с нужным слоем
    private CharacterLayer LayerForState(GameState state) => state switch
    {
        GameState.ApplyingCream => acneLayer,
        GameState.ApplyingShadows => shadowLayer,
        GameState.ApplyingLipstick => lipstickLayer,
        _ => null
    };

    private Vector3 ScreenToWorld(Vector2 screenPos)
    {
        Vector3 pos = cam.ScreenToWorldPoint(screenPos);
        pos.z = 0f; // фиксируем z для 2D
        return pos;
    }
}