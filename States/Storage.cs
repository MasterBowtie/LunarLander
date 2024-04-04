using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using CS5410;
using Microsoft.Xna.Framework.Input;

namespace Apedaile {
  [DataContract(Name = "Storage")]
  public class Storage {
    [DataMember()]
    public List<(uint, ushort)> HighScores = new List<(uint, ushort)>();

    private KeyboardInput keyboard;

    public Storage(KeyboardInput keyboard) {
      this.keyboard = keyboard;
    }

    public void submitScore(uint score, ushort level)
    {
      HighScores.Add((score, level));
      HighScores.Sort(compare);
      if (HighScores.Count > 5)
      {
        HighScores.RemoveAt(5);
      }
    }

    public int compare((uint, ushort) item1, (uint, ushort) item2) {
      if (item1.Item2 > item2.Item2) {
        return -1;
      } else if (item1.Item2 == item2.Item2) {
        if (item1.Item1 > item2.Item1) {
          return -1;
        }
      }
      return 1;
    }

    [DataMember()]
    Dictionary<string, Dictionary<string, CommandString>> bindings = new Dictionary<string, Dictionary<string, CommandString>>();

    public struct CommandString {
      public string key;
      public bool keyPressOnly;
      public string action;

      public CommandString(Keys key, bool keyPressOnly, Actions action) {
        this.key = key.ToString();
        this.keyPressOnly = keyPressOnly;
        this.action = action.ToString();
      }
    }

    public void registerCommand(GameStateEnum state, Keys key, bool keyPressOnly, Actions action, IInputDevice.CommandDelegate callback) {
      KeyboardInput.CommandEntry commandEntry = new KeyboardInput.CommandEntry(key, keyPressOnly, callback, action);
      keyboard.registerCommand(key, keyPressOnly, callback, state, action);
      if (bindings.ContainsKey(state.ToString())) {
        if (bindings[state.ToString()].ContainsKey(key.ToString())) {
          bindings[state.ToString()][key.ToString()] = new CommandString(key, keyPressOnly, action);
        }
        else {
          bindings[state.ToString()].Add(key.ToString(), new CommandString(key, keyPressOnly, action));
        }
      } 
      else {
        bindings.Add(state.ToString(), new Dictionary<string, CommandString>());
        bindings[state.ToString()].Add(key.ToString(), new CommandString(key, keyPressOnly, action));
      }
    }

    public void loadCommands() {
      var stateCommands = keyboard.getStateCommands();
      foreach (var state in Enum.GetValues(typeof(GameStateEnum))) {
        var stateBindings = bindings[state.ToString()];
        foreach (var action in Enum.GetValues(typeof(Actions))) {
          if (stateBindings.ContainsKey(action.ToString())) {
            foreach (var key in Enum.GetValues(typeof(Keys))) {
              if (stateBindings[key.ToString()].action == action.ToString()) {
                KeyboardInput.CommandEntry commandEntry = stateCommands[(GameStateEnum)state][(Actions)action];
                commandEntry.key = (Keys)key;
                commandEntry.action = (Actions)action;
              }
            }
          }
        }
      }
    }

  }
}
