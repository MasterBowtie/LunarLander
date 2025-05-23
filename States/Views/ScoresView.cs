using System;
using CS5410;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Media;

namespace Apedaile
{
  public class ScoresView : GameStateView
  {

    private GameStateEnum nextState = GameStateEnum.HighScores;
    private SpriteFont mainFont;
    private SpriteFont titleFont;
    private Texture2D background;
    private Rectangle backRect;
    private Song music;
    private Storage storage;
    private SaveScore save;

    public void setSave(SaveScore saveScore) {
      this.save = saveScore;
    }

    public override void loadContent(ContentManager contentManager)
    {
      mainFont = contentManager.Load<SpriteFont>("Fonts/CourierPrime");
      titleFont = contentManager.Load<SpriteFont>("Fonts/CourierPrimeLg");
      background = contentManager.Load<Texture2D>("Images/earth_image");
      backRect = new Rectangle(graphics.PreferredBackBufferWidth - background.Width/4, 0, background.Width/4, background.Height/4);
    }
    
    public override void loadMusic(Song music) {
      this.music = music;
    }

    public override GameStateEnum processInput(GameTime gameTime) {
      if (nextState != GameStateEnum.HighScores)
      {
        GameStateEnum nextState = this.nextState;
        this.nextState = GameStateEnum.HighScores;
        return nextState;
      }
      return GameStateEnum.HighScores;
    }

    public override void render(GameTime gameTime) {
      String message;
      Vector2 stringSize;
      spriteBatch.Begin();
      spriteBatch.Draw(background, backRect, Color.White);

      message = "High Scores";
      stringSize = titleFont.MeasureString(message);
      float bottom = stringSize.Y + graphics.PreferredBackBufferHeight * .2f;
      spriteBatch.DrawString(
        titleFont, "High Scores", new Vector2(graphics.PreferredBackBufferWidth / 2 - stringSize.X / 2, graphics.PreferredBackBufferHeight * .1f), Color.White);

      if (storage != null) {
        foreach (var item in storage.HighScores) {
          message = String.Format("Level {0}: {1}", item.Item2, item.Item1);
          bottom = drawMenuItem(mainFont, message, bottom);
        }
      }
      else {
        message = "Loading";
        stringSize = mainFont.MeasureString(message);
        spriteBatch.DrawString(
        mainFont,message, new Vector2(graphics.PreferredBackBufferWidth / 2 - stringSize.X / 2, graphics.PreferredBackBufferHeight/2 - stringSize.Y/2), Color.White);
      }
      spriteBatch.End();
    }

    public override void setupInput(Storage storage, KeyboardInput keyboard) {
      this.keyboard = keyboard;
      this.storage = storage;
      storage.registerCommand(GameStateEnum.HighScores, Keys.Escape, true, Actions.exit, exitState);
      storage.registerCommand(GameStateEnum.HighScores, Keys.F1, true, Actions.pauseMusic, pauseMusic);
      storage.registerCommand(GameStateEnum.HighScores, Keys.F2, true, Actions.playMusic, resumeMusic);
    }

    public override void update(GameTime gameTime) {
      if (MediaPlayer.State == MediaState.Stopped) {
        MediaPlayer.Play(music);
      }
    }

    private float drawMenuItem(SpriteFont font, string text, float y)
    {
      Vector2 stringSize = font.MeasureString(text);
      spriteBatch.DrawString(
        font, text, new Vector2(graphics.PreferredBackBufferWidth * .3f, y), Color.White);

      return y + stringSize.Y;
    }

    private void exitState(GameTime gameTime, float value) {
      nextState = GameStateEnum.MainMenu;
    }

    public void saveScore(uint score, ushort level) {
      if (storage != null) {
        storage.submitScore(score, level);
      } 
      save();
    }

    private void pauseMusic(GameTime gameTime, float value) {
      MediaPlayer.Pause();
      System.Console.WriteLine("Music Paused");
    }

    private void resumeMusic(GameTime gameTime, float value) {
      MediaPlayer.Resume();
      System.Console.WriteLine("Music Resume");
    }
  }

  public delegate void SaveScore();
}