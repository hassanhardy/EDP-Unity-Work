using System.Collections;
using System.Collections.Generic;
using UnityEngine;

  public class MoveTrigger : MonoBehaviour
{
  public float moveSpeed = 10f;
  public float turnSpeed = 50f;


  void Update()
  {
    /*if (Input.GetKey(KeyCode.Alpha1))
        transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);

    if (Input.GetKey(KeyCode.Alpha2))
        transform.Translate(-Vector3.forward * moveSpeed * Time.deltaTime);

    if (Input.GetKey(KeyCode.Alpha3))
        transform.Rotate(Vector3.up, -turnSpeed * Time.deltaTime);

    if (Input.GetKey(KeyCode.Alpha4))
        transform.Rotate(Vector3.up, turnSpeed * Time.deltaTime);

    if (Input.GetKey(KeyCode.Alpha5))
        transform.Rotate(Vector3.right, turnSpeed * Time.deltaTime);

    if (Input.GetKey(KeyCode.Alpha6))
        transform.Rotate(Vector3.right, -turnSpeed * Time.deltaTime);

    if (Input.GetKey(KeyCode.Alpha7))
        transform.Translate(Vector3.up * moveSpeed * Time.deltaTime);

    if (Input.GetKey(KeyCode.Alpha8))
        transform.Translate(-Vector3.up * moveSpeed * Time.deltaTime);*/

    if (WebCamera.fp != null && WebCamera.ip != null)
    {
      print("camera is calibrated " + WebCamera.ip.IsCalibrated);
      print("feature point inlier ratio " + WebCamera.fp.InlierRatio);
      WebCamera.fp.GetPose(WebCamera.ip.CameraMatrix, WebCamera.ip.DistortionCoefficients, out Emgu.CV.Mat rotationMat, out Emgu.CV.Mat translationVector);

      if (!rotationMat.IsEmpty && !translationVector.IsEmpty)
      {
        Emgu.CV.Mat r3 = new Emgu.CV.Mat();
        Emgu.CV.CvInvoke.Rodrigues(rotationMat, r3);

        print("r");
        print(ARKit.MatExtension.GetValue(rotationMat, 0, 0));
        print(ARKit.MatExtension.GetValue(rotationMat, 0, 1));
        print(ARKit.MatExtension.GetValue(rotationMat, 0, 2));
        print(ARKit.MatExtension.GetValue(rotationMat, 1, 0));
        print(ARKit.MatExtension.GetValue(rotationMat, 1, 1));
        print(ARKit.MatExtension.GetValue(rotationMat, 1, 2));
        print(ARKit.MatExtension.GetValue(rotationMat, 2, 0));
        print(ARKit.MatExtension.GetValue(rotationMat, 2, 1));
        print(ARKit.MatExtension.GetValue(rotationMat, 2, 2));
        print("t");
        print(ARKit.MatExtension.GetValue(translationVector, 0, 0));
        print(ARKit.MatExtension.GetValue(translationVector, 0, 1));
        print(ARKit.MatExtension.GetValue(translationVector, 0, 2));

        float[] eulers = WebCamera.fp.GetEulerAngles(r3);

        Vector3 angles = new Vector3(eulers[0], eulers[1], eulers[2]);

        transform.Rotate(angles);
      }
    }
  }

}

