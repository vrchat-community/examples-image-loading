using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDK3.Image;
using VRC.SDK3.StringLoading;
using VRC.SDKBase;
using VRC.Udon.Common.Interfaces;

public class FrameDataLoader : UdonSharpBehaviour
{
    private VRCImageDownloader _imageDownloader;
    private IUdonEventReceiver udonEventReceiver;
    public VRCUrl[] rgbUrl;
    public VRCUrl stringUrl;
    public Renderer renderer;
    public Text field;
    public float photoDurationSeconds = 10f; 
    private int _loadedIndex = -1;

    public string[] captions = new string[0];
    
    void Start()
    {
        udonEventReceiver = (IUdonEventReceiver)this;
        _imageDownloader = new VRCImageDownloader();
        VRCStringDownloader.LoadUrl(stringUrl, udonEventReceiver);
        LoadNextRecursive();
    }

    public void LoadNextRecursive()
    {
        LoadNext();
        SendCustomEventDelayedSeconds(nameof(LoadNextRecursive), photoDurationSeconds);
    }
    
    private void LoadNext()
    {
        _loadedIndex = (int)(Networking.GetServerTimeInMilliseconds() / 1000f / photoDurationSeconds) % rgbUrl.Length;
        
        var rgbInfo = new TextureInfo();
        rgbInfo.WrapModeU = TextureWrapMode.Mirror;
        rgbInfo.WrapModeV = TextureWrapMode.Mirror;
        _imageDownloader.DownloadImage(rgbUrl[_loadedIndex], renderer.material, udonEventReceiver, rgbInfo);

        // Set caption if one is provided
        if (_loadedIndex < captions.Length)
        {
            field.text = captions[_loadedIndex];
        }
        else
        {
            field.text = "";
        }
    }

    public override void OnStringLoadSuccess(IVRCStringDownload result)
    {
        Debug.Log($"Loaded String, making Captions from {result.Result}");
        captions = result.Result.Split('\n');
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
