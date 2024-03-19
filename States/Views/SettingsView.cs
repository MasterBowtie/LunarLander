using System;
using System.Collections.Generic;
using System.Text;
using CS5410;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Xna.Framework.Media;

namespace Apedaile {
  public class SettingsView: GameStateView {

    public enum MenuState {
      Thrust, 
      RotateRight,
      RotateLeft,
      Pause,
    }

    private SpriteFont mainFont;
    private Texture2D selector;
    private Texture2D background;
    private Rectangle backRect;
    private Lander player;
    private KeyBindings bindings;
    private Dictionary<MenuState, IInputDevice.CommandDelegate> commands = new Dictionary<MenuState, IInputDevice.CommandDelegate>();
    private SaveBinding save;
    private Song music;

    private MenuState currentSelection = MenuState.Thrust;
    private GameStateEnum nextState = GameStateEnum.Settings;
    private SettingState currentState;
    private SettingState select;
    private SettingState rebind;
    private bool waitforKeyRelease = true;
    private TimeSpan delay = new TimeSpan(0, 0, 1);


    public override void setupInput()
    {
      // System.Console.WriteLine(delay.Milliseconds);
      select = new Select(this);
      rebind = new Rebind(this);
      currentState = select;

      keyboard.registerCommand(Keys.Up, waitforKeyRelease, new IInputDevice.CommandDelegate(moveUp));
      keyboard.registerCommand(Keys.Down, waitforKeyRelease, new IInputDevice.CommandDelegate(moveDown));
      keyboard.registerCommand(Keys.Enter, waitforKeyRelease, new IInputDevice.CommandDelegate(selectItem));
      keyboard.registerCommand(Keys.Escape, waitforKeyRelease, new IInputDevice.CommandDelegate(exitState));
      keyboard.registerCommand(Keys.F1, waitforKeyRelease, pauseMusic);
      keyboard.registerCommand(Keys.F2, waitforKeyRelease, resumeMusic);

      commands.Add(MenuState.Thrust, new IInputDevice.CommandDelegate(player.moveForward));
      commands.Add(MenuState.RotateRight, new IInputDevice.CommandDelegate(player.rotateRight));
      commands.Add(MenuState.RotateLeft, new IInputDevice.CommandDelegate(player.rotateLeft));
      commands.Add(MenuState.Pause, new IInputDevice.CommandDelegate(player.pause));
    }

    public override void loadMusic(Song music) {
      this.music = music;
    }

    public void attachBindings(KeyBindings bindings, SaveBinding save) {
      this.bindings = bindings;
      this.save = save;
      if (bindings == null) {
        this.bindings = new KeyBindings();
        saveBinding(MenuState.Thrust, Keys.W);
        saveBinding(MenuState.RotateRight, Keys.D);
        saveBinding(MenuState.RotateLeft, Keys.A);
        saveBinding(MenuState.Pause, Keys.P);
      } else {
        foreach (var bind in bindings.getBindings()) {
          List<Keys> keys = Enum.GetValues(typeof(Keys)).Cast<Keys>().ToList();
          List<MenuState> states = Enum.GetValues(typeof(MenuState)).Cast<MenuState>().ToList();
          foreach(MenuState state in states) {
            foreach (Keys key in keys) {
              if (key.ToString() == bind.Value && state.ToString() == bind.Key) {
                saveBinding(state, key);
                break;
              }
            }
          }
        }
      }
    }

    public void setupPlayer(Lander player) {
      this.player = player;
    }

    public override GameStateEnum processInput(GameTime gameTime)
    {
      delay -= gameTime.ElapsedGameTime;
      if (delay.Milliseconds <= 0) {
        currentState.processInput(gameTime);
      }
      if (nextState != GameStateEnum.Settings) {
        nextState = GameStateEnum.Settings;
        return GameStateEnum.MainMenu;
      }
      return GameStateEnum.Settings;
    }

    public override void loadContent(ContentManager contentManager)
    {
      mainFont = contentManager.Load<SpriteFont>("Fonts/CourierPrime");
      selector = contentManager.Load<Texture2D>("Images/MenuSelector");
      background = contentManager.Load<Texture2D>("Images/earth_image");
      backRect = new Rectangle(graphics.PreferredBackBufferWidth - background.Width/4, 0, background.Width/4, background.Height/4);

    
    }
    
    public override void render(GameTime gameTime)
    {
      spriteBatch.Begin();
      spriteBatch.Draw(background, backRect, Color.White);
      spriteBatch.End();

      currentState.render(gameTime);
    }

    public override void update(GameTime gameTime)
    {
      currentState.update(gameTime);
      if (MediaPlayer.State == MediaState.Stopped) {
        MediaPlayer.Play(music);
      }
    }

    private float drawMenuItem(SpriteFont font, string text, float y, float x, float xSize, bool selected) {
      Vector2 stringSize = font.MeasureString(text);
      
      if (selected) {
        spriteBatch.Draw(selector, new Rectangle((int)x, (int)y, (int) xSize, (int)stringSize.Y), Color.White);
      }
      spriteBatch.DrawString(
        font, text, new Vector2(graphics.PreferredBackBufferWidth/2 - stringSize.X/2, y), Color.White);

      return y + stringSize.Y;
    }

    public void moveUp(GameTime gameTime, float value) {
      if (currentSelection != MenuState.Thrust) {
        currentSelection = currentSelection - 1;
      }
    }

    public void moveDown(GameTime gameTime, float value) {
      if (currentSelection != MenuState.Pause) {
        currentSelection = currentSelection + 1;
      }
    }

    public void exitState(GameTime gameTime, float value){
      nextState = GameStateEnum.MainMenu;
      delay = new TimeSpan(0, 0, 1);
    }

    public void selectItem(GameTime gameTime, float value) {
      currentState = rebind;
    }

    public void saveBinding(MenuState action, Keys key) {
      if (action == MenuState.Pause) {
        player.bindCommand(commands[action], key, true);
      } else {
        player.bindCommand(commands[action], key, false);
      }
      
      bindings.submitBinding(action.ToString(), key.ToString());
      save(bindings);
    }

    // This is the different states and these could have been a lot cleaner but this is how it goes for now

    protected class Rebind: SettingState {
      private SettingsView parent;

      public Rebind(SettingsView parent) {
        this.parent = parent;
      }

      public void render(GameTime gameTime) {
        // parent.select.render(gameTime);
        parent.spriteBatch.Begin();
        Vector2 biggest = parent.mainFont.MeasureString("Press any key");
        int buffer = 50;
        float x = parent.graphics.PreferredBackBufferWidth/2 - biggest.X/2 - buffer/2;

        parent.drawMenuItem(parent.mainFont, string.Format("Rebinding: {0}", parent.currentSelection), parent.graphics.PreferredBackBufferHeight * .1f, x, biggest.X + buffer, false);
        parent.drawMenuItem(parent.mainFont, "Press Any Key", parent.graphics.PreferredBackBufferHeight/ 2, x, biggest.X + buffer, true);
        parent.spriteBatch.End();
      }
      
      public void update(GameTime gameTime) {

      }

      public void processInput(GameTime gameTime) {
        if (Keyboard.GetState().IsKeyDown(Keys.Escape)) {
          parent.currentState = parent.select;
        }
        Keys[] keys = Keyboard.GetState().GetPressedKeys();
        if (keys.Length == 1 && keys[0] != Keys.Enter && keys[0] != Keys.Escape) {
          parent.saveBinding(
            parent.currentSelection,
            keys[0]);
          parent.currentState = parent.select;
        }
      }
    } 

    protected class Select: SettingState {
      private SettingsView parent;

      public Select(SettingsView parent) {
        this.parent = parent;
      }

      public void render(GameTime gameTime) {
        var bindings = parent.bindings.getBindings();
        Vector2 biggest = parent.mainFont.MeasureString(string.Format("Rotate Right: {0}", bindings[MenuState.RotateRight.ToString()]));
        int buffer = 30;
        float x = parent.graphics.PreferredBackBufferWidth/2 - biggest.X/2 - buffer/2;
      
        parent.spriteBatch.Begin();

        float bottom = parent.drawMenuItem(parent.mainFont, string.Format("Thrust: {0}", bindings[MenuState.Thrust.ToString()]), parent.graphics.PreferredBackBufferHeight * .1f , x, biggest.X + buffer, parent.currentSelection == MenuState.Thrust);

        bottom = parent.drawMenuItem(parent.mainFont, string.Format("Rotate Right: {0}", bindings[MenuState.RotateRight.ToString()]), bottom, x, biggest.X + buffer, parent.currentSelection == MenuState.RotateRight);
      
        bottom = parent.drawMenuItem(parent.mainFont, string.Format("Rotate Left: {0}", bindings[MenuState.RotateLeft.ToString()]), bottom, x, biggest.X + buffer, parent.currentSelection == MenuState.RotateLeft);
      
        bottom = parent.drawMenuItem(parent.mainFont, string.Format("Pause: {0}", bindings[MenuState.Pause.ToString()]), bottom, x, biggest.X + buffer, parent.currentSelection == MenuState.Pause);

        bottom = parent.drawMenuItem(parent.mainFont, "Select: Enter", bottom, x, biggest.X + buffer, false);

        bottom = parent.drawMenuItem(parent.mainFont, "Exit: Escape", bottom, x, biggest.X + buffer, false);

        bottom = parent.drawMenuItem(parent.mainFont, "Pause Music: F1", bottom, x, biggest.X + buffer, false);

        bottom = parent.drawMenuItem(parent.mainFont, "Resume Music: F2", bottom, x, biggest.X + buffer, false);

        parent.spriteBatch.End();
      }

      public void update(GameTime gameTime) {
        // no updates here
      }

      public void processInput(GameTime gameTime) {
        parent.keyboard.Update(gameTime);
      }
    }

    private void pauseMusic(GameTime gameTime, float value) {
      MediaPlayer.Pause();
      System.Console.WriteLine("Music Paused");
    }

    private void resumeMusic(GameTime gameTime, float value) {
      MediaPlayer.Resume();
      System.Console.WriteLine("Music Resume");
    }
    
  }

  public interface SettingState {
      public void render(GameTime gameTime);
      public void update(GameTime gameTime);
      public void processInput(GameTime gameTime);
  }

  public delegate Task SaveBinding(KeyBindings item);
}