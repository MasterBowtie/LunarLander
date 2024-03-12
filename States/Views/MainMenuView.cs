using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
 

namespace CS5410 {
  public class MainMenuView : GameStateView {
    private enum MenuState {
      NewGame,
      HighScores,
      Settings,
      About,
      Quit,
    }

    private SpriteFont _mainFont;
    private Texture2D _selector;

    private MenuState _currentSelection = MenuState.NewGame;
    private GameStateEnum _nextState = GameStateEnum.MainMenu;
    private bool _waitforKeyRelease = true;

    public override void setupInput()
    {
      _keyboard.registerCommand(Keys.Up, _waitforKeyRelease, new IInputDevice.CommandDelegate(moveUp));
      _keyboard.registerCommand(Keys.Down, _waitforKeyRelease, new IInputDevice.CommandDelegate(moveDown));
      _keyboard.registerCommand(Keys.Enter, _waitforKeyRelease, new IInputDevice.CommandDelegate(selectItem));
    }

    public override void loadContent(ContentManager contentManager){
      _mainFont = contentManager.Load<SpriteFont>("Fonts/CourierPrime");
      _selector = contentManager.Load<Texture2D>("Images/MenuSelector");

    }

    public override GameStateEnum processInput(GameTime gameTime){
      _keyboard.Update(gameTime);
      if (_nextState != GameStateEnum.MainMenu) {
        GameStateEnum nextState = _nextState;
        _nextState = GameStateEnum.MainMenu;
        return nextState;
      }
      return GameStateEnum.MainMenu;
    }

    public override void update(GameTime gameTime){
      // Nothing here, move along, move along.
    }

    public override void render(GameTime gameTime){
      Vector2 biggest = _mainFont.MeasureString("High Scores");
      int buffer = 50;
      float x = _graphics.PreferredBackBufferWidth/2 - biggest.X/2 - buffer/2;
      
      _spriteBatch.Begin();

      float bottom = drawMenuItem(_mainFont, "New Game", _graphics.PreferredBackBufferHeight * .4f , x, biggest.X + buffer, _currentSelection == MenuState.NewGame);

      bottom = drawMenuItem(_mainFont, "High Scores", bottom, x, biggest.X + buffer, _currentSelection == MenuState.HighScores);
      
      bottom = drawMenuItem(_mainFont, "Settings", bottom, x, biggest.X + buffer, _currentSelection == MenuState.Settings);
      
      bottom = drawMenuItem(_mainFont, "About", bottom, x, biggest.X + buffer, _currentSelection == MenuState.About);

      bottom = drawMenuItem(_mainFont, "Quit", bottom, x, biggest.X + buffer, _currentSelection == MenuState.Quit);

      _spriteBatch.End();
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
      if (_currentSelection != MenuState.NewGame) {
        _currentSelection = _currentSelection - 1;
      }
    }

    public void moveDown(GameTime gameTime, float value) {
      if (_currentSelection != MenuState.Quit) {
        _currentSelection = _currentSelection + 1;
      }
    }

    public void selectItem(GameTime gameTime, float value) {
      switch (_currentSelection) {
        case MenuState.NewGame: {
          _nextState = GameStateEnum.GamePlay;
          break;
        }
        case MenuState.HighScores: {
          _nextState = GameStateEnum.HighScores;
          break;
        }
        case MenuState.Settings: {
          _nextState = GameStateEnum.Settings;
          break;
        }
        case MenuState.About: {
          _nextState = GameStateEnum.About;
          break;
        }
        case MenuState.Quit: {
          _nextState = GameStateEnum.Exit;
          break;
        }
      }
    }
  }
}