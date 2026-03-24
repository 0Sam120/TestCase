using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ClearAll : MonoBehaviour
{
    [SerializeField] private CharacterLayer shadowLayer;
    [SerializeField] private CharacterLayer lipstickLayer;

    // При клике очищаем слои теней и помады, если игра в состоянии Idle
    private void OnMouseDown()
    {
        if (InputHandler.Instance.CurrentState != GameState.Idle) return;
        shadowLayer.GetComponent<SpriteRenderer>().sprite = null;
        lipstickLayer.GetComponent<SpriteRenderer>().sprite = null;
    }
}
