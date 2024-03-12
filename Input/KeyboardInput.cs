using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace CS5410
{
  ///<summary>
  ///Derived input device for the PC keyboard
  ///</summary>
  public class KeyboardInput : IInputDevice 
  {

    private KeyboardState m_statePrevious;

    /// <summary>
    /// Track all registered command in this dictionary
    /// </summary>
    private Dictionary<Keys, CommandEntry> m_commandEntries = new Dictionary<Keys, CommandEntry>();


    private struct CommandEntry {
      public Keys key;
      public bool keyPressOnly;
      public IInputDevice.CommandDelegate callback;

      public CommandEntry(Keys key, bool keyPressOnly, IInputDevice.CommandDelegate callback) {
        this.key = key;
        this.keyPressOnly = keyPressOnly;
        this.callback = callback;
      }
    }

    public List<IInputDevice.CommandDelegate> getCallbacks() {
      List<IInputDevice.CommandDelegate> entries = new List<IInputDevice.CommandDelegate>();
      foreach (var item in m_commandEntries) {
        entries.Add(item.Value.callback);
      }
      return entries;
    }

    public Keys getKey(IInputDevice.CommandDelegate callback) {
      foreach (var item in m_commandEntries) {
        if (item.Value.callback == callback) {
          return item.Key;
        }
      }
      return Keys.None;
    }

    ///<summary>
    /// Registers a callback-based command
    ///</summary>
    public void registerCommand(Keys key, bool keyPressOnly, IInputDevice.CommandDelegate callback) {
      // If callback already registered, remove it!
      foreach (var item in m_commandEntries) {
        if (item.Value.callback == callback) {
          m_commandEntries.Remove(item.Key);
        }
      }
      // If key already registered, remove
      if (m_commandEntries.ContainsKey(key)) {
        m_commandEntries.Remove(key);
      }
      m_commandEntries.Add(key, new CommandEntry(key, keyPressOnly, callback));
    }

    public void Update(GameTime gameTime) {
      KeyboardState state = Keyboard.GetState();
      
      foreach(CommandEntry entry in this.m_commandEntries.Values){
        if (entry.keyPressOnly && keyPressed(entry.key)) {
          entry.callback(gameTime, 1.0f);
        }
        else if (!entry.keyPressOnly && state.IsKeyDown(entry.key)){
          entry.callback(gameTime, 1.0f);
        }
      }
      m_statePrevious = state;
    }

    private bool keyPressed(Keys key) {
      return (Keyboard.GetState().IsKeyDown(key) && !m_statePrevious.IsKeyDown(key));
    }
  }
}