using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace Apedaile {
  [DataContract(Name = "Scores")]
  public class Scores {
    [DataMember()]
    public List<(uint, ushort)> HighScores = new List<(uint, ushort)>();

    public Scores() {
    }

    public void submitScore(uint score, ushort level)
    {
      if (HighScores.Count < 5)
      {
        HighScores.Add((score, level));

      }
      else
      {
        for (int i = 0; i < HighScores.Count; i++)
        {
          var item = HighScores[i];
          if (score > item.Item1)
          {
            HighScores.Insert(i, (score, level));
          }
        }
        if (HighScores.Count > 5)
        {
          HighScores.RemoveRange(5, HighScores.Count);
        }
      }
      HighScores.Sort(compare);
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
}