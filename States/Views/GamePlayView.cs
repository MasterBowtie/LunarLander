using System;
using CS5410;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Apedaile {
  public class GamePlayView : GameStateView {
    private GameStateEnum nextState = GameStateEnum.GamePlay;
    private bool waitforKeyRelease = true;

    private Texture2D background;
    private Rectangle backRect;

    private ParticleSystem particleSystem;
    private ParticleSystemRenderer particleRender;

    private Lander player;
    private Terrain terrain;
    private float landingRatio = .15f;
    private ushort level = 1;
    private ScoresView score; 

    private Song music;


    public void setupEffects()
    {
    effect = new BasicEffect(graphics.GraphicsDevice)
      {
        VertexColorEnabled = true,
        View = Matrix.CreateLookAt(new Vector3(0, 0, 1), Vector3.Zero, Vector3.Up),
        Projection = Matrix.CreateOrthographicOffCenter(
          0, graphics.GraphicsDevice.Viewport.Width,
          graphics.GraphicsDevice.Viewport.Height, 0,   // doing this to get it to match the default of upper left of (0, 0)
          0.1f, 2)
       };

      graphics.GraphicsDevice.RasterizerState = new RasterizerState
      {
        FillMode = FillMode.Solid,
        CullMode = CullMode.CullClockwiseFace,   // CullMode.None If you want to not worry about triangle winding order
        MultiSampleAntiAlias = true,
      };
      
    }

    public void attachScore(ScoresView score) {
      this.score = score;
    }

    public override void setupInput() {
      keyboard.registerCommand(Keys.Escape, waitforKeyRelease, new IInputDevice.CommandDelegate(exitState));
      keyboard.registerCommand(Keys.Enter, waitforKeyRelease, new IInputDevice.CommandDelegate(nextLevel));
      keyboard.registerCommand(Keys.F1, waitforKeyRelease, pauseMusic);
      keyboard.registerCommand(Keys.F2, waitforKeyRelease, resumeMusic);
      // player.setupInput(keyboard);
    }

    public void setupPlayer(Lander player) {
      this.particleSystem = new ParticleSystem(
                (int)(graphics.PreferredBackBufferHeight * .01f) , (int)(graphics.PreferredBackBufferHeight * .005f),
                0.12f, 0.05f,
                2000, 500, (float)Math.PI / 8);
      this.player = player;
      this.player.initialize(graphics, spriteBatch);
      this.terrain = new Terrain(0, graphics.PreferredBackBufferWidth, (int) (graphics.PreferredBackBufferHeight * .4f), graphics.PreferredBackBufferHeight, .15f);
      this.player.attachSystems(particleSystem, terrain);
    }

    public override void loadContent(ContentManager contentManager) {
      particleRender = new ParticleSystemRenderer("Images/particle");
      background = contentManager.Load<Texture2D>("Images/1968_Earthrise");
      float ratio = background.Height * graphics.PreferredBackBufferWidth / background.Width;

      backRect = new Rectangle(0, 0, graphics.PreferredBackBufferWidth, (int)ratio);
      
      particleRender.LoadContent(contentManager);
      player.loadContent(contentManager);
      terrain.buildTerrain(2);
    }

    public override void loadMusic(Song music) {
      this.music = music;
    }

        public override void render(GameTime gameTime) {
      spriteBatch.Begin();
      spriteBatch.Draw(background, backRect, Color.White);
      spriteBatch.End();
      
      particleRender.draw(spriteBatch, particleSystem);
      terrain.render(effect, graphics);

      spriteBatch.Begin();
      player.render(gameTime);
      spriteBatch.End();
    }

    public override void update(GameTime gameTime) {
      particleSystem.update(gameTime);
      player.update(gameTime);
      if (MediaPlayer.State == MediaState.Stopped) {
        MediaPlayer.Play(music);
      }
    }

    public override GameStateEnum processInput(GameTime gameTime) {
      keyboard.Update(gameTime);
      player.processInput(gameTime);
      if (nextState != GameStateEnum.GamePlay) {
        GameStateEnum newState = this.nextState;
        this.nextState = GameStateEnum.GamePlay;
        return newState;
      }
      return GameStateEnum.GamePlay;
    }

    protected void exitState(GameTime gameTime, float value) {
      score.saveScore(player.getScore(), (ushort)(level - 1));
      nextState = GameStateEnum.MainMenu;
      level = 1;
      terrain.setZoneWidth(landingRatio - level * .02f, player.getRadius());
      terrain.buildTerrain(2);
      player.reset(true);

    }

    protected void nextLevel(GameTime gameTime, float value) {
      if (player.win()) {
        level += 1;
        terrain.setZoneWidth(landingRatio - level * .02f, player.getRadius()*2);
        terrain.buildTerrain(1);
        player.reset(false);
      }
    }

    private void pauseMusic(GameTime gameTime, float value) {
      MediaPlayer.Pause();
      System.Console.WriteLine("Music Paused");
    }

    private void resumeMusic(GameTime gameTime, float value) {
      MediaPlayer.Resume();
      System.Console.WriteLine("Music Resume");
    }

    // These were development commands

    // protected void buildTerrain(GameTime gameTime, float value) {
    //   terrain.buildTerrain(2);
    // }
    
    // protected void increasePass(GameTime gameTime, float value) {
    //   terrain.setPass(terrain.getPass() + 1);
    // }
    
    // protected void decreasePass(GameTime gameTime, float value) {
    //   terrain.setPass(terrain.getPass() - 1);
    // }

    // protected void increaseRough(GameTime gameTime, float value) {
    //   terrain.setRough(terrain.getRough() + .01f);
    // }
    
    // protected void decreaseRough(GameTime gameTime, float value) {
    //   terrain.setRough(terrain.getRough() - .01f);
    // }
  }
}