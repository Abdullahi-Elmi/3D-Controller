using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    public void LoadStory()
    {
        SceneManager.LoadScene("StoryScene");
        Time.timeScale = 1f;
    }
    public void LoadLobby()
    {
        SceneManager.LoadScene("Lobby");
        Time.timeScale = 1f;
    }
    public void LoadLevelOne()
    {
        SceneManager.LoadScene("GameScene");
        Time.timeScale = 1f;
    }

    public void LoadLevelTwo()
    {
        SceneManager.LoadScene("PirateScene2");
        Time.timeScale = 1f;
    }

    public void LoadLevelThree()
    {
        SceneManager.LoadScene("MedievalScene1");
        Time.timeScale = 1f;
    }

    public void LoadLevelFour()
    {
        SceneManager.LoadScene("DragonScene");
        Time.timeScale = 1f;
    }

    public void LoadLevelFive()
    {
        SceneManager.LoadScene("KrakenScene");
        Time.timeScale = 1f;
    }

    public void QuitGame()
    {
        Debug.Log("Quit Game");
        Application.Quit();
    }

    // Start is called before the first frame update
    void Start()
    {
        PlayerPrefs.DeleteAll();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
