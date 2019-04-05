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

      ARKit.Frame frame;

      this.fp.ComputeAndMatch();
      MT.objectFound = this.fp.FindObject();
      if (this.fp.GetPose(this.ip.CameraMatrix, this.ip.DistortionCoefficients, out Emgu.CV.Mat r, out Emgu.CV.Mat t))
        frame = this.fp.DrawObjectBorder(true, this.ip.CameraMatrix, this.ip.DistortionCoefficients, r, t);
      else
        frame = this.fp.DrawObjectBorder();
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
        this.fp.GetPose(this.ip.CameraMatrix, this.ip.DistortionCoefficients, out Emgu.CV.Mat rotationMat, out Emgu.CV.Mat translationVector);

        Vector3 position = new Vector3()
        {
          x = ARKit.MatExtension.GetValue(translationVector, 0, 0),
          y = ARKit.MatExtension.GetValue(translationVector, 1, 0),
          z = -ARKit.MatExtension.GetValue(translationVector, 2, 0)
        };

        Emgu.CV.CvInvoke.Rodrigues(rotationMat, rotationMat);

        Matrix4x4 rotation = new Matrix4x4()
        {
          m00 = -ARKit.MatExtension.GetValue(rotationMat, 0, 0),
          m01 = -ARKit.MatExtension.GetValue(rotationMat, 1, 0),
          m02 = -ARKit.MatExtension.GetValue(rotationMat, 2, 0),
          m03 = 0,
          m10 = -ARKit.MatExtension.GetValue(rotationMat, 0, 1),
          m11 = -ARKit.MatExtension.GetValue(rotationMat, 1, 1),
          m12 = -ARKit.MatExtension.GetValue(rotationMat, 2, 1),
          m13 = 0,
          m20 = ARKit.MatExtension.GetValue(rotationMat, 0, 2),
          m21 = ARKit.MatExtension.GetValue(rotationMat, 1, 2),
          m22 = ARKit.MatExtension.GetValue(rotationMat, 2, 2),
          m23 = 0,
          m30 = 0,
          m31 = 0,
          m32 = 0,
          m33 = 1,
        };

        float T, S, X, Y, Z, W;

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
          {     // Column 1: 
            S = Mathf.Sqrt((float)(1.0 + rotation[5] - rotation[0] - rotation[10])) * 2;
            X = (rotation[4] + rotation[1]) / S;
            Y = 0.25f * S;
            Z = (rotation[9] + rotation[6]) / S;
            W = (rotation[2] - rotation[8]) / S;

          }
          else
          {           // Column 2:
            S = Mathf.Sqrt((float)(1.0 + rotation[10] - rotation[0] - rotation[5])) * 2;
            X = (rotation[2] + rotation[8]) / S;
            Y = (rotation[9] + rotation[6]) / S;
            Z = 0.25f * S;
            W = (rotation[4] - rotation[1]) / S;
          }
        }


        cam.transform.position = -1 * position;
        cam.transform.rotation = new Quaternion(X, Y, Z, W);
        print(rotation);
        print("T " + T + "S " + S + "X " + X + " Y " + Y + " Z " + Z + " W " + W);

        //Emgu.CV.Matrix<double> proj = this.fp.projection_mat(H_mat, cam_mat);
        //proj[0, 3] /= 100000;
        //proj[1, 3] /= -100000;
        //proj[2, 3] /= -100000;

        //this.fp.GetProjectionMatrix(this.ip.CameraMatrix, this.ip.DistortionCoefficients, out Emgu.CV.Mat projectionMat);

        //Matrix4x4 proj_mat = Matrix4x4.identity;

        /*for (int i = 0; i < 4; i++)
        {
          for (int j = 0; j < 4; j++)
          {
            proj_mat[i, j] = (float)proj[i, j];
            //proj_mat[i, j] = (float)ARKit.MatExtension.GetValue(projectionMat, i, j);
          }
        }*/

        //cam.worldToCameraMatrix = proj_mat;
        //print("projection matrix set");
        //print(cam.fieldOfView.ToString());
        //print("world to camera matrix " + cam.worldToCameraMatrix.rotation.ToString());
        //print("world to camera matrix " + cam.worldToCameraMatrix.ToString());
        //print("projection matrix " + cam.projectionMatrix.rotation.ToString());
        // print("projection matrix " + cam.projectionMatrix.ToString());
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
