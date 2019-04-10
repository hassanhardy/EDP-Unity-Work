using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WebCamera : MonoBehaviour
{
  public InstantiateBoard MT;
  private int counter = 0;

  private Texture2D backgroundTexture;
  private ARKit.Camera capture;
  public ARKit.FeaturePoints fp = null;
  public ARKit.InitialFrame ip = null;
  public bool initialMatchDone = false;

  /*
  private bool camAvailable;
  private WebCamTexture webCam;
  private Texture defaultBackground;
  */

  public RawImage background;
  public AspectRatioFitter fit;
  public Canvas canvas;

  private void Start()
  {
    // defaultBackground = background.texture;
    // WebCamDevice[] devices = WebCamTexture.devices;

    /*
    if (devices.Length == 0)
    {
        Debug.Log("No camera detected");
        camAvailable = false;
        return;
    }

    for (int i = 0; i < devices.Length; i++)
    {
        if (devices[i].isFrontFacing) {
            webCam = new WebCamTexture(devices[i].name, Screen.width, Screen.height);
        }
    }

    if(webCam == null)
    {
        Debug.Log("Unable to find camera");
        return;
    }

    webCam.Play();
    background.texture = webCam;

    camAvailable = true;
    */

    ARKit.Frame frame;

    this.capture = new ARKit.Camera(0, new ARKit.Size(1280, 720));

    frame = this.capture.GetNextFrame();

    this.backgroundTexture = new Texture2D(frame.Width, frame.Height);
    background.texture = this.backgroundTexture; // set texture to webcam frames
    // referenced from https://answers.unity.com/questions/23891/resizing-an-object.html
    canvas.transform.localScale = new Vector3((float)frame.Width / (float)frame.Height, 1, 1); // fix resolution of plane

    if (System.IO.File.Exists("intrinsics.yml"))
    {
      this.ip = new ARKit.InitialFrame();
      this.ip.ReadFromFile();
    }
    else
    {
      this.ip = new ARKit.InitialFrame(this.capture, new ARKit.Size(4, 7), 30);
      this.ip.Start();
    }

    ARKit.FeaturePoints.ComputeAndSave("simpsons-orig.jpg", "Assets/keypoints.yml");
    this.fp = ARKit.FeaturePoints.ReadData("Assets/keypoints.yml");
    ARKit.Memory.Frame = Emgu.CV.CvInvoke.Imread("match.jpg");
  }


  private void Update()
  {
    /*
    if (!camAvailable)
        return;

    float ratio = (float)webCam.width / (float)webCam.height;

    fit.aspectRatio = ratio;

    float scaleY = webCam.videoVerticallyMirrored ? -1f : 1f;
    background.rectTransform.localScale = new Vector3(1f, scaleY, 1f);

    int orient = -webCam.videoRotationAngle;
    background.rectTransform.localEulerAngles = new Vector3(0, 0, orient);
    */
    //GameObject g = GameObject.FindGameObjectWithTag("MainTrigger");
    //MT = g.GetComponent<InstantiateBoard>();

    bool tracking = false, gotPose = false;

    if (ip != null && fp != null)
    {
      /*if (!ip.Homography.IsEmpty)
      {
        var frame = this.capture.GetNextFrame();

        if (!initialMatchDone)
        {
          fp.ComputeAndMatch();
          initialMatchDone = true;
        }
        else
          tracking = fp.TrackObject();

        if (fp.FindObject(!tracking))
        {
          if (fp.GetPose(ip.CameraMatrix, ip.DistortionCoefficients, out Emgu.CV.Mat rotations, out Emgu.CV.Mat translations))
          {
            frame = fp.DrawObjectBorder(true, ip.CameraMatrix, ip.DistortionCoefficients, rotations, translations);
          }
          else
            frame = fp.DrawObjectBorder();

          print("object found");
                    MT.objectFound = true;
                    counter = 0;
        }
                else {
                    print("object not found");
                    if(counter >= 30) {
                        MT.objectFound = false;
                    }
                    counter++;                    
                }*/

      ARKit.Frame frame;
      Emgu.CV.Mat r, t, H;
      Camera cam = Camera.main;

      // arbitrary switch for testing
      if (counter == 10)
        ARKit.Memory.Frame = Emgu.CV.CvInvoke.Imread("track.jpg");

      // arbitrary condition for testing
      if (counter < 10)
        this.fp.ComputeAndMatch();
      else
        tracking = this.fp.TrackObject();

      // compute border points
      MT.objectFound = this.fp.FindObject(!tracking);

      // obtain pose by solvePnP
      gotPose = this.fp.GetPose(this.ip.CameraMatrix, this.ip.DistortionCoefficients, out r, out t);

      // draw object border
      if (gotPose)
        frame = this.fp.DrawObjectBorder(true, this.ip.CameraMatrix, this.ip.DistortionCoefficients, r, t);
      else
        frame = this.fp.DrawObjectBorder();

      // load image to background texture
      this.backgroundTexture.LoadImage(frame.Image);

      
      if (fp.GetHomography(out H) && gotPose)
      {
        Emgu.CV.CvInvoke.Rodrigues(r, r);

        Emgu.CV.Matrix<double> rm = new Emgu.CV.Matrix<double>(3, 3);
        Emgu.CV.Matrix<double> tm = new Emgu.CV.Matrix<double>(3, 1);
        for (int i = 0; i < 3; i++)
        {
          for (int j = 0; j < 3; j++)
            rm[i, j] = ARKit.MatExtension.GetValue(r, i, j);
          tm[i, 0] = ARKit.MatExtension.GetValue(t, i, 0);
        }

        rm = rm.Transpose();
        // tm = -1 * rm * tm;

        Vector3 position = new Vector3()
        {
          x = (float)(tm[0, 0] + ARKit.MatExtension.GetValue(this.ip.CameraMatrix, 0, 2)),
          y = (float)(-tm[1, 0] + ARKit.MatExtension.GetValue(this.ip.CameraMatrix, 1, 2)),
          z = (float)tm[2, 0]
        };
        
        Vector4 rQ = ConvertRotationMatToQuaternion(rm);

        cam.transform.position = -1 * position;
        cam.transform.rotation = new Quaternion(rQ.x, -rQ.y, rQ.z, rQ.w);

        // debugging section
        Matrix4x4 hmat = new Matrix4x4()
        {
          m00 = ARKit.MatExtension.GetValue(H, 0, 0),
          m01 = ARKit.MatExtension.GetValue(H, 0, 1),
          m02 = ARKit.MatExtension.GetValue(H, 0, 2),
          m03 = 0,
          m10 = ARKit.MatExtension.GetValue(H, 1, 0),
          m11 = ARKit.MatExtension.GetValue(H, 1, 1),
          m12 = ARKit.MatExtension.GetValue(H, 1, 2),
          m13 = 0,
          m20 = ARKit.MatExtension.GetValue(H, 2, 0),
          m21 = ARKit.MatExtension.GetValue(H, 2, 1),
          m22 = ARKit.MatExtension.GetValue(H, 2, 2),
          m23 = 0,
          m30 = 0,
          m31 = 0,
          m32 = 0,
          m33 = 1,
        };
        Matrix4x4 kmat = new Matrix4x4()
        {
          m00 = (float)ARKit.MatExtension.GetValue(this.ip.CameraMatrix, 0, 0),
          m01 = (float)ARKit.MatExtension.GetValue(this.ip.CameraMatrix, 0, 1),
          m02 = (float)ARKit.MatExtension.GetValue(this.ip.CameraMatrix, 0, 2),
          m03 = (float)0,
          m10 = (float)ARKit.MatExtension.GetValue(this.ip.CameraMatrix, 1, 0),
          m11 = (float)ARKit.MatExtension.GetValue(this.ip.CameraMatrix, 1, 1),
          m12 = (float)ARKit.MatExtension.GetValue(this.ip.CameraMatrix, 1, 2),
          m13 = (float)0,
          m20 = (float)ARKit.MatExtension.GetValue(this.ip.CameraMatrix, 2, 0),
          m21 = (float)ARKit.MatExtension.GetValue(this.ip.CameraMatrix, 2, 1),
          m22 = (float)ARKit.MatExtension.GetValue(this.ip.CameraMatrix, 2, 2),
          m23 = (float)0,
          m30 = (float)0,
          m31 = (float)0,
          m32 = (float)0,
          m33 = (float)0,
        };
        Matrix4x4 emat = new Matrix4x4()
        {
          m00 = (float)rm[0, 0],
          m01 = (float)rm[0, 1],
          m02 = (float)rm[0, 2],
          m03 = (float)tm[0, 0],
          m10 = (float)rm[1, 0],
          m11 = (float)rm[1, 1],
          m12 = (float)rm[1, 2],
          m13 = (float)tm[1, 0],
          m20 = (float)rm[2, 0],
          m21 = (float)rm[2, 1],
          m22 = (float)rm[2, 2],
          m23 = (float)tm[2, 0],
          m30 = 0,
          m31 = 0,
          m32 = 0,
          m33 = 0,
        };

        print(hmat);
        print(kmat);
        print(emat);
        print("euler angles " + cam.transform.eulerAngles.x + " " + cam.transform.eulerAngles.y + " " + cam.transform.eulerAngles.z);

        // arbitrary condition update
        counter++;
      }
    }
  }

  private Vector4 ConvertRotationMatToQuaternion(Emgu.CV.Matrix<double> rotationMat)
  {
    float T, S, X, Y, Z, W;

    Matrix4x4 rotation = new Matrix4x4()
    {
      m00 = (float)rotationMat[0, 0],
      m01 = (float)rotationMat[0, 1],
      m02 = (float)rotationMat[0, 2],
      m03 = 0,
      m10 = (float)rotationMat[1, 0],
      m11 = (float)rotationMat[1, 1],
      m12 = (float)rotationMat[1, 2],
      m13 = 0,
      m20 = (float)rotationMat[2, 0],
      m21 = (float)rotationMat[2, 1],
      m22 = (float)rotationMat[2, 2],
      m23 = 0,
      m30 = 0,
      m31 = 0,
      m32 = 0,
      m33 = 0,
    };

    T = 1 + rotation[0] + rotation[5] + rotation[10];

    if (T > 0.00000001)
    {
      S = Mathf.Sqrt(T) * 2;
      X = (rotation[9] - rotation[6]) / S;
      Y = (rotation[2] - rotation[8]) / S;
      Z = (rotation[4] - rotation[1]) / S;
      W = 0.25f * S;
    }
    else
    {
      if (rotation[0] > rotation[5] && rotation[0] > rotation[10])
      { // Column 0: 
        S = Mathf.Sqrt((float)(1.0 + rotation[0] - rotation[5] - rotation[10])) * 2;
        X = 0.25f * S;
        Y = (rotation[4] + rotation[1]) / S;
        Z = (rotation[2] + rotation[8]) / S;
        W = (rotation[9] - rotation[6]) / S;

      }
      else if (rotation[5] > rotation[10])
      { // Column 1: 
        S = Mathf.Sqrt((float)(1.0 + rotation[5] - rotation[0] - rotation[10])) * 2;
        X = (rotation[4] + rotation[1]) / S;
        Y = 0.25f * S;
        Z = (rotation[9] + rotation[6]) / S;
        W = (rotation[2] - rotation[8]) / S;

      }
      else
      { // Column 2:
        S = Mathf.Sqrt((float)(1.0 + rotation[10] - rotation[0] - rotation[5])) * 2;
        X = (rotation[2] + rotation[8]) / S;
        Y = (rotation[9] + rotation[6]) / S;
        Z = 0.25f * S;
        W = (rotation[4] - rotation[1]) / S;
      }
    }

    print("T " + T + " S " + S + " X " + X + " Y " + Y + " Z " + Z + " W " + W);

    return new Vector4(X, Y, Z, W);
  }
}
