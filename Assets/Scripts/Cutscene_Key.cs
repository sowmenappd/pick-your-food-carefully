﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cutscene_Key : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D col){
      if(col.tag == "Finish")
        Destroy(gameObject);
    }
}
