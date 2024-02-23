using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseControl : MonoBehaviour
{
    private float _previousTimeScale = 1f;
    public static bool gameIsPaused = false;
    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape)){
            TogglePause();
        }
    }

    void TogglePause(){
        if(Time.timeScale > 0f){
            _previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;
            AudioListener.pause = true;
            gameIsPaused = true;
        } else if(Time.timeScale == 0f){
            Time.timeScale = _previousTimeScale;
            AudioListener.pause = false;
            gameIsPaused = false;
        }
    }
}