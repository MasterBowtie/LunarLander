using System;
using CS5410;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Apedaile {
  public class GamePlayView : GameStateView {
    private GameStateEnum _nextState = GameStateEnum.GamePlay;
    private bool _waitforKeyRelease = true;

    private Texture2D _background;
    private Rectangle _backRect;
    private Lander _player;
    private Terrain _terrain;
    private float _landingRatio = .15f;
    private ushort _level = 1;
    private ScoresView _score; 

    public void setupEffects()
    {
    _effect = new BasicEffect(_graphics.GraphicsDevice)
      {
        VertexColorEnabled = true,
        View = Matrix.CreateLookAt(new Vector3(0, 0, 1), Vector3.Zero, Vector3.Up),
        Projection = Matrix.CreateOrthographicOffCenter(
          0, _graphics.GraphicsDevice.Viewport.Width,
          _graphics.GraphicsDevice.Viewport.Height, 0,   // doing this to get it to match the default of upper left of (0, 0)
          0.1f, 2)
       };

      _graphics.GraphicsDevice.RasterizerState = new RasterizerState
      {
        FillMode = FillMode.Solid,
        CullMode = CullMode.CullClockwiseFace,   // CullMode.None If you want to not worry about triangle winding order
        MultiSampleAntiAlias = true,
      };
    }

    public void attachScore(ScoresView score) {
      _score = score;
    }

    public override void setupInput() {
      _keyboard.registerCommand(Keys.Escape, _waitforKeyRelease, new IInputDevice.CommandDelegate(exitState));
      _keyboard.registerCommand(Keys.Enter, _waitforKeyRelease, new IInputDevice.CommandDelegate(nextLevel));
      // _player.setupInput(_keyboard);
    }

    public void setupPlayer(Lander player) {
      _player = player;
      _player.initialize(_graphics, _spriteBatch);
      _terrain = new Terrain(0, _graphics.PreferredBackBufferWidth, (int) (_graphics.PreferredBackBufferHeight * .4f), _graphics.PreferredBackBufferHeight, .15f);
    }

    public override void loadContent(ContentManager contentManager) {
      _background = contentManager.Load<Texture2D>("Images/earth_image");
      _backRect = new Rectangle(_graphics.PreferredBackBufferWidth - _background.Width/4, 0, _background.Width/4, _background.Height/4);
      _player.loadContent(contentManager);
      _terrain.buildTerrain(2);
    }

    public override void render(GameTime gameTime) {
      _spriteBatch.Begin();
      _spriteBatch.Draw(_background, _backRect, Color.White);
      _spriteBatch.End();

      _terrain.render(_effect, _graphics);

      _spriteBatch.Begin();
      _player.render(gameTime);
      _spriteBatch.End();
    }

    public override void update(GameTime gameTime) {
      _player.collision(_terrain.getStrip());
      _player.update(gameTime);
    }

    public override GameStateEnum processInput(GameTime gameTime) {
      _keyboard.Update(gameTime);
      _player.processInput(gameTime);
      if (_nextState != GameStateEnum.GamePlay) {
        GameStateEnum nextState = _nextState;
        _nextState = GameStateEnum.GamePlay;
        return nextState;
      }
      return GameStateEnum.GamePlay;
    }

    protected void exitState(GameTime gameTime, float value) {
      _score.saveScore(_player.getScore(), (ushort)(_level - 1));
      _nextState = GameStateEnum.MainMenu;
      _level = 1;
      _terrain.setZoneWidth(_landingRatio - _level * .02f, _player.getRadius());
      _terrain.buildTerrain(2);
      _player.reset(true);

    }

    protected void nextLevel(GameTime gameTime, float value) {
      if (_player.win()) {
        _level += 1;
        _terrain.setZoneWidth(_landingRatio - _level * .02f, _player.getRadius()*2);
        _terrain.buildTerrain(1);
        _player.reset(false);
      }
    }

    // These were development commands

    // protected void buildTerrain(GameTime gameTime, float value) {
    //   _terrain.buildTerrain(2);
    // }
    
    // protected void increasePass(GameTime gameTime, float value) {
    //   _terrain.setPass(_terrain.getPass() + 1);
    // }
    
    // protected void decreasePass(GameTime gameTime, float value) {
    //   _terrain.setPass(_terrain.getPass() - 1);
    // }

    // protected void increaseRough(GameTime gameTime, float value) {
    //   _terrain.setRough(_terrain.getRough() + .01f);
    // }
    
    // protected void decreaseRough(GameTime gameTime, float value) {
    //   _terrain.setRough(_terrain.getRough() - .01f);
    // }
  }
}