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
  public Scores scores = null;
  public KeyBindings keyBindings = null;

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
    settings.setupPlayer(player);

    // Give all game states a chance to initialize Inputs
    foreach(var item in states) {
      item.Value.setupInput();
    }      
    lock (this) {
      if (!this.saving){
        this.saving = true;
        var result = finalizeLoadAsync();
        result.Wait();
      }
    }
    scoresView.setScores(scores, finalizeScoreSaveAsync);
    settings.attachBindings(keyBindings, finalizeBindSaveAsync);

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

  private async Task finalizeScoreSaveAsync(Scores state)
    {
      await Task.Run(() =>
      {
        using (IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication())
        {
          try
          {
            using (IsolatedStorageFileStream fs = storage.OpenFile("LunarLander.json", FileMode.Create))
            {
              if (fs != null)
              {
                DataContractJsonSerializer mySerializer = new DataContractJsonSerializer(typeof(Scores));
                mySerializer.WriteObject(fs, state);
              }
            }
          }
          catch (IsolatedStorageException)
          {
            // Ideally show something to the user, but this is demo code :)
          }
        }

        this.saving = false;
      });
    }

  private async Task finalizeBindSaveAsync(KeyBindings state)
    {
      await Task.Run(() =>
      {
        using (IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication())
        {
          try
          {
            using (IsolatedStorageFileStream fs = storage.OpenFile("LunarLanderBinds.json", FileMode.Create))
            {
              if (fs != null)
              {
                DataContractJsonSerializer mySerializer = new DataContractJsonSerializer(typeof(KeyBindings));
                mySerializer.WriteObject(fs, state);
              }
            }
          }
          catch (IsolatedStorageException)
          {
            // Ideally show something to the user, but this is demo code :)
          }
        }

        this.saving = false;
      });
    }

  private async Task finalizeLoadAsync()
    {
      await Task.Run(() =>
      {
        using (IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication())
        {
          try
          {
            if (storage.FileExists("LunarLander.json"))
            {
              using (IsolatedStorageFileStream fs = storage.OpenFile("LunarLander.json", FileMode.Open))
              {
                if (fs != null)
                {
                  DataContractJsonSerializer mySerializer = new DataContractJsonSerializer(typeof(Scores));
                  scores = (Scores)mySerializer.ReadObject(fs);
                }
              }
            }
          }
          catch (IsolatedStorageException)
          {
            System.Console.WriteLine("This file doesn't exist yet");
            // Ideally show something to the user, but this is demo code :)
          }
        }

        this.loading = false;
      });
      await Task.Run(() =>
      {
        using (IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication())
        {
          try
          {
            if (storage.FileExists("LunarLanderBinds.json"))
            {
              using (IsolatedStorageFileStream fs = storage.OpenFile("LunarLanderBinds.json", FileMode.Open))
              {
                if (fs != null)
                {
                  DataContractJsonSerializer mySerializer = new DataContractJsonSerializer(typeof(KeyBindings));
                  keyBindings = (KeyBindings)mySerializer.ReadObject(fs);
                }
              }
            }
          }
          catch (IsolatedStorageException)
          {
            System.Console.WriteLine("This file doesn't exist yet");
            // Ideally show something to the user, but this is demo code :)
          }
        }

        this.loading = false;
      });
    }
}
