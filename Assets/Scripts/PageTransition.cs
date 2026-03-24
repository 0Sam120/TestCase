using UnityEngine;
using UnityEngine.UI;


public class PageTransition : MonoBehaviour
{
    public Button LipstickTab; // Кнопка для страницы с помадой
    public Button MakeUpTab; // Кнопка для страницы с тенями
    public GameObject LipstickPage; // Панель с помадой
    public GameObject MakeUpPage; // Панель с тенями
    private GameObject activePage; // Текущая активная страница

    private void Start()
    {
        ShowPage(MakeUpPage); // По умолчанию открываем страницу с тенями
    }

    // Метод для отображения новой страницы и скрытия текущей
    private void ShowPage (GameObject newPage)
    {
        if (activePage != null)
        {
            activePage.SetActive(false);
        }
        newPage.SetActive(true);
        activePage = newPage;
    }

    // Методы для кнопок
    public void OpenMakeUpPage()
    {
        ShowPage(MakeUpPage);
    }

    public void OpenLipstickPage()
    {
        ShowPage(LipstickPage);
    }
}
