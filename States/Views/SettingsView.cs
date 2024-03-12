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

    private SpriteFont _mainFont;
    private Texture2D _selector;
    private Lander _player;
    private Dictionary<MenuState, (IInputDevice.CommandDelegate, Keys)> _bindings;

    private MenuState _currentSelection = MenuState.Thrust;
    private GameStateEnum _nextState = GameStateEnum.Settings;
    private SettingState _currentState;
    private SettingState _select;
    private SettingState _rebind;
    private bool _waitforKeyRelease = true;
    private TimeSpan _delay = new TimeSpan(0, 0, 1);


    public override void setupInput()
    {
      // System.Console.WriteLine(_delay.Milliseconds);
      _select = new Select(this);
      _rebind = new Rebind(this);
      _currentState = _select;

      _keyboard.registerCommand(Keys.Up, _waitforKeyRelease, new IInputDevice.CommandDelegate(moveUp));
      _keyboard.registerCommand(Keys.Down, _waitforKeyRelease, new IInputDevice.CommandDelegate(moveDown));
      _keyboard.registerCommand(Keys.Enter, _waitforKeyRelease, new IInputDevice.CommandDelegate(selectItem));
      _keyboard.registerCommand(Keys.Escape, _waitforKeyRelease, new IInputDevice.CommandDelegate(exitState));

      _bindings = new Dictionary<MenuState ,(IInputDevice.CommandDelegate, Keys)>();
      _bindings.Add(MenuState.Thrust ,(new IInputDevice.CommandDelegate(_player.moveForward), Keys.W));
      _bindings.Add(MenuState.RotateRight ,(new IInputDevice.CommandDelegate(_player.rotateRight), Keys.D));
      _bindings.Add(MenuState.RotateLeft ,(new IInputDevice.CommandDelegate(_player.rotateLeft), Keys.A));
      _bindings.Add(MenuState.Pause ,(new IInputDevice.CommandDelegate(_player.pause), Keys.P));

      foreach (var bind in _bindings) {
        if (bind.Value == _bindings[MenuState.Pause]) {
          _player.bindCommand(bind.Value.Item1, bind.Value.Item2, true);
        } else {
          _player.bindCommand(bind.Value.Item1, bind.Value.Item2, false);
        }
      }
    }

    public void setupPlayer(Lander player) {
      _player = player;
    }

    public override GameStateEnum processInput(GameTime gameTime)
    {
      _delay -= gameTime.ElapsedGameTime;
      if (_delay.Milliseconds <= 0) {
        _currentState.processInput(gameTime);
      }
      if (_nextState != GameStateEnum.Settings) {
        _nextState = GameStateEnum.Settings;
        return GameStateEnum.MainMenu;
      }
      return GameStateEnum.Settings;
    }

    public override void loadContent(ContentManager contentManager)
    {
      _mainFont = contentManager.Load<SpriteFont>("Fonts/CourierPrime");
      _selector = contentManager.Load<Texture2D>("Images/MenuSelector");
    }
    public override void render(GameTime gameTime)
    {
      _currentState.render(gameTime);
    }

    public override void update(GameTime gameTime)
    {
      _currentState.update(gameTime);
    }

    private float drawMenuItem(SpriteFont font, string text, float y, float x, float xSize, bool selected) {
      Vector2 stringSize = font.MeasureString(text);
      
      if (selected) {
        _spriteBatch.Draw(_selector, new Rectangle((int)x, (int)y, (int) xSize, (int)stringSize.Y), Color.White);
      }
      _spriteBatch.DrawString(
        font, text, new Vector2(_graphics.PreferredBackBufferWidth/2 - stringSize.X/2, y), Color.White);

      return y + stringSize.Y;
    }

    public void moveUp(GameTime gameTime, float value) {
      if (_currentSelection != MenuState.Thrust) {
        _currentSelection = _currentSelection - 1;
      }
    }

    public void moveDown(GameTime gameTime, float value) {
      if (_currentSelection != MenuState.Pause) {
        _currentSelection = _currentSelection + 1;
      }
    }

    public void exitState(GameTime gameTime, float value){
      _nextState = GameStateEnum.MainMenu;
      _delay = new TimeSpan(0, 0, 1);
    }

    public void selectItem(GameTime gameTime, float value) {
      _currentState = _rebind;
    }


    // This is the different states and these could have been a lot cleaner but this is how it goes for now

    protected class Rebind: SettingState {
      private SettingsView parent;

      public Rebind(SettingsView parent) {
        this.parent = parent;
      }

      public void render(GameTime gameTime) {
        // parent._select.render(gameTime);
        parent._spriteBatch.Begin();
        Vector2 biggest = parent._mainFont.MeasureString("Press any key");
        int buffer = 50;
        float x = parent._graphics.PreferredBackBufferWidth/2 - biggest.X/2 - buffer/2;

        parent.drawMenuItem(parent._mainFont, string.Format("Rebinding: {0}", parent._currentSelection), parent._graphics.PreferredBackBufferHeight * .1f, x, biggest.X + buffer, false);
        parent.drawMenuItem(parent._mainFont, "Press Any Key", parent._graphics.PreferredBackBufferHeight/ 2, x, biggest.X + buffer, true);
        parent._spriteBatch.End();
      }
      
      public void update(GameTime gameTime) {

      }

      public void processInput(GameTime gameTime) {
        if (Keyboard.GetState().IsKeyDown(Keys.Escape)) {
          parent._currentState = parent._select;
        }
        Keys[] keys = Keyboard.GetState().GetPressedKeys();
        if (keys.Length == 1 && keys[0] != Keys.Enter && keys[0] != Keys.Escape) {
          parent._bindings[parent._currentSelection] = (parent._bindings[parent._currentSelection].Item1, keys[0]);
          if (parent._currentSelection == MenuState.Pause) {
            parent._player.bindCommand(parent._bindings[parent._currentSelection].Item1, keys[0], true);
          } else { 
            parent._player.bindCommand(parent._bindings[parent._currentSelection].Item1, keys[0], false);
          }
          parent._currentState = parent._select;
        }
      }
    } 

    protected class Select: SettingState {
      private SettingsView parent;

      public Select(SettingsView parent) {
        this.parent = parent;
      }

      public void render(GameTime gameTime) {
        Vector2 biggest = parent._mainFont.MeasureString(string.Format("Rotate Right: {0}",parent._bindings[MenuState.RotateRight].Item2));
        int buffer = 50;
        float x = parent._graphics.PreferredBackBufferWidth/2 - biggest.X/2 - buffer/2;
      
        parent._spriteBatch.Begin();

        float bottom = parent.drawMenuItem(parent._mainFont, string.Format("Thrust: {0}",parent._bindings[MenuState.Thrust].Item2), parent._graphics.PreferredBackBufferHeight * .2f , x, biggest.X + buffer, parent._currentSelection == MenuState.Thrust);

        bottom = parent.drawMenuItem(parent._mainFont, string.Format("Rotate Right: {0}",parent._bindings[MenuState.RotateRight].Item2), bottom, x, biggest.X + buffer, parent._currentSelection == MenuState.RotateRight);
      
        bottom = parent.drawMenuItem(parent._mainFont, string.Format("Rotate Left: {0}",parent._bindings[MenuState.RotateLeft].Item2), bottom, x, biggest.X + buffer, parent._currentSelection == MenuState.RotateLeft);
      
        bottom = parent.drawMenuItem(parent._mainFont, string.Format("Pause: {0}",parent._bindings[MenuState.Pause].Item2), bottom, x, biggest.X + buffer, parent._currentSelection == MenuState.Pause);

        bottom = parent.drawMenuItem(parent._mainFont, "Select: Enter", bottom, x, biggest.X + buffer, false);

        bottom = parent.drawMenuItem(parent._mainFont, "Exit: Escape", bottom, x, biggest.X + buffer, false);

        parent._spriteBatch.End();
      }

      public void update(GameTime gameTime) {
        // no updates here
      }

      public void processInput(GameTime gameTime) {
        parent._keyboard.Update(gameTime);
      }
      
    }
  }

  public interface SettingState {
      public void render(GameTime gameTime);
      public void update(GameTime gameTime);
      public void processInput(GameTime gameTime);
  }
}