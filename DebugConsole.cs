using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;
using Photon.LoadBalancing.Ntroy.Messages.Events.MessageArgs;

using Hashtable = ExitGames.Client.Photon.Hashtable;

public class DebugConsole : MonoBehaviour
{
    [SerializeField] private bool DontDestroy = true;

    private List<Command> commands = new List<Command>();
    private List<Command> Logs = new List<Command>();
    [SerializeField]
    private RectTransform commandViwer;
    [SerializeField] private CommandView commandView;
    [SerializeField] private TMP_InputField commandInputField;
    private List<GameObject> commandObjects = new List<GameObject>();
    [SerializeField] private GameObject console;

    private bool isVisable = false;

    private DebugConsole instance;
    public DebugConsole Instance => instance;

    void Start()
    {
        if (instance == null) instance = this;
        else if (instance != this) Destroy(gameObject);
        if (DontDestroy) DontDestroyOnLoad(this);
        InitCommands();
        ShowCommands();
        isVisable = console.activeSelf;
    }
    private void Update()
    {      
        if (Input.GetKeyDown(KeyCode.BackQuote))
        {
            ShowToggle();
            Debug.Log("Chating!");
        }
    }
    public void Invoke(int time, Action action)
    {
        StartCoroutine(InvokeNumerator(time, action));
    }
    private IEnumerator InvokeNumerator(float time, Action action)
    {
        yield return new WaitForSeconds(time);
        if (action != null) action();
    }
    private void ShowToggle()
    {
        isVisable = !isVisable;
        console.SetActive(isVisable);
    }

    private void ShowCommands()
    {
        foreach (var command in commands) Log(command);
    }

    public void ClearLogs()
    {
        foreach (var item in commandObjects) Destroy(item);
        Logs.Clear();
        commandObjects.Clear();
    }
    public void Log(Command command)
    {
        var commandViewObject = Instantiate<CommandView>(commandView, commandViwer.transform);
        commandViewObject.Init(command);
        commandObjects.Add(commandViewObject.gameObject);
        Logs.Add(command);
    }
    public void Log(string msg)
    {
        var cmd = new Command("Log", msg);
        Log(cmd);
    }
    private void InitCommands()
    {
        commands.Clear();
        commands.Add(new Kill(this));
    }


    public void SendCommand()
    {
        var msg = commandInputField.text;
        if (!String.IsNullOrEmpty(msg) && TryGetCommand(msg, out var cmd)) cmd.InvokeCmnd();
        else
        {
            Debug.LogError("Command not found!");
            Log(msg);
        }
        commandInputField.text = "";
    }

    private bool TryGetCommand(string msg, out Command cmd)
    {
        var cmndArg = msg.Split(' ');
        cmd = commands.Find(it => it.Name.ToLower() == cmndArg[0].ToLower());
        if (cmd == null) return false;


        cmd.Args = cmndArg.Skip(1).ToArray();
        return true;
    }
    
}
