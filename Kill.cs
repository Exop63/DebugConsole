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