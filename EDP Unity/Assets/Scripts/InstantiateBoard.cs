using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstantiateBoard : MonoBehaviour
{

    public GameObject MainGame;
    public Transform Trigger;
    private GameObject MainGameInstance;
    bool instantiated = false;
    public bool objectFound = false;


    // Update is called once per frame
    void Update()
    {
        print("Object Found = " + objectFound);
        if (objectFound == true && instantiated == false)
        {
            MainGameInstance = Instantiate(MainGame, Trigger.position, Trigger.rotation);
            instantiated = true;
        }

        if(instantiated == true)
        {
            MainGameInstance.transform.position = Trigger.transform.position;
            MainGameInstance.transform.rotation = Trigger.transform.rotation;
        }


        if(objectFound == false && instantiated == true)
        {
            Destroy(MainGameInstance);
            instantiated = false;
        }
        
    }
}
