using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DrawingState
{
    PenTool,
    EraserTool,
    SpawnSelector,
    TargetSelector
}

public class DrawManager : MonoBehaviour
{
    public Texture2D penBrush;
    public Texture2D eraserBrush;

    public GameObject spawnIconObject;
    public GameObject targetIconObject;

    public GameObject backgroundObject;
    public Texture2D terrainImage;

    public DrawingState State { get => state; set => ChangeState(value); }

    private Sprite background;
    private BoxCollider backgroundCollider;

    private Texture2D activeBrush;
    private Color[] brushPixels;
    private Vector2 prevSpritePos = new Vector2(-1, -1);

    private Vector2 spawnPosition = new Vector2(-1, -1);
    private float spawnRotation = 0;
    private Vector2 targetPosition = new Vector2(-1, -1);

    private DrawingState state;

    private void Start()
    {
        State = DrawingState.PenTool;

        background = backgroundObject.GetComponent<SpriteRenderer>().sprite;
        backgroundCollider = backgroundObject.GetComponent<BoxCollider>();

        ResetBackground();

        ChangeTool(state);
    }

    // Update is called once per frame
    private void Update()
    {
        if (state == DrawingState.PenTool || state == DrawingState.EraserTool)
        {
            Draw();
        }
        else if (state == DrawingState.SpawnSelector)
        {
            if (Input.GetMouseButton(0))
            {
                SetObjectPosition(spawnIconObject);
                Vector2 pos = WorldToPixel(backgroundCollider.size, background.texture, spawnIconObject.transform.position);
                pos = new Vector2(pos.x, background.texture.height - pos.y);
                spawnPosition = pos;
            }
            else if (Input.GetMouseButton(1))
            {
                SetObjectRotation(spawnIconObject, Vector3.forward, 100);
                spawnRotation = spawnIconObject.transform.rotation.eulerAngles.z * -1;
            }

        }
        else if (state == DrawingState.TargetSelector && Input.GetMouseButton(0))
        {
            SetObjectPosition(targetIconObject);
            Vector2 pos = WorldToPixel(backgroundCollider.size, background.texture, targetIconObject.transform.position);
            pos = new Vector2(pos.x, background.texture.height - pos.y);
            targetPosition = pos;
        }
    }

    public bool SaveBackground()
    {
        if (spawnPosition != new Vector2(-1, -1) && targetPosition != new Vector2(-1, -1))
        {
            GloabalData.TrackData.TerrainImage = background.texture;

            GloabalData.TrackData.SpawnPosition = spawnPosition;
            GloabalData.TrackData.SpawnRotation = spawnRotation;
            GloabalData.TrackData.TargetPosition = targetPosition;

            Debug.Log(spawnPosition);
            Debug.Log(spawnRotation);
            Debug.Log(targetPosition);

            return true;
        }

        return false;
    }

    public void ResetBackground()
    {
        Color[] colors = new Color[background.texture.width * background.texture.height];
        for (int i = 0; i < colors.Length; i++)
        {
            colors[i] = Color.black;
        }
        background.texture.SetPixels(colors);
        background.texture.Apply();
    }

    private void Draw()
    {
        if (Input.GetMouseButton(0))
        {
            Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            Physics.Raycast(mouseRay, out hit);

            if (hit.collider != null)
            {
                if (hit.collider.gameObject.GetInstanceID() == backgroundObject.GetInstanceID())
                {
                    Vector3 colliderSize = backgroundCollider.size;
                    Vector2 spritePos = WorldToPixel(colliderSize, background.texture, hit.point);
                    spritePos = new Vector2(spritePos.x - activeBrush.width / 2, spritePos.y - activeBrush.height / 2); // center of brush is at mouse position
                    spritePos = new Vector2(Mathf.Clamp(spritePos.x, activeBrush.width / 2, background.texture.width - activeBrush.width * 3F/2F),
                        Mathf.Clamp(spritePos.y, activeBrush.height / 2, background.texture.height - activeBrush.height * 3F / 2F)); // set bounds

                    Vector2 dist = Vector2.zero;
                    float step = 1;
                    if (prevSpritePos != new Vector2(-1, -1))
                    {
                        dist = spritePos - prevSpritePos;
                        if (dist.magnitude > activeBrush.width / 4F) // check if the distance the mouse travelled is longer than 1/4 of the brush width
                        {
                            float amount = dist.magnitude / (activeBrush.width / 4F); // how many times longer
                            step = 1 / amount;
                        }
                        else if (dist.magnitude < activeBrush.width / 4F)
                        {
                            step = 2; // stop drawing if mouse didn't move far enough
                        }
                    }
                    else
                    {
                        prevSpritePos = spritePos;
                    }

                    for (float iter = step; iter <= 1; iter += step)
                    {
                        spritePos = prevSpritePos + iter * dist; // gradually add the distance to the prev pos

                        Color[] pixels = background.texture.GetPixels((int)spritePos.x, (int)spritePos.y, activeBrush.width, activeBrush.height);
                        for (int i = 0; i < pixels.Length; i++)
                        {
                            pixels[i] = brushPixels[i] * brushPixels[i].a + pixels[i] * (1 - brushPixels[i].a);
                            pixels[i].a = 1;
                        }
                        background.texture.SetPixels((int)spritePos.x, (int)spritePos.y, activeBrush.width, activeBrush.height, pixels);
                    }
                    
                    if (step != 2)
                    {
                        background.texture.Apply();
                        prevSpritePos = spritePos;
                    }

                }
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            prevSpritePos = new Vector2(-1, -1);
        }
    }

    private void SetObjectPosition(GameObject obj)
    {
        Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        Physics.Raycast(mouseRay, out hit);

        if (hit.collider != null)
        {
            if (hit.collider.gameObject.GetInstanceID() == backgroundObject.GetInstanceID())
            {
                obj.transform.position = new Vector3(hit.point.x, hit.point.y, obj.transform.position.z);
            }
        }
    }

    private void SetObjectRotation(GameObject obj, Vector3 axis, float scale)
    {
        Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        Physics.Raycast(mouseRay, out hit);

        if (hit.collider != null)
        {
            if (hit.collider.gameObject.GetInstanceID() == backgroundObject.GetInstanceID())
            {
                Vector3 dist = (hit.point - obj.transform.position);
                float angle = VectorToAngle(new Vector2(dist.x, dist.y)) / Mathf.Deg2Rad - 90;
                obj.transform.rotation = Quaternion.Euler(angle * axis.x, angle * axis.y, angle * axis.z);
            }
        }
    }

    private void ChangeTool(DrawingState st)
    {
        if (st == DrawingState.PenTool)
        {
            activeBrush = penBrush;
            brushPixels = activeBrush.GetPixels();
        }
        else if (st == DrawingState.EraserTool)
        {
            activeBrush = eraserBrush;
            brushPixels = activeBrush.GetPixels();
        }
        else if (st == DrawingState.SpawnSelector)
        {

        }
        else if (st == DrawingState.TargetSelector)
        {

        }
    }

    private void ChangeState(DrawingState val)
    {
        state = val;
        ChangeTool(state);
    }

    private float VectorToAngle(Vector2 vector)
    {
        return (float)Mathf.Atan2(vector.x, -vector.y);
    }

    private Vector2 WorldToPixel(Vector3 colliderSize, Texture2D targetTexture, Vector3 point)
    {
        Vector2 pos = new Vector2((point.x + colliderSize.x / 2) / colliderSize.x * targetTexture.width,
        (point.y + colliderSize.y / 2) / colliderSize.y * targetTexture.height);

        return pos;
    }
}
