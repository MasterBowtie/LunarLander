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

    private SpriteFont mainFont;
    private Texture2D selector;
    private Texture2D background;
    private Rectangle backRect;

    private MenuState currentSelection = MenuState.NewGame;
    private GameStateEnum nextState = GameStateEnum.MainMenu;
    private bool waitforKeyRelease = true;

    public override void setupInput()
    {
      keyboard.registerCommand(Keys.Up, waitforKeyRelease, new IInputDevice.CommandDelegate(moveUp));
      keyboard.registerCommand(Keys.Down, waitforKeyRelease, new IInputDevice.CommandDelegate(moveDown));
      keyboard.registerCommand(Keys.Enter, waitforKeyRelease, new IInputDevice.CommandDelegate(selectItem));
    }

    public override void loadContent(ContentManager contentManager){
      mainFont = contentManager.Load<SpriteFont>("Fonts/CourierPrime");
      selector = contentManager.Load<Texture2D>("Images/MenuSelector");
      background = contentManager.Load<Texture2D>("Images/earth_image");
      backRect = new Rectangle(graphics.PreferredBackBufferWidth - background.Width/4, 0, background.Width/4, background.Height/4);

    }

    public override GameStateEnum processInput(GameTime gameTime){
      keyboard.Update(gameTime);
      if (nextState != GameStateEnum.MainMenu) {
        GameStateEnum nextState = this.nextState;
        this.nextState = GameStateEnum.MainMenu;
        return nextState;
      }
      return GameStateEnum.MainMenu;
    }

    public override void update(GameTime gameTime){
      // Nothing here, move along, move along.
    }

    public override void render(GameTime gameTime){
      Vector2 biggest = mainFont.MeasureString("High Scores");
      int buffer = 50;
      float x = graphics.PreferredBackBufferWidth/2 - biggest.X/2 - buffer/2;
      
      spriteBatch.Begin();

      spriteBatch.Draw(background, backRect, Color.White);

      float bottom = drawMenuItem(mainFont, "New Game", graphics.PreferredBackBufferHeight * .4f , x, biggest.X + buffer, currentSelection == MenuState.NewGame);

      bottom = drawMenuItem(mainFont, "High Scores", bottom, x, biggest.X + buffer, currentSelection == MenuState.HighScores);
      
      bottom = drawMenuItem(mainFont, "Settings", bottom, x, biggest.X + buffer, currentSelection == MenuState.Settings);
      
      bottom = drawMenuItem(mainFont, "About", bottom, x, biggest.X + buffer, currentSelection == MenuState.About);

      bottom = drawMenuItem(mainFont, "Quit", bottom, x, biggest.X + buffer, currentSelection == MenuState.Quit);

      spriteBatch.End();
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
      if (currentSelection != MenuState.NewGame) {
        currentSelection = currentSelection - 1;
      }
    }

    public void moveDown(GameTime gameTime, float value) {
      if (currentSelection != MenuState.Quit) {
        currentSelection = currentSelection + 1;
      }
    }

    public void selectItem(GameTime gameTime, float value) {
      switch (currentSelection) {
        case MenuState.NewGame: {
          nextState = GameStateEnum.GamePlay;
          break;
        }
        case MenuState.HighScores: {
          nextState = GameStateEnum.HighScores;
          break;
        }
        case MenuState.Settings: {
          nextState = GameStateEnum.Settings;
          break;
        }
        case MenuState.About: {
          nextState = GameStateEnum.About;
          break;
        }
        case MenuState.Quit: {
          nextState = GameStateEnum.Exit;
          break;
        }
      }
    }
  }
}