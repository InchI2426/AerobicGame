using UnityEngine;
using UnityEngine.UI;

public class WebcamDisplay : MonoBehaviour
{
    public RawImage display;

    private WebCamTexture webcamTexture;

    void Start()
    {
        // Request 1280x720 ให้ตรงกับ Python
        webcamTexture = new WebCamTexture(1280, 720);
        display.texture = webcamTexture;
        display.rectTransform.localScale = new Vector3(-1, 1, 1);
        webcamTexture.Play();
    }

    void Update()
    {
        // รอให้กล้องเริ่มแล้วค่อย adjust aspect ratio
        if (webcamTexture.width > 16)
        {
            float ratio = (float)webcamTexture.width / webcamTexture.height;
            AspectRatioFitter fitter = display.GetComponent<AspectRatioFitter>();
            if (fitter != null)
                fitter.aspectRatio = ratio;
        }
    }

    void OnDestroy()
    {
        if (webcamTexture != null)
            webcamTexture.Stop();
    }
}