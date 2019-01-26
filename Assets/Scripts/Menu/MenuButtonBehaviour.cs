using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuButtonBehaviour : MonoBehaviour
{
    public Button startButton;
    public Button editorButton;
    public Button quitButton;
    public Button creditsButton;

    private void Start()
    {
        startButton.onClick.AddListener(StartButtonClick);
        editorButton.onClick.AddListener(EditorButtonClick);
        quitButton.onClick.AddListener(QuitButtonClick);
        creditsButton.onClick.AddListener(CreditsButtonClick);
    }

    public void StartButtonClick()
    {
        SceneManager.LoadScene(1);
    }

    private void EditorButtonClick()
    {
        SceneManager.LoadScene(2);
    }

    private void QuitButtonClick()
    {
        Application.Quit();
    }

    private void CreditsButtonClick()
    {
        SceneManager.LoadScene(3);
    }
}
