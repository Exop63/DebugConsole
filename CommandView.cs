using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static DebugConsole;

public class CommandView : MonoBehaviour
{
    [SerializeField] private TMP_Text commandText;
    public void Init(Command command)
    {
        commandText.text = $"{command.Name} - {command.Description}";
    }
}
