using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CreditsButtonBehaviour : MonoBehaviour
{
    public Button backButton;
    public Button detailsButton;
    public Canvas detailsCanvas;

    private void Start()
    {
        backButton.onClick.AddListener(BackButtonClick);
        detailsButton.onClick.AddListener(DetailsButtonClick);
    }

    private void BackButtonClick()
    {
        SceneManager.LoadScene(0);
    }

    private void DetailsButtonClick()
    {
        detailsCanvas.enabled = !detailsCanvas.enabled;
    }
}
