using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDK3.Image;
using VRC.SDK3.StringLoading;
using VRC.SDKBase;
using VRC.Udon.Common.Interfaces;

public class FrameDataLoader : UdonSharpBehaviour
{
   
    public VRCUrl[] rgbUrl;
    public VRCUrl stringUrl;
    public new Renderer renderer;
    public Text field;
    public float photoDurationSeconds = 10f;
    
    // Private Variables
    private int _loadedIndex = -1;
    private VRCImageDownloader _imageDownloader;
    private IUdonEventReceiver udonEventReceiver;
    private string[] captions = new string[0];
    private bool[] downloadsComplete;
    private Texture2D[] _textures;
    
    void Start()
    {
        // Cast self to the type needed for Image and String Loading methods
        udonEventReceiver = (IUdonEventReceiver)this;
        
        // Track which downloads have been completed already
        downloadsComplete = new bool[rgbUrl.Length];
        _textures = new Texture2D[rgbUrl.Length];
        
        // Construct Image Downloader to reuse
        _imageDownloader = new VRCImageDownloader();
        
        // Load Captions
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

        if (downloadsComplete[_loadedIndex])
        {
            renderer.sharedMaterial.mainTexture = _textures[_loadedIndex];
        }
        else
        {
            var rgbInfo = new TextureInfo();
            rgbInfo.WrapModeU = TextureWrapMode.Mirror;
            rgbInfo.WrapModeV = TextureWrapMode.Mirror;
            _imageDownloader.DownloadImage(rgbUrl[_loadedIndex], renderer.material, udonEventReceiver, rgbInfo);
        }

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
        captions = result.Result.Split('\n');
    }

    public override void OnStringLoadError(IVRCStringDownload result)
    {
        Debug.LogError($"Could not load string {result.Error}");
    }

    public override void OnImageLoadSuccess(IVRCImageDownload result)
    {
        Debug.Log($"Image loaded: {result.SizeInMemoryBytes} bytes.");
        
        downloadsComplete[_loadedIndex] = true;
        _textures[_loadedIndex] = result.Result;
    }

    public override void OnImageLoadError(IVRCImageDownload result)
    {
        Debug.Log($"Image not loaded: {result.Error.ToString()}: {result.ErrorMessage}.");
        downloadsComplete[_loadedIndex] = false;
    }

    private void OnDestroy()
    {
        _imageDownloader.Dispose();
    }
}
