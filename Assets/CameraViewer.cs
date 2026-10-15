using UnityEngine;
using UnityEngine.UI;


public class CameraViewer : MonoBehaviour
{
    [Header("UI References")]
    public RawImage cameraDisplay;

    [Header("AI Model")]
    public Unity.InferenceEngine.ModelAsset modelAsset; // Kéo file best.onnx vào đây khi có

    private Unity.InferenceEngine.Model runtimeModel;
    private Unity.InferenceEngine.Worker worker;
    private WebCamTexture webCamTexture;

    void Start()
    {
        // 1. Bật Camera thiết bị
        if (WebCamTexture.devices.Length > 0)
        {
            webCamTexture = new WebCamTexture();
            cameraDisplay.texture = webCamTexture;
            webCamTexture.Play();
        }

        // 2. Nạp AI nếu đã có file ONNX
        if (modelAsset != null)
        {
            runtimeModel = Unity.InferenceEngine.ModelLoader.Load(modelAsset);
            worker = new Unity.InferenceEngine.Worker(runtimeModel, Unity.InferenceEngine.BackendType.GPUCompute);
            Debug.Log("Đã nạp mô hình AI thành công!");
        }
    }

    void Update()
    {
        if (webCamTexture == null || !webCamTexture.isPlaying || webCamTexture.width <= 16) return;

        // 3. Phân tích ảnh qua Camera nếu đã cài file ONNX
        if (worker != null)
        {
            // Đã đổi TensorFloat thành Tensor<float> đúng chuẩn Sentis mới
            using Unity.InferenceEngine.Tensor<float> inputTensor = Unity.InferenceEngine.TextureConverter.ToTensor(webCamTexture, 640, 640, 3);
            worker.Schedule(inputTensor);

            Unity.InferenceEngine.Tensor<float> outputTensor = worker.PeekOutput() as Unity.InferenceEngine.Tensor<float>;
        }
    }

    void OnDisable()
    {
        worker?.Dispose();
        if (webCamTexture != null && webCamTexture.isPlaying)
        {
            webCamTexture.Stop();
        }
    }
}