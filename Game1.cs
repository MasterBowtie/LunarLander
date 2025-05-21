using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;
using CS5410;
using Apedaile;
using System;
using Microsoft.Xna.Framework.Media;

namespace LunarLander;

public class LunarLander : Game
{
  private GraphicsDeviceManager graphics;

  private IGameState currentState;
  private Dictionary<GameStateEnum, IGameState> states;
  private bool loading = false;
  private bool saving = false;
  public Storage storage = null;
  public KeyboardInput keyboard;

  public Song music;
  
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
      {GameStateEnum.HighScores, new ScoresView()},
      {GameStateEnum.About, new AboutView()}
    };

    // Give all game states a chance to initialize, other than constructor
    foreach(var item in states) {
      item.Value.initialize(this.GraphicsDevice, graphics);
    }
    ScoresView scoresView = (ScoresView)states[GameStateEnum.HighScores];

    var play = (GamePlayView)states[GameStateEnum.GamePlay];


    // Attach player to GamePlay and Settings
    var settings = (SettingsView)states[GameStateEnum.Settings];
    Lander player = new Lander();

    play.setupPlayer(player);
    play.attachScore(scoresView);
    settings.setupExtras(player, saveState);

    keyboard = new KeyboardInput();
    loadState();
    if (storage == null) {
      storage = new Storage(keyboard);
    } else {
      storage.attachKeyboard(keyboard);
    }

    // Give all game states a chance to initialize Inputs
    foreach(var item in states) {
      item.Value.setupInput(storage, keyboard);
    }
    
    storage.loadCommands();

    scoresView.setSave(saveState);

    // Start in the Main Menu
    currentState = states[GameStateEnum.MainMenu];

    base.Initialize();
  }

  protected override void LoadContent() {
    music = this.Content.Load<Song>("Sounds/Big_Eyes");
    // Load Content for all the game states
    foreach (var item in states) {
      item.Value.loadContent(this.Content);
      item.Value.loadMusic(music);
    }

    GamePlayView gameplay = (GamePlayView)states[GameStateEnum.GamePlay];
    gameplay.setupEffects();
  }

  protected override void Update(GameTime gameTime) {

    GameStateEnum nextStateEnum = currentState.processInput(gameTime);
    keyboard.Update(gameTime, nextStateEnum);

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
  
private void saveState() {
    lock (this) {
      if (!this.saving) {
        this.saving = true;
        finalizeSaveAsync(storage);
        }
      }
  }

    private async Task finalizeSaveAsync(Storage state) {
        await Task.Run(() => {
            using (IsolatedStorageFile storageFile = IsolatedStorageFile.GetUserStoreForApplication()) {
                try {
                    using (IsolatedStorageFileStream fs = storageFile.OpenFile("LunarLander.json", FileMode.Create)) {
                        if (fs != null) {
                            DataContractJsonSerializer mySerializer = new DataContractJsonSerializer(typeof(Storage));
                            mySerializer.WriteObject(fs, state);
                        }
                    }
                } catch(IsolatedStorageException err) {
                    System.Console.WriteLine("There was an error writing to storage\n{0}", err);
                }
            }
            this.saving = false;
        });
    }

    private void loadState() {
        lock (this) {
            if (!this.loading) {
                this.loading = true;
                var result = finalizeLoadAsync();
                result.Wait();
            }
        }
    }

    private async Task finalizeLoadAsync() {
        await Task.Run(() => {
           using (IsolatedStorageFile storageFile = IsolatedStorageFile.GetUserStoreForApplication()) {
            try {
                if (storageFile.FileExists("LunarLander.json")) {
                    using (IsolatedStorageFileStream fs = storageFile.OpenFile("LunarLander.json", FileMode.Open)) {
                        if (fs != null) {
                            DataContractJsonSerializer mySerializer = new DataContractJsonSerializer(typeof(Storage));
                            storage = (Storage)mySerializer.ReadObject(fs);
                        }
                    }
                }
            } catch (IsolatedStorageException err) {
                System.Console.WriteLine("Something broke: {0}", err);
            }
           } 
           this.loading = false;
        });
    }
  
}
