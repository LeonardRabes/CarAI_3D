using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Assets.Scripts.Main;

public class MainUIBehaviour : MonoBehaviour
{
    public SimulationManager simulationManager;
    public Camera targetCamera;
    public GameObject targetGrid;

    public Text generationText;
    public Toggle toggleGrid;
    public Toggle toggleDetails;
    public Toggle toggleBest;
    public Button switchSpeed;
    public Button backToMenu;
    public Button saveStructure;
    public Image neuralNetImage;
    public Texture2D[] neuralNetTextures;

    private CameraMovement camMove;
    private MeshRenderer gridRenderer;
    private int currentBestCar = 0;
    private int currentSpeedState = 0;

    private void Start()
    {
        camMove = targetCamera.GetComponent<CameraMovement>();
        toggleGrid.onValueChanged.AddListener(OnToggleGrid);
        toggleDetails.onValueChanged.AddListener(OnToggleDetails);
        toggleBest.onValueChanged.AddListener(OnToggleBest);
        backToMenu.onClick.AddListener(OnMenuButtonClick);
        saveStructure.onClick.AddListener(OnSaveStructureButtonClick);
        switchSpeed.onClick.AddListener(OnSwitchSpeedClick);
    }

    private void FixedUpdate()
    {
        generationText.text = "Generation: " + simulationManager.simulationEngine.Generation;

        if (!camMove.followBestCar)
        {
            toggleBest.isOn = false;
        }

        int best = simulationManager.GetBestCar();
        if (best != currentBestCar && best != -1)
        {
            
            Texture2D texture = neuralNetImage.sprite.texture;
            NeuralNetImage.CreateTexture(simulationManager.simulationEngine.Cars[best].Brain, texture, neuralNetTextures[0], new Rect(0, 0, 300, 200), neuralNetTextures[1], neuralNetTextures[2], neuralNetTextures[3]);
            texture.Apply();

            currentBestCar = best;
        }
    }

    private void OnToggleGrid(bool val)
    {
        targetGrid.GetComponent<MeshRenderer>().enabled = val;
    }

    private void OnToggleDetails(bool val)
    {
        simulationManager.showDetails = val;
    }

    private void OnToggleBest(bool val)
    {
        camMove.followBestCar = val;
    }

    private void OnMenuButtonClick()
    {
        SceneManager.LoadScene(0);
    }

    private void OnSaveStructureButtonClick()
    {
        bool dirExists = Directory.Exists(@"save\");

        if (!dirExists)
        {
            Directory.CreateDirectory(@"save\");
        }

        if (simulationManager.simulationEngine.CarRanking != null)
        {
            MemoryStream mstream = simulationManager.simulationEngine.CarRanking[simulationManager.simulationEngine.CarRanking.Length - 1].Brain.BuildStructureStream();
            FileStream fstream = new FileStream(@"save\save.carStream", FileMode.Create);
            mstream.WriteTo(fstream);
            mstream.Close();
            fstream.Close();
        }

        saveStructure.enabled = false;
        saveStructure.enabled = true;
    }

    private void OnSwitchSpeedClick()
    {
        Text txt = switchSpeed.transform.GetChild(0).gameObject.GetComponent<Text>();
        currentSpeedState++;
        switch (currentSpeedState)
        {
            case 0:
                simulationManager.simulationLoopSpeed = 1;
                txt.text = "Speed: x1";
                break;
            case 1:
                simulationManager.simulationLoopSpeed = 0.5F;
                txt.text = "Speed: x2";
                break;
            case 2:
                simulationManager.simulationLoopSpeed = 0.25F;
                txt.text = "Speed: x4";
                break;
            case 3:
                simulationManager.simulationLoopSpeed = 0.125F;
                txt.text = "Speed: x8";
                break;
            case 4:
                simulationManager.simulationLoopSpeed = 0;
                txt.text = "Speed: Max";
                break;
            default:
                currentSpeedState = 0;
                simulationManager.simulationLoopSpeed = 1;
                txt.text = "Speed: x1";
                break;
        }
    }
}
