using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class count : MonoBehaviour
{
    // Start is called before the first frame update
    float cnt = 0;
    public Text t;
    void Start()
    {
        Application.targetFrameRate = 60;
        cntNum();

        // Update is called once per frame
        
    }
    void Update()
    {
        cnt += Time.deltaTime;

    }
    public void cntNum()
    {
        t.text = (int)cnt + "";
        Invoke("cntNum", 1f);
    }
}
