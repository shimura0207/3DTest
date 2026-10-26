/*
 * @file    FieldSIZEUIControllere.cs
 * @autor   Kuu
 */

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FieldSIZEUIControllere : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    public int fieldSize = 0;
    // Start is called before the first frame update
    void Start() {

    }

    // Update is called once per frame
    void Update() {
        text.text = fieldSize.ToString();
    }

    
}
