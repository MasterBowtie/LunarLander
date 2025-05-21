using System;
using System.Collections.Generic;
using System.Text;
using CS5410;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Apedaile {
  public class SettingsView: GameStateView {

    private SpriteFont mainFont;
    private Texture2D selector;
    private Texture2D background;
    private Rectangle backRect;
    private Lander player;
    private Storage storage;
    private Dictionary<Actions, IInputDevice.CommandDelegate> commands = new Dictionary<Actions, IInputDevice.CommandDelegate>();
    private SaveBinding save;
    private Song music;

    private Actions currentSelection = Actions.thrust;
    private GameStateEnum nextState = GameStateEnum.Settings;
    private SettingState currentState;
    private SettingState select;
    private SettingState rebind;
    private bool waitforKeyRelease = true;
    private TimeSpan delay = new TimeSpan(0, 0, 1);


    public override void setupInput(Storage storage, KeyboardInput keyboard)
    {
      this.keyboard = keyboard;
      this.storage = storage;
      // System.Console.WriteLine(delay.Milliseconds);
      select = new Select(this);
      rebind = new Rebind(this);
      currentState = select;

      storage.registerCommand(GameStateEnum.Settings, Keys.Up, waitforKeyRelease, Actions.up, new IInputDevice.CommandDelegate(moveUp));
      storage.registerCommand(GameStateEnum.Settings, Keys.Down, waitforKeyRelease, Actions.down, new IInputDevice.CommandDelegate(moveDown));
      storage.registerCommand(GameStateEnum.Settings, Keys.Enter, waitforKeyRelease, Actions.select, new IInputDevice.CommandDelegate(selectItem));
      storage.registerCommand(GameStateEnum.Settings, Keys.Escape, waitforKeyRelease, Actions.exit, new IInputDevice.CommandDelegate(exitState));
      storage.registerCommand(GameStateEnum.Settings, Keys.F1, waitforKeyRelease, Actions.pauseMusic, pauseMusic);
      storage.registerCommand(GameStateEnum.Settings, Keys.F2, waitforKeyRelease, Actions.playMusic, resumeMusic);

      commands.Add(Actions.thrust, new IInputDevice.CommandDelegate(player.moveForward));
      commands.Add(Actions.right, new IInputDevice.CommandDelegate(player.rotateRight));
      commands.Add(Actions.left, new IInputDevice.CommandDelegate(player.rotateLeft));
      commands.Add(Actions.pauseGame, new IInputDevice.CommandDelegate(player.pause));

      storage.registerCommand(GameStateEnum.GamePlay, Keys.W, false, Actions.thrust, commands[Actions.thrust]);
      storage.registerCommand(GameStateEnum.GamePlay, Keys.D, false, Actions.right, commands[Actions.right]);
      storage.registerCommand(GameStateEnum.GamePlay, Keys.A, false, Actions.left, commands[Actions.left]);
      storage.registerCommand(GameStateEnum.GamePlay, Keys.P, true, Actions.pauseGame, commands[Actions.pauseGame]);

    }

    public override void loadMusic(Song music) {
      this.music = music;
    }

    public void setupExtras(Lander player, SaveBinding save) {
      this.player = player;
      this.save = save;
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
      if (currentSelection != Actions.thrust) {
        currentSelection = currentSelection - 1;
      }
    }

    public void moveDown(GameTime gameTime, float value) {
      if (currentSelection != Actions.pauseGame) {
        currentSelection = currentSelection + 1;
      }
    }

    public void exitState(GameTime gameTime, float value){
      nextState = GameStateEnum.MainMenu;
      delay = new TimeSpan(0, 0, 1);
    }

    public void selectItem(GameTime gameTime, float value) {
      currentState = rebind;
      delay = new TimeSpan(0, 0, 1);
    }

    public void saveBinding(Actions action, Keys key) {
      if (action == Actions.pauseGame) {
        storage.registerCommand(GameStateEnum.GamePlay, key, true, action, commands[action]);
      } else {
        storage.registerCommand(GameStateEnum.GamePlay, key, false, action, commands[action]);
      }
      
      save();
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
        var bindings_p = parent.storage.getBindings();
        var bindings = bindings_p[GameStateEnum.GamePlay.ToString()];
        Vector2 biggest = parent.mainFont.MeasureString(string.Format("Rotate Right: {0}", bindings[Actions.right.ToString()]));
        int buffer = 30;
        float x = parent.graphics.PreferredBackBufferWidth/2 - biggest.X/2 - buffer/2;
      
        parent.spriteBatch.Begin();

        float bottom = parent.drawMenuItem(parent.mainFont, string.Format("Thrust: {0}", bindings[Actions.thrust.ToString()].key), parent.graphics.PreferredBackBufferHeight * .1f , x, biggest.X + buffer, parent.currentSelection == Actions.thrust);

        bottom = parent.drawMenuItem(parent.mainFont, string.Format("Rotate Right: {0}", bindings[Actions.right.ToString()].key), bottom, x, biggest.X + buffer, parent.currentSelection == Actions.right);
      
        bottom = parent.drawMenuItem(parent.mainFont, string.Format("Rotate Left: {0}", bindings[Actions.left.ToString()].key), bottom, x, biggest.X + buffer, parent.currentSelection == Actions.left);
      
        bottom = parent.drawMenuItem(parent.mainFont, string.Format("Pause: {0}", bindings[Actions.pauseGame.ToString()].key), bottom, x, biggest.X + buffer, parent.currentSelection == Actions.pauseGame);

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

  public delegate void SaveBinding();
}