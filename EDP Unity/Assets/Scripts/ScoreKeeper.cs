using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreKeeper : MonoBehaviour
{
    public int scoreP1 = 0;
    public int scoreP2 = 0;
    public Text text;


    // Start is called before the first frame update
    void Start()
    {
        text.text = scoreP1.ToString() + ":" + scoreP2.ToString();
    }


    public void ScoreGoal(string source)
    {
        if (source == "P2")
        {
            scoreP1++;
            text.text = scoreP1.ToString() + ":" + scoreP2.ToString();
        }
        else if(source == "P1")
        {
            scoreP2++;
            text.text = scoreP1.ToString() + ":" + scoreP2.ToString();
        }

    }

}
