using Apedaile;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;


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
    private SpriteFont lgFont;

    private Texture2D selector;
    private Texture2D background;
    private Rectangle backRect;
    private Song music;
    private bool canPlayMusic = true;

    private MenuState currentSelection = MenuState.NewGame;
    private GameStateEnum nextState = GameStateEnum.MainMenu;
    private bool waitforKeyRelease = true;

    public override void setupInput(Storage storage, KeyboardInput keyboard)
    {
      this.keyboard = keyboard;
      storage.registerCommand(GameStateEnum.MainMenu, Keys.Up, waitforKeyRelease, Actions.up, new IInputDevice.CommandDelegate(moveUp));
      storage.registerCommand(GameStateEnum.MainMenu, Keys.Down, waitforKeyRelease, Actions.down, new IInputDevice.CommandDelegate(moveDown));
      storage.registerCommand(GameStateEnum.MainMenu, Keys.Enter, waitforKeyRelease, Actions.select, new IInputDevice.CommandDelegate(selectItem));
      storage.registerCommand(GameStateEnum.MainMenu, Keys.F1, waitforKeyRelease, Actions.pauseMusic, new IInputDevice.CommandDelegate(pauseMusic));
      storage.registerCommand(GameStateEnum.MainMenu, Keys.F2, waitforKeyRelease, Actions.playMusic, new IInputDevice.CommandDelegate(resumeMusic));
    }

    public override void loadContent(ContentManager contentManager){
      mainFont = contentManager.Load<SpriteFont>("Fonts/CourierPrime");
      lgFont = contentManager.Load<SpriteFont>("Fonts/CourierPrimeLg");
      selector = contentManager.Load<Texture2D>("Images/MenuSelector");
      background = contentManager.Load<Texture2D>("Images/background");
      backRect = new Rectangle(0, 0, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight);
    }

    public override void loadMusic(Song music) {
      this.music = music;
    }

    public override GameStateEnum processInput(GameTime gameTime){
      if (nextState != GameStateEnum.MainMenu) {
        GameStateEnum nextState = this.nextState;
        this.nextState = GameStateEnum.MainMenu;
        return nextState;
      }
      return GameStateEnum.MainMenu;
    }

    public override void update(GameTime gameTime){
      if (MediaPlayer.State == MediaState.Stopped) {
        MediaPlayer.Play(music);
      }
    }

    public override void render(GameTime gameTime){
      Vector2 biggest = mainFont.MeasureString("High Scores");
      int buffer = 50;
      float x = graphics.PreferredBackBufferWidth/2 - biggest.X/2 - buffer/2;
      
      spriteBatch.Begin();

      spriteBatch.Draw(background, backRect, Color.White);

      drawMenuItem(lgFont, "Lunar Lander", graphics.PreferredBackBufferHeight * .1f, graphics.PreferredBackBufferWidth/2 - lgFont.MeasureString("Lunar Lander").X/2, lgFont.MeasureString("Lunar Lander").X, false);

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

    public void pauseMusic(GameTime gameTime, float value) {
      MediaPlayer.Pause();
      System.Console.WriteLine("Music Paused");
    }

    public void resumeMusic(GameTime gameTime, float value) {
      MediaPlayer.Resume();
      System.Console.WriteLine("Music Resume");
    }
  }
}