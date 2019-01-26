using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class EditorUIBehaviour : MonoBehaviour
{
    public Button selectPen;
    public Button selectEraser;
    public Button selectSpawn;
    public Button selectTarget;
    public Button saveBackground;
    public Button resetBackground;
    public Button backToMenu;
    public Toggle loadSave;

    public Text toolName;
    public Text toolTips;
    public Text MessageText;

    private Button[] buttons;
    private DrawManager manager;
    private bool IsSaveLoaded = false;

    void Start()
    {
        selectPen.onClick.AddListener(ButtonClickPen);
        selectEraser.onClick.AddListener(ButtonClickEraser);
        selectSpawn.onClick.AddListener(ButtonClickSpawn);
        selectTarget.onClick.AddListener(ButtonClickTarget);
        saveBackground.onClick.AddListener(ButtonClickSave);
        resetBackground.onClick.AddListener(ButtonOnClickReset);
        backToMenu.onClick.AddListener(ButtonOnClickMenu);
        loadSave.onValueChanged.AddListener(OnLoadSave);

        buttons = new Button[] { selectPen, selectEraser, selectSpawn, selectTarget };

        manager = this.GetComponent<DrawManager>();
    }

    private void ButtonClickPen()
    {
        ResetButtons();
        ColorBlock cb = selectPen.colors;
        cb.normalColor = Color.gray;
        selectPen.colors = cb;
        ApplyColor(selectPen);
        manager.State = DrawingState.PenTool;

        toolName.text = "Pen Brush";
        toolTips.text = "This tool can be used to draw onto the canvas.\n\nUsage: LeftClick to draw";
    }

    private void ButtonClickEraser()
    {
        ResetButtons();
        ColorBlock cb = selectEraser.colors;
        cb.normalColor = Color.gray;
        selectEraser.colors = cb;
        ApplyColor(selectEraser);
        manager.State = DrawingState.EraserTool;

        toolName.text = "Eraser Brush";
        toolTips.text = "This tool can be used to remove drawings from canvas.\n\nUsage: LeftClick to erase";
    }

    private void ButtonClickSpawn()
    {
        ResetButtons();
        ColorBlock cb = selectSpawn.colors;
        cb.normalColor = Color.gray;
        selectSpawn.colors = cb;
        ApplyColor(selectSpawn);
        manager.State = DrawingState.SpawnSelector;

        toolName.text = "Spawn Selector";
        toolTips.text = "This tool marks the spawn of the cars in the simulation.\n\nUsage: LeftClick to set/change position\n\nRightClick + MouseMovement to change spawn rotation";
    }

    private void ButtonClickTarget()
    {
        ResetButtons();
        ColorBlock cb = selectTarget.colors;
        cb.normalColor = Color.gray;
        selectTarget.colors = cb;
        ApplyColor(selectTarget);
        manager.State = DrawingState.TargetSelector;

        toolName.text = "Target Selector";
        toolTips.text = "This tool marks the target of the cars in the simulation.\n\nUsage: LeftClick to set/change position";
    }

    private void ButtonClickSave()
    {
        bool saved = manager.SaveBackground();

        if (!saved)
        {
            MessageText.text = "No spawn and/or target set. Please use the SpawnSelector and TargetSelector.";
            Invoke("ResetMessageText", 4);
        }
        else
        {
            MessageText.text = "Successfully saved!";
            Invoke("ResetMessageText", 4);

            if (IsSaveLoaded)
            {
                bool exists = File.Exists(@"save\save.carStream");
                if (exists)
                {
                    FileStream fstream = new FileStream(@"save\save.carStream", FileMode.Open);
                    GloabalData.NetworkData.Structure = fstream;
                }
            }
            else
            {
                GloabalData.NetworkData.Structure = null;
            }
        }
    }

    private void ButtonOnClickReset()
    {
        manager.ResetBackground();
    }

    private void ButtonOnClickMenu()
    {
        SceneManager.LoadScene(0);
    }

    private void OnLoadSave(bool isSaved)
    {
        IsSaveLoaded = isSaved;
    }

    private void ResetButtons()
    {
        foreach (var btn in buttons)
        {
            ColorBlock cb = btn.colors;
            cb.normalColor = Color.white;
            btn.colors = cb;
        }
    }

    private void ApplyColor(Button btn)
    {
        btn.enabled = false;
        btn.enabled = true;
    }

    private void ResetMessageText()
    {
        MessageText.text = string.Empty;
    }
}
