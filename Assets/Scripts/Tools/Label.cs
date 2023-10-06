using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Label : MonoBehaviour
{
    #region Variables

    [Header("Settings")]

    [Tooltip("Text the label will display (overrides child TMP if not blank)")]
    [SerializeField] private string text;

    [Tooltip("Should this label orient to always face the camera?")]
    [SerializeField] private bool orientToCamera;

    // Refernce to the UI canvas for this label
    private Canvas canvas;

    // Reference to the TextMeshPro Text for this label
    private TMP_Text tmpText;

    #endregion

    #region Methods

    private void Awake()
    {
        // Getting references and checking for null
        canvas = GetComponentInChildren<Canvas>();
        if (canvas == null) Debug.LogError("Label must contain a Canvas!");

        tmpText = GetComponentInChildren<TMP_Text>();
        if (tmpText == null) Debug.LogError("Label must contain a TMPText!");
    }

    private void Start()
    {
        // Change text in label if the inspector field isn't empty
        if (text != "")
        {
            tmpText.text = text;
        }
    }

    private void Update()
    {
        // Rotate to face the camera
        if (orientToCamera) transform.forward = Camera.main.transform.forward;
    }

    #endregion
}
