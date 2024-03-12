using System;
using Apedaile;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace CS5410 {
  public class GamePlayView : GameStateView {
    private GameStateEnum _nextState = GameStateEnum.GamePlay;
    private bool _waitforKeyRelease = true;
    private bool _continue = true;

    private Texture2D _background;
    private Rectangle _backRect;
    private Lander _player;
    private Terrain _terrain;

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

    public override void setupInput() {
      _keyboard.registerCommand(Keys.Escape, _waitforKeyRelease, new IInputDevice.CommandDelegate(exitState));
      _player.setupInput(_keyboard);
    }

    public void updateCommand(KeyboardInput keyboard, Keys key, bool keyPressOnly, IInputDevice.CommandDelegate callback) {
      keyboard.registerCommand(key, keyPressOnly, callback);
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
      _player.render(gameTime);
            
      _spriteBatch.End();

      foreach (EffectPass pass in _effect.CurrentTechnique.Passes)
      {
        pass.Apply();
        _graphics.GraphicsDevice.DrawUserIndexedPrimitives(
          PrimitiveType.TriangleStrip,
          _terrain.getStrip(), 0, _terrain.getStrip().Length,
          _terrain.getStripIndex(), 0, _terrain.getStrip().Length- 2);
      }
    }

    public override void update(GameTime gameTime) {
      _player.collision(_terrain.getStrip());
      _player.update(gameTime);
    }

    public override GameStateEnum processInput(GameTime gameTime) {
      _keyboard.Update(gameTime);
      if (_nextState != GameStateEnum.GamePlay) {
        GameStateEnum nextState = _nextState;
        _nextState = GameStateEnum.GamePlay;
        return nextState;
      }
      return GameStateEnum.GamePlay;
    }

    protected void exitState(GameTime gameTime, float value) {
      _nextState = GameStateEnum.MainMenu;
      _continue = true;
      _terrain.buildTerrain(2);
      _player.reset();
    }

    // These were development commands

    // protected void pauseState(GameTime gameTime, float value) {
    //   _continue = !_continue;
    // }

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

    // I'm too dumb to try and change this code in any meaningful way 
    // Wasted WAYYYYYY too much time on collision and all that (redacted)!
  }
}