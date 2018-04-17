using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Source : MonoBehaviour 
{
    public enum Mode
    {
        Manual,
        Automatic,
        None
    };

    public float output { get; private set; }
    public Vector2 position { get; private set; }
    public Vector2 force { get; private set; }
    public Color color { get; private set; }

    [SerializeField] public Mode mode;
    [SerializeField] float speed;
    [SerializeField, Range(0, 1)] float colorValue;
    [SerializeField, Range(0, 1)] float colorSaturation;

    Vector2 prevPosition;

    void OnEnable()
    {
        prevPosition = Vector2.one * 0.5f; 
    }

    void Update()
    {
        // update position
        Vector2 p = prevPosition;
        bool addSource = true;
        switch (mode)
        {
            case Mode.Automatic:
                addSource = AutoSourceUpdate(out p);
                break;
            case Mode.Manual:
                addSource = ManualSouceUpdate(out p);
                break;
            default:
                addSource = false;
                break;
        }
        force = p - prevPosition;
        prevPosition = position = p;
        output = addSource ? 1 : 0;
        // update color
        var hue = Mathf.PerlinNoise(0, Time.time * speed);
        color = Color.HSVToRGB(hue, colorSaturation, colorValue);
    }

    bool ManualSouceUpdate(out Vector2 pos)
    {
        var position_ = Input.mousePosition;
        position_.x /= (float)Screen.width;
        position_.y /= (float)Screen.height;
        pos = position_;
        return Input.GetMouseButton(0);
    }

    bool AutoSourceUpdate(out Vector2 pos)
    {
        pos = new Vector2(Mathf.PerlinNoise(0, Time.time * speed), Mathf.PerlinNoise(1, Time.time * speed));
        return Mathf.PerlinNoise(2, Time.time) > 0.1f;
    }
}
