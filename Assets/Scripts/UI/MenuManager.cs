using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private GameObject _mainMenu;
    [SerializeField] private GameObject _optionsMenu;
    [SerializeField] private GameObject _mainMenuFirstSelected;
    [SerializeField] private GameObject _optionsMenuFirstSelected;
    // Start is called before the first frame update
    void Start()
    {
        EventSystem.current.SetSelectedGameObject(_mainMenuFirstSelected);
    }
    public void StartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
    public void OptionsMenu()
    {
        _mainMenu.SetActive(false);
        _optionsMenu.SetActive(true);
        EventSystem.current.SetSelectedGameObject(_optionsMenuFirstSelected);
    }

    public void MainMenu()
    {
        _optionsMenu.SetActive(false);
        _mainMenu.SetActive(true);
        EventSystem.current.SetSelectedGameObject(_mainMenuFirstSelected);
    }

    public void QuitGame()
    {
        Debug.Log("Quit Game");
        Application.Quit();
    }
}
