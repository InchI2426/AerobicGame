using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class WebcamDisplay : MonoBehaviour
{
    public RawImage display;
    public int targetFPS = 20;

    private WebCamTexture webcamTexture;
    private float interval;
    private float lastTime;
    private float lastLogTime;

    void Start()
    {
        if (display == null) { Debug.LogError("display is not assigned"); return; }

        interval = 1f / targetFPS;
        webcamTexture = new WebCamTexture(1280, 720);
        display.texture = webcamTexture;
        display.rectTransform.localScale = new Vector3(-1, 1, 1);
        webcamTexture.Play();

        SceneManager.sceneLoaded += (scene, mode) =>
        {
            Debug.Log("[WebcamDebug] scene loaded: " + scene.name
                + " isPlaying=" + webcamTexture.isPlaying
                + " width=" + webcamTexture.width
                + " height=" + webcamTexture.height);
        };
    }

    void Update()
    {
        if (Time.time - lastLogTime > 2f)
        {
            lastLogTime = Time.time;
            Debug.Log("[WebcamDebug] tick"
                + " isPlaying=" + (webcamTexture != null && webcamTexture.isPlaying)
                + " didUpdateThisFrame=" + (webcamTexture != null && webcamTexture.didUpdateThisFrame)
                + " width=" + (webcamTexture != null ? webcamTexture.width : -1)
                + " gameObjectActive=" + gameObject.activeInHierarchy);
        }

        if (webcamTexture == null) return;

        if (!webcamTexture.isPlaying)
        {
            Debug.LogWarning("[WebcamDebug] webcamTexture stopped unexpectedly, restarting Play()");
            webcamTexture.Play();
            return;
        }

        if (!webcamTexture.didUpdateThisFrame) return;

        if (Time.time - lastTime < interval) return;
        lastTime = Time.time;

        display.texture = webcamTexture;

        AspectRatioFitter fitter = display.GetComponent<AspectRatioFitter>();
        if (fitter != null && webcamTexture.width > 16)
            fitter.aspectRatio = (float)webcamTexture.width / webcamTexture.height;
    }

    void OnDestroy()
    {
        if (webcamTexture != null) webcamTexture.Stop();
    }
}