using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetWebcamBG : MonoBehaviour
{
    void Start()
    {
        GUITexture BackgroundTexture = gameObject.AddComponent<GUITexture>();
        BackgroundTexture.pixelInset = new Rect(0, 0, Screen.width, Screen.height);
        WebCamDevice[] devices = WebCamTexture.devices;
        string backCamName = devices[0].name;
        WebCamTexture CameraTexture = new WebCamTexture(backCamName, 10000, 10000, 30);
        CameraTexture.Play();
        BackgroundTexture.texture = CameraTexture;
    }
}

