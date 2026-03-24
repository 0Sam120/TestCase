using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class CharacterLayer : MonoBehaviour
{
    public enum LayerType { Acne, Shadows, Lipstick } // “ип сло€ дл€ определени€ его поведени€ при применении спрайта

    [SerializeField] private LayerType layerType; // “ип сло€, который будет определ€ть его поведение при применении спрайта

    private Collider2D col; //  оллайдер дл€ управлени€ кликами на слое

    // »нициализаци€ коллайдера и его отключение при старте, чтобы слой не реагировал на клики до применени€ спрайта
    private void Awake()
    {
        col = GetComponent<Collider2D>();
        col.enabled = false;
    }

    public void SetColliderActive(bool active)
    {
        col.enabled = active; // ¬ключаем или отключаем коллайдер в зависимости от переданного параметра
    }

    public void Apply(Sprite sprite)
    {
        // ¬ зависимости от типа сло€ выполн€ем разные действи€ при применении спрайта
        switch (layerType)
        {
            case LayerType.Acne:
                gameObject.SetActive(false);
                break;

            case LayerType.Lipstick:
                if (sprite != null)
                    GetComponent<SpriteRenderer>().sprite = sprite;
                break;

            case LayerType.Shadows:
                if (sprite != null)
                    GetComponent<SpriteRenderer>().sprite = sprite;
                break;
        }

        // ѕосле применени€ спрайта отключаем коллайдер, чтобы слой не реагировал на клики
        col.enabled = false;
    }
}