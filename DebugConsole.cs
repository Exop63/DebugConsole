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
        // Not work
        //if (commandInputField.isFocused && Input.GetKeyDown(KeyCode.KeypadEnter)){
        //    SendCommand();
        //}
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
        commands.Add(new Help(this));
        commands.Add(new Clear(this));
        commands.Add(new ListMinions(this));
        //commands.Add(new BotReqCommand(this));

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
    #region Commands
    public class Kill : Command
    {
        GameManager gameManager;
        //("Kill", "Kill any minion with name");
        public Kill(DebugConsole debugConsole) : base(debugConsole)
        {
            Name = "Kill";
            Description = "Kill any minion with name. example kill self Ra";
        }

        public override void InvokeCmnd()
        {
            base.InvokeCmnd();
            gameManager = GameController.GetSingleton().GameManager;

            if (Args.Length != 2) DebugConsole.Log("Kill has 2 parameters ");
            var isSelf = Args[0].ToLower().Contains("self");
            var minionKeyId = Args[1].ToLower();

            foreach (var item in PhotonNetwork.room.MatchData.MinionDic)
            {
                if (item.Key.ToLower().Contains(minionKeyId) && gameManager.PlayerOwner.Equals(item.Value.Owner) == isSelf)
                {
                    Networker.SendEvent(new KillCommandEventCTS(item.Key));
                }
            }




        }

    }
    public class ListMinions : Command
    {
        GameManager gameManager;

        public ListMinions(DebugConsole debugConsole) : base(debugConsole)
        {
            Name = "minion-list";
            Description = "List all minions with key id";
        }
        public override void InvokeCmnd()
        {
            base.InvokeCmnd();
            gameManager = GameController.GetSingleton().GameManager;

            foreach (var item in PhotonNetwork.room.MatchData.MinionDic)
            {
                var msg = "";
                if (gameManager.PlayerOwner.Equals(item.Value.Owner)) msg = "Selg: ";
                else msg = "Opponet: ";
                DebugConsole.Log($"{msg} Minion: {item.Key}");
            }
        }
    }

    public class BotReqCommand : Command
    {
        public BotReqCommand(DebugConsole debugConsole) : base(debugConsole)
        {
            Name = "BotReq";
            Description = "Create a boot game.";
        }
        public override void InvokeCmnd()
        {
            base.InvokeCmnd();
            Hashtable roomProp = new Hashtable();
            roomProp.Add("GameMode", MatchmakingManager.GetSingleton().GameMode); //0,1


            RoomOptions roomOptions = new RoomOptions { MaxPlayers = 2, CustomRoomProperties = roomProp };
            roomOptions.CustomRoomPropertiesForLobby = new string[] { "GameMode" };
            PhotonNetwork.CreateRoom(Magic.RandomString(6), roomOptions, null);

            //Networker.SendEvent(new BotRequestEventCTS());
            DebugConsole.Invoke(1, () => Networker.SendEvent(new BotRequestEventCTS()));
        }


    }
    public class Help : Command
    {
        public Help(DebugConsole debugConsole) : base(debugConsole)
        {
            Name = "Help";
            Description = "List all commands";
        }
        public override void InvokeCmnd()
        {
            base.InvokeCmnd();
            DebugConsole.ShowCommands();
        }
    }

    public class Clear : Command
    {
        public Clear(DebugConsole debugConsole) : base(debugConsole)
        {
            Name = "Clear";
            Description = "Clear all logs.";
        }

        public override void InvokeCmnd()
        {
            base.InvokeCmnd();
            DebugConsole.ClearLogs();
        }

    }
    #endregion
    public class Command
    {
        public string Name { get; set; }
        public string Description { get; set; }

        private DebugConsole _debugConsole = null;
        internal DebugConsole DebugConsole => _debugConsole;
        public string[] Args { get; set; }
        public Command(DebugConsole debugConsole)
        {
            _debugConsole = debugConsole;
        }
        public Command(string name, string description)
        {
            Name = name;
            Description = description;
        }

        public virtual void InvokeCmnd()
        {
            Debug.Log(Name + $" Command Arg: {string.Join(',', Args)}");
        }
    }
}
