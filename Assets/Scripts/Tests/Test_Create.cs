using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test_Create : MonoBehaviour
{
    public GameObject model;


    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < 1000; i++)
        {
            var cell = Instantiate(model, transform);
            cell.SetActive(true);
            cell.name = $"{i + 1}"; 
        }
    }

}
