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

    bool tracking = false;

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

      this.fp.ComputeAndMatch();
      MT.objectFound = this.fp.FindObject();
      ARKit.Frame frame = this.fp.DrawObjectBorder();
      this.backgroundTexture.LoadImage(frame.Image);

      Camera cam = Camera.main;
      if (fp.GetHomography(out Emgu.CV.Mat H))
      {
        Emgu.CV.Matrix<double> H_mat = new Emgu.CV.Matrix<double>(3, 3);
        Emgu.CV.Matrix<double> cam_mat = new Emgu.CV.Matrix<double>(3, 3);
        for (int i = 0; i < 3; i++)
        {
          for (int j = 0; j < 3; j++)
          {
            H_mat[i, j] = ARKit.MatExtension.GetValue(H, i, j);
            cam_mat[i, j] = ARKit.MatExtension.GetValue(ip.CameraMatrix, i, j);
          }
        }
        Emgu.CV.Matrix<double> proj = this.fp.projection_mat(H_mat, cam_mat);

        Matrix4x4 proj_mat = Matrix4x4.identity;

        for (int i = 0; i < 4; i++)
        {
          for (int j = 0; j < 4; j++)
          {
            proj_mat[i, j] = (float)proj[i, j];
          }
        }

        cam.projectionMatrix =  proj_mat;
        print("projection matrix set");
        //print(cam.fieldOfView.ToString());
        print(cam.projectionMatrix.rotation.ToString());
        print(cam.projectionMatrix.ToString());
        //print("top " + cam.projectionMatrix.decomposeProjection.top + " bottom " + cam.projectionMatrix.decomposeProjection.bottom
        //  + " right " + cam.projectionMatrix.decomposeProjection.right + " left " + cam.projectionMatrix.decomposeProjection.left
        //  + " znear " + cam.projectionMatrix.decomposeProjection.zNear + " zfar " + cam.projectionMatrix.decomposeProjection.zFar);
      }

      //ARKit.Memory.Frame = Emgu.CV.CvInvoke.Imread("track.jpg");

      //this.fp.TrackObject();
      //MT.objectFound = this.fp.FindObject(false);
      //frame = this.fp.DrawObjectBorder();
      //this.backgroundTexture.LoadImage(frame.Image);

      //if (fp.GetHomography(out H))
      //{
      //  Emgu.CV.Matrix<double> H_mat = new Emgu.CV.Matrix<double>(3, 3);
      //  Emgu.CV.Matrix<double> cam_mat = new Emgu.CV.Matrix<double>(3, 3);
      //  for (int i = 0; i < 3; i++)
      //  {
      //    for (int j = 0; j < 3; j++)
      //    {
      //      double val = ARKit.MatExtension.GetValue(H, i, j);
      //      print("i: " + i.ToString());
      //      print("j: " + j.ToString());
      //      H_mat[i, j] = val;

      //      val = ARKit.MatExtension.GetValue(ip.CameraMatrix, i, j);
      //      cam_mat[i, j] = val;
      //    }
      //  }
      //  Emgu.CV.Matrix<double> proj = this.fp.projection_mat(H_mat, cam_mat);


      //  Matrix4x4 proj_mat = Matrix4x4.identity;


      //  for (int i = 0; i < 4; i++)
      //  {
      //    for (int j = 0; j < 4; j++)
      //    {
      //      double val = proj[i, j];
      //      int val_proj = (int)val;

      //      proj_mat[i, j] = val_proj;
      //    }
      //  }

      //  cam.projectionMatrix = proj_mat;
      //  print("projection matrix set");

      //}

    }
  }
}
