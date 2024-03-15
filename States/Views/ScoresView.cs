using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;
using CS5410;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Apedaile
{
  public class ScoresView : GameStateView
  {

    private GameStateEnum _nextState = GameStateEnum.HighScores;
    private SpriteFont _mainFont;
    private SpriteFont _titleFont;
    private Scores scores = null;
    private bool loading = false;
    private bool saving = false;

    public override void loadContent(ContentManager contentManager)
    {
      _mainFont = contentManager.Load<SpriteFont>("Fonts/CourierPrime");
      _titleFont = contentManager.Load<SpriteFont>("Fonts/CourierPrimeLg");
      lock (this)
      {
        if (!this.loading)
        {
          this.loading = true;
          var result = this.finalizeLoadAsync();
          result.Wait();
        }
      }
    }

    public override GameStateEnum processInput(GameTime gameTime) {
      _keyboard.Update(gameTime);
      if (_nextState != GameStateEnum.HighScores)
      {
        GameStateEnum nextState = _nextState;
        _nextState = GameStateEnum.HighScores;
        return nextState;
      }
      return GameStateEnum.HighScores;
    }

    public override void render(GameTime gameTime) {
      String message;
      Vector2 stringSize;
      _spriteBatch.Begin();

      message = "High Scores";
      stringSize = _titleFont.MeasureString(message);
      float bottom = stringSize.Y + _graphics.PreferredBackBufferHeight * .2f;
      _spriteBatch.DrawString(
        _titleFont, "High Scores", new Vector2(_graphics.PreferredBackBufferWidth / 2 - stringSize.X / 2, _graphics.PreferredBackBufferHeight * .1f), Color.White);

      if (scores != null) {
        foreach (var item in scores.HighScores) {
          message = String.Format("Level {0}: {1}", item.Item2, item.Item1);
          bottom = drawMenuItem(_mainFont, message, bottom);
        }
      }
      else {
        message = "Loading";
        stringSize = _mainFont.MeasureString(message);
        _spriteBatch.DrawString(
        _mainFont,message, new Vector2(_graphics.PreferredBackBufferWidth / 2 - stringSize.X / 2, _graphics.PreferredBackBufferHeight/2 - stringSize.Y/2), Color.White);
      }
      _spriteBatch.End();
    }

    public override void setupInput() {
      _keyboard.registerCommand(Keys.Escape, true, exitState);
    }

    public override void update(GameTime gameTime) {
      // System.Console.WriteLine("Pending");
    }

    private float drawMenuItem(SpriteFont font, string text, float y)
    {
      Vector2 stringSize = font.MeasureString(text);
      _spriteBatch.DrawString(
        font, text, new Vector2(_graphics.PreferredBackBufferWidth * .3f, y), Color.White);

      return y + stringSize.Y;
    }

    private void exitState(GameTime gameTime, float value) {
      _nextState = GameStateEnum.MainMenu;
    }

    public void saveScore(uint score, ushort level) {
      if (scores != null) {
        scores.submitScore(score, level);
      } else {
        scores = new Scores();
        scores.submitScore(score, level);
        System.Console.WriteLine("New Scores");
      }
      lock (this)
      {
        if (!this.saving)
        {
          this.saving = true;
          finalizeSaveAsync(scores);
        }
      }
    }

    private async Task finalizeSaveAsync(Scores state)
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
    }
  }
}