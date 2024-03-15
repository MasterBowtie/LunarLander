using System;
using System.Collections.Generic;
using System.Text;
using CS5410;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Apedaile {
  public class SettingsView: GameStateView {

    private enum MenuState {
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
    private Dictionary<MenuState, (IInputDevice.CommandDelegate, Keys)> bindings;

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

      bindings = new Dictionary<MenuState ,(IInputDevice.CommandDelegate, Keys)>();
      bindings.Add(MenuState.Thrust ,(new IInputDevice.CommandDelegate(player.moveForward), Keys.W));
      bindings.Add(MenuState.RotateRight ,(new IInputDevice.CommandDelegate(player.rotateRight), Keys.D));
      bindings.Add(MenuState.RotateLeft ,(new IInputDevice.CommandDelegate(player.rotateLeft), Keys.A));
      bindings.Add(MenuState.Pause ,(new IInputDevice.CommandDelegate(player.pause), Keys.P));

      foreach (var bind in bindings) {
        if (bind.Value == bindings[MenuState.Pause]) {
          player.bindCommand(bind.Value.Item1, bind.Value.Item2, true);
        } else {
          player.bindCommand(bind.Value.Item1, bind.Value.Item2, false);
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
          parent.bindings[parent.currentSelection] = (parent.bindings[parent.currentSelection].Item1, keys[0]);
          if (parent.currentSelection == MenuState.Pause) {
            parent.player.bindCommand(parent.bindings[parent.currentSelection].Item1, keys[0], true);
          } else { 
            parent.player.bindCommand(parent.bindings[parent.currentSelection].Item1, keys[0], false);
          }
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
        Vector2 biggest = parent.mainFont.MeasureString(string.Format("Rotate Right: {0}",parent.bindings[MenuState.RotateRight].Item2));
        int buffer = 50;
        float x = parent.graphics.PreferredBackBufferWidth/2 - biggest.X/2 - buffer/2;
      
        parent.spriteBatch.Begin();

        float bottom = parent.drawMenuItem(parent.mainFont, string.Format("Thrust: {0}",parent.bindings[MenuState.Thrust].Item2), parent.graphics.PreferredBackBufferHeight * .2f , x, biggest.X + buffer, parent.currentSelection == MenuState.Thrust);

        bottom = parent.drawMenuItem(parent.mainFont, string.Format("Rotate Right: {0}",parent.bindings[MenuState.RotateRight].Item2), bottom, x, biggest.X + buffer, parent.currentSelection == MenuState.RotateRight);
      
        bottom = parent.drawMenuItem(parent.mainFont, string.Format("Rotate Left: {0}",parent.bindings[MenuState.RotateLeft].Item2), bottom, x, biggest.X + buffer, parent.currentSelection == MenuState.RotateLeft);
      
        bottom = parent.drawMenuItem(parent.mainFont, string.Format("Pause: {0}",parent.bindings[MenuState.Pause].Item2), bottom, x, biggest.X + buffer, parent.currentSelection == MenuState.Pause);

        bottom = parent.drawMenuItem(parent.mainFont, "Select: Enter", bottom, x, biggest.X + buffer, false);

        bottom = parent.drawMenuItem(parent.mainFont, "Exit: Escape", bottom, x, biggest.X + buffer, false);

        parent.spriteBatch.End();
      }

      public void update(GameTime gameTime) {
        // no updates here
      }

      public void processInput(GameTime gameTime) {
        parent.keyboard.Update(gameTime);
      }
      
    }
  }

  public interface SettingState {
      public void render(GameTime gameTime);
      public void update(GameTime gameTime);
      public void processInput(GameTime gameTime);
  }
}