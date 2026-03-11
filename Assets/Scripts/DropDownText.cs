using System;
using System.Collections;
using System.Collections.Generic;
using Custom;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DropDownText : MonoBehaviour
{
    public CustomTMPDropdown dropdown;
    public TMP_InputField inputField;
    public Button isInteractableButton;
    
    void Start()
    {
        dropdown.ClearOptions();
        dropdown.AddOptions(new List<string> { "Option 1", "Option 2", "Option 3" });
        dropdown.interactable = true;
        dropdown.onValueChanged.AddListener(OnDropdownValueChanged);
        isInteractableButton.onClick.AddListener(OnInteractableButtonClicked);
    }

    public void OnDropdownValueChanged(int value)
    {
        print("Selected option: " + dropdown.options[value].text);
        dropdown.options[value].text = inputField.text;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            AddOption();
        }
    }

    public void AddOption()
    {
        string newOption = inputField.text;
        dropdown.options.Add(new TMP_Dropdown.OptionData(newOption));
        dropdown.RefreshShownValue();
    }

    public void OnInteractableButtonClicked()
    {
        dropdown.interactable = !dropdown.interactable;
    }

}
