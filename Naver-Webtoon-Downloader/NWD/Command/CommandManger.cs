using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WRforest.NWD.Command
{
    class CommandManager
    {
        public CommandManager()
        {
            commandDictionary = new Dictionary<string, Command>();
            /*
            agent = Parser.Agent.Instance;
            parser = Parser.Parser.Instance;
            commandDictionary = new Dictionary<string, CommandDelegate>();
            commandDictionary.Add("get", Get);
            commandDictionary.Add("clear", Clear);
            commandDictionary.Add("download", Download);
            commandDictionary.Add("d", Download);
            commandDictionary.Add("merge", Merge);
            commandDictionary.Add("set", Set);
            */
        }
        Dictionary<string, Command> commandDictionary;
        Parser.Agent agent;
        Parser.Parser parser;
        public bool Contains(string commandName)
        {
            return commandDictionary.ContainsKey(commandName);
        }
        public void Start(string commandName, params string[] args)
        {
            commandDictionary[commandName].Start(args);
        }
        public string[] GetCommandList()
        {
            return commandDictionary.Keys.ToArray();
        }
    }
}
