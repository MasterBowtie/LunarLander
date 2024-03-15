using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using CS5410;
using Apedaile;

namespace LunarLander;

public class LunarLander : Game
{
  private GraphicsDeviceManager graphics;

  private IGameState currentState;
  private Dictionary<GameStateEnum, IGameState> states;
  
  public LunarLander() {
    graphics = new GraphicsDeviceManager(this);
    Content.RootDirectory = "Content";
    IsMouseVisible = true;
  }
  
  protected override void Initialize() {

    // This size is WAY too big for my computer font sizes were build for such.
    // Font sizes will be small for the given larger screen.
    // graphics.PreferredBackBufferWidth = 1920;
    // graphics.PreferredBackBufferHeight = 1080;
    // graphics.ApplyChanges();

    graphics.GraphicsDevice.RasterizerState = new RasterizerState
      {
        FillMode = FillMode.Solid,
        CullMode = CullMode.None,   // CullMode.None If you want to not worry about triangle winding order
        MultiSampleAntiAlias = true,
      };
  
    states = new Dictionary<GameStateEnum, IGameState> {
      {GameStateEnum.MainMenu, new MainMenuView()},
      {GameStateEnum.GamePlay, new GamePlayView()},
      {GameStateEnum.Settings, new SettingsView()},
      {GameStateEnum.HighScores, new ScoresView()}
    };

    // Give all game states a chance to initialize, other than constructor
    foreach(var item in states) {
      item.Value.initialize(this.GraphicsDevice, graphics);
    }
    var scores = (ScoresView)states[GameStateEnum.HighScores];
    var play = (GamePlayView)states[GameStateEnum.GamePlay];

    // Attach player to GamePlay and Settings
    var settings = (SettingsView)states[GameStateEnum.Settings];
    Lander player = new Lander();

    play.setupPlayer(player);
    play.attachScore(scores);
    settings.setupPlayer(player);

    // Give all game states a chance to initialize Inputs
    foreach(var item in states) {
      item.Value.setupInput();
    }



    // Start in the Main Menu
    currentState = states[GameStateEnum.MainMenu];

    base.Initialize();
  }

  protected override void LoadContent() {
    // Load Content for all the game states
    foreach (var item in states) {
      item.Value.loadContent(this.Content);
    }
    GamePlayView gameplay = (GamePlayView)states[GameStateEnum.GamePlay];
    gameplay.setupEffects();
  }

  protected override void Update(GameTime gameTime) {

    GameStateEnum nextStateEnum = currentState.processInput(gameTime);

    // Special Case for exiting Game
    if (nextStateEnum == GameStateEnum.Exit) {
      Exit();
    }
    else {
      currentState.update(gameTime);
      currentState = states[nextStateEnum];
      
    }

    base.Update(gameTime);
  }


  protected override void Draw(GameTime gameTime) {
    GraphicsDevice.Clear(Color.Black);
    currentState.render(gameTime);
    base.Draw(gameTime);
  }
}
