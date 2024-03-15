using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using CS5410;
using Apedaile;

namespace LunarLander;

public class LunarLander : Game
{
  private GraphicsDeviceManager _graphics;
  private KeyboardInput _keyboard;

  private IGameState _currentState;
  private Dictionary<GameStateEnum, IGameState> _states;
  
  public LunarLander() {
    _graphics = new GraphicsDeviceManager(this);
    Content.RootDirectory = "Content";
    IsMouseVisible = true;
  }
  
  protected override void Initialize() {
    // _graphics.PreferredBackBufferWidth = 1920;
    // _graphics.PreferredBackBufferHeight = 1080;
    // _graphics.ApplyChanges();

    _graphics.GraphicsDevice.RasterizerState = new RasterizerState
      {
        FillMode = FillMode.Solid,
        CullMode = CullMode.None,   // CullMode.None If you want to not worry about triangle winding order
        MultiSampleAntiAlias = true,
      };
  
    _states = new Dictionary<GameStateEnum, IGameState> {
      {GameStateEnum.MainMenu, new MainMenuView()},
      {GameStateEnum.GamePlay, new GamePlayView()},
      {GameStateEnum.Settings, new SettingsView()},
      {GameStateEnum.HighScores, new ScoresView()}
    };

    // Give all game states a chance to initialize, other than constructor
    foreach(var item in _states) {
      item.Value.initialize(this.GraphicsDevice, _graphics);
    }
    var scores = (ScoresView)_states[GameStateEnum.HighScores];
    var play = (GamePlayView)_states[GameStateEnum.GamePlay];

    // Attach player to GamePlay and Settings
    var settings = (SettingsView)_states[GameStateEnum.Settings];
    Lander player = new Lander();

    play.setupPlayer(player);
    play.attachScore(scores);
    settings.setupPlayer(player);

    // Give all game states a chance to initialize Inputs
    foreach(var item in _states) {
      item.Value.setupInput();
    }



    // Start in the Main Menu
    _currentState = _states[GameStateEnum.MainMenu];

    base.Initialize();
  }

  protected override void LoadContent() {
    // Load Content for all the game states
    foreach (var item in _states) {
      item.Value.loadContent(this.Content);
    }
    GamePlayView gameplay = (GamePlayView)_states[GameStateEnum.GamePlay];
    gameplay.setupEffects();
  }

  protected override void Update(GameTime gameTime) {

    GameStateEnum nextStateEnum = _currentState.processInput(gameTime);

    // Special Case for exiting Game
    if (nextStateEnum == GameStateEnum.Exit) {
      Exit();
    }
    else {
      _currentState.update(gameTime);
      _currentState = _states[nextStateEnum];
      
    }

    base.Update(gameTime);
  }


  protected override void Draw(GameTime gameTime) {
    GraphicsDevice.Clear(Color.CornflowerBlue);
    _currentState.render(gameTime);
    base.Draw(gameTime);
  }
}
