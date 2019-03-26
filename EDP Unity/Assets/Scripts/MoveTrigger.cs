using System;
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
      WebCamera.fp.GetCenter(WebCamera.ip.CameraMatrix, WebCamera.ip.DistortionCoefficients, out Emgu.CV.Mat rotationMat,
        out Emgu.CV.Mat translationVector, out float[] center);

      if (!rotationMat.IsEmpty && !translationVector.IsEmpty)
      {
        Emgu.CV.Mat r3 = new Emgu.CV.Mat();
        Emgu.CV.CvInvoke.Rodrigues(rotationMat, r3);
        
        print("rodrigues " + rotationMat.Rows + "x" + rotationMat.Cols);
        print(ARKit.MatExtension.GetValue(rotationMat, 0, 0));
        print(ARKit.MatExtension.GetValue(rotationMat, 1, 0));
        print(ARKit.MatExtension.GetValue(rotationMat, 2, 0));
        
        print("r " + r3.Rows + "x" + r3.Cols);
        print(ARKit.MatExtension.GetValue(r3, 0, 0));
        print(ARKit.MatExtension.GetValue(r3, 0, 1));
        print(ARKit.MatExtension.GetValue(r3, 0, 2));
        print(ARKit.MatExtension.GetValue(r3, 1, 0));
        print(ARKit.MatExtension.GetValue(r3, 1, 1));
        print(ARKit.MatExtension.GetValue(r3, 1, 2));
        print(ARKit.MatExtension.GetValue(r3, 2, 0));
        print(ARKit.MatExtension.GetValue(r3, 2, 1));
        print(ARKit.MatExtension.GetValue(r3, 2, 2));
        
        /*
        print("t");
        print(ARKit.MatExtension.GetValue(translationVector, 0, 0));
        print(ARKit.MatExtension.GetValue(translationVector, 0, 1));
        print(ARKit.MatExtension.GetValue(translationVector, 0, 2));
        */

        float[] eulers = WebCamera.fp.GetEulerAngles(r3);

        print("eulers");
        print(eulers[0] + "\t" + eulers[1] + "\t" + eulers[2]);

        transform.rotation = Quaternion.Euler(eulers[0] * Mathf.PI, eulers[2] * Mathf.PI, eulers[1] * Mathf.PI);
        /*
        transform.position = new Vector3(
          ARKit.MatExtension.GetValue(translationVector, 0, 0),
          150 - ARKit.MatExtension.GetValue(translationVector, 0, 2) / 4.2f,
          -ARKit.MatExtension.GetValue(translationVector, 0, 1)
        );
        */
        transform.position = new Vector3(
           center[0],
           150 - ARKit.MatExtension.GetValue(translationVector, 0, 2) / 4.2f,
           -center[1]
        );
      }
      else
      {
        transform.rotation = new Quaternion(0, 0, 0, 0);
        //transform.position = new Vector3(-266.1339f, 335, 1263.554f);
        transform.position = new Vector3(0, 0, 0);
      }
    }
  }

}

