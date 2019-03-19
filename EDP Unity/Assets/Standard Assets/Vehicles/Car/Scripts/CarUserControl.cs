using System;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

namespace UnityStandardAssets.Vehicles.Car
{
    [RequireComponent(typeof (CarController))]
    public class CarUserControl : MonoBehaviour
    {
        private CarController m_Car; // the car controller we want to use
        public String HorizontalCtrl = "Horizontal_P1";
        public String VerticalCtrl = "Vertical_P1";
        public String JumpButton = "Jump_P1";


        private void Awake()
        {
            // get the car controller
            m_Car = GetComponent<CarController>();
        }


        private void FixedUpdate()
        {
            // pass the input to the car!
            float h = Input.GetAxis(HorizontalCtrl);
            float v = Input.GetAxis(VerticalCtrl);
#if !MOBILE_INPUT
            float handbrake = Input.GetAxis(JumpButton);
            m_Car.Move(h, v, v, handbrake);
#else
            m_Car.Move(h, v, v, 0f);
#endif
        }
    }
}
