using UnityEngine;
using UnityEngine.UI;

public class SpriteToggle : MonoBehaviour
{
    [SerializeField] private Image targetImage;
    [SerializeField] private Sprite spriteON;
    [SerializeField] private Sprite spriteOFF;

    private bool _isOn = false;

    public void OnButtonPressed() // แก้ตรงนี้อย่างเดียว
    {
        _isOn = !_isOn;
        targetImage.sprite = _isOn ? spriteON : spriteOFF;
    }
}