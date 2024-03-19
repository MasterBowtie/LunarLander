using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using CS5410;
using Microsoft.Xna.Framework.Input;

namespace Apedaile {
  [DataContract(Name = "Scores")]
  public class Scores {
    [DataMember()]
    public List<(uint, ushort)> HighScores = new List<(uint, ushort)>();

    public Scores() {
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
  }

  [DataContract(Name = "KeyBinds")]
  public class KeyBindings {
    
    [DataMember()]
    Dictionary<String, String> bindings = new Dictionary<string, string>();

    public KeyBindings() {
    }


    public void submitBinding(String action, String key) {
      bindings[action] = key;
    }

    public Dictionary<String, String> getBindings() {
      return bindings;
    }
  }
}