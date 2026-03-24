# Makeup Dress-Up Game
Тестовое задание для Playnera, в виде 2D-игры в жанре «одевалки/макияж» на Unity. Реализует систему нанесения косметики на персонажа: крем, тени, помада — с поддержкой drag-and-drop, выбора цвета и управления слоями.
## Технические детали реализации
 
### Finite State Machine
 
Игровой процесс управляется перечислением `GameState` с четырьмя состояниями:
 
```csharp
public enum GameState { Idle, ApplyingCream, ApplyingShadows, ApplyingLipstick }
```
 
`InputHandler` — это **Singleton**, хранящий текущее состояние. Все остальные объекты проверяют `InputHandler.Instance.CurrentState` перед обработкой кликов, что исключает конкурентные взаимодействия.
 
---
 
### Система ввода (New Input System)
 
Используется **Unity Input System** (не устаревший Input Manager):
 
- `InputActions.ScreenInput.Press` — клики мышью
- `InputActions.ScreenInput.Touch` — тач-ввод (мобильные устройства)
- `InputActions.ScreenInput.ScreenValue` — координаты курсора/пальца
 
Оба источника ввода (`Press` и `Touch`) подписаны на одни и те же обработчики `OnPressStarted` / `OnPressReleased`, что обеспечивает единое поведение на ПК и мобильных платформах.
 
---
 
### Drag-and-Drop с плавным перемещением
 
Перетаскивание реализовано через `Vector3.Lerp` в `Update()` для плавного следования за курсором:
 
```csharp
currentObject.transform.position = Vector3.Lerp(
    currentObject.transform.position,
    dragWorldPos,
    Time.deltaTime * dragFollowSpeed
);
```
 
При выборе предмета запускается **корутина** `SlideToCentre`, которая плавно перемещает предмет в центр экрана перед началом drag. Флаг `isSliding` блокирует начало перетаскивания до завершения анимации.
 
---
 
### Определение попадания на слой
 
Попадание при отпускании предмета проверяется через **физику коллайдеров**:
 
1. Если у предмета есть `Collider2D` — используется `bounds.Intersects()` (перекрытие объектов)
2. Если коллайдера нет — `col.OverlapPoint(worldPos)` (попадание курсора в точку)
 
Коллайдер целевого слоя (`CharacterLayer`) включается только на время взаимодействия (`SetColliderActive`), что предотвращает случайные срабатывания.
 
---
 
### Двухрежимный выбор предметов (`SelectionMode`)
 
`SelectableItem` поддерживает два режима через `enum SelectionMode`:
 
- **`Direct`** — предмет сразу начинает движение к персонажу при клике
- **`RequiresColour`** — предмет ждёт, пока игрок не выберет цвет через `ColourButton`, и только после этого запускает взаимодействие
 
```csharp
public void AssignColour(Sprite sprite)
{
    targetSprite = sprite;
    InputHandler.Instance.OnItemSelected(gameObject, associatedState);
}
```
 
---
 
### Слои персонажа (`CharacterLayer`)
 
Три типа слоёв с разным поведением при применении:
 
| Тип | Поведение |
|---|---|
| `Acne` | Скрывает объект (`SetActive(false)`) — имитирует нанесение крема |
| `Shadows` | Меняет спрайт на выбранный вариант теней |
| `Lipstick` | Меняет спрайт на выбранный цвет помады |
 
После применения коллайдер слоя автоматически отключается.
 
---
 
### Навигация по вкладкам UI
 
`PageTransition` управляет отображением панелей «Тени» и «Помада»:
- Хранит ссылку на `activePage`
- При переключении скрывает текущую панель и показывает новую
- По умолчанию открывается страница с тенями (`MakeUpPage`)
 
---
 
## Зависимости
 
- **Unity 2021.3+** (поддержка New Input System)
- **Unity Input System** package
- **2D Physics** (Collider2D для определения попаданий)
 
---
