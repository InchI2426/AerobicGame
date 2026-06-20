using UnityEngine;
using UnityEngine.UI;

public class WebcamDisplay : MonoBehaviour
{
    public RawImage display;
    public int targetFPS = 20;
    private WebCamTexture webcamTexture;

    void Start()
    {
        if (display == null)
        {
            Debug.LogError("display is not assigned");
            return;
        }

        // ใส่ targetFPS เป็น parameter ที่ 3
        webcamTexture = new WebCamTexture(1280, 720, targetFPS);
        display.texture = webcamTexture;
        display.rectTransform.localScale = new Vector3(-1, 1, 1);
        webcamTexture.Play();

        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = targetFPS;
        Debug.Log("Camera FPS requested: " + targetFPS + " | Application.targetFrameRate set: " + Application.targetFrameRate);
    }

    void Update()
    {
        if (webcamTexture == null || !webcamTexture.isPlaying) return;

        if (webcamTexture.width > 16)
        {
            AspectRatioFitter fitter = display.GetComponent<AspectRatioFitter>();
            if (fitter != null)
                fitter.aspectRatio = (float)webcamTexture.width / webcamTexture.height;
        }
    }

    void OnDestroy()
    {
        if (webcamTexture != null) webcamTexture.Stop();
    }
}