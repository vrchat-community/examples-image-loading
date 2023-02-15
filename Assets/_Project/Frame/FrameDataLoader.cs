using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDK3.Image;
using VRC.SDK3.StringLoading;
using VRC.SDKBase;
using VRC.Udon;

public class FrameDataLoader : UdonSharpBehaviour
{
    private VRCImageDownloader _imageDownloader;
    public UdonBehaviour udonEventReceiver;
    public VRCUrl[] rgbUrl;
    public VRCUrl[] stringUrl;
    public Renderer renderer;
    public Text field;
    public float photoDurationSeconds = 10f; 
    private int _loadedIndex = -1;
    
    void Start()
    {
        _imageDownloader = new VRCImageDownloader();
        LoadNextRecursive();
    }

    public void LoadNextRecursive()
    {
        LoadNext();
        SendCustomEventDelayedSeconds(nameof(LoadNextRecursive), photoDurationSeconds);
    }
    
    private void LoadNext()
    {
        // _loadedIndex++;
        // if(_loadedIndex >= rgbUrl.Length)
        // {
        //     _loadedIndex = 0;
        // }
        _loadedIndex = (int)(Networking.GetServerTimeInMilliseconds() / 1000f / photoDurationSeconds) % rgbUrl.Length;
        
        var rgbInfo = new TextureInfo();
        rgbInfo.WrapModeU = TextureWrapMode.Mirror;
        rgbInfo.WrapModeV = TextureWrapMode.Mirror;
        _imageDownloader.DownloadImage(rgbUrl[_loadedIndex], renderer.material, udonEventReceiver, rgbInfo);
        VRCStringDownloader.LoadUrl(stringUrl[_loadedIndex], udonEventReceiver);
    }

    public override void OnStringLoadSuccess(IVRCStringDownload result)
    {
        field.text = result.Result;
    }

    public override void OnStringLoadError(IVRCStringDownload result)
    {
        Debug.LogError($"Could not load string {result.Error}");
    }

    public override void OnImageLoadSuccess(IVRCImageDownload result)
    {
        Debug.Log($"Image loaded: {result.SizeInMemoryBytes} bytes.");
    }

    public override void OnImageLoadError(IVRCImageDownload result)
    {
        Debug.Log($"Image not loaded: {result.Error.ToString()}: {result.ErrorMessage}.");
    }

    private void OnDestroy()
    {
        _imageDownloader.Dispose();
    }
}
