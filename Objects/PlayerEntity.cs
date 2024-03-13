using CS5410;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace Apedaile {
  public abstract class PlayerEntity : IEntity {
    protected GraphicsDeviceManager _graphics;
    protected SpriteBatch _spriteBatch;
    protected KeyboardInput _keyboard;

    public void initialize(GraphicsDeviceManager graphics, SpriteBatch spriteBatch) {
      _graphics = graphics;
      _spriteBatch = spriteBatch;
      setupStates();
      _keyboard = new KeyboardInput();
    }

    protected abstract void setupStates();

    public abstract void setupInput(KeyboardInput keyboard);

    public abstract void processInput(GameTime gameTime);

    public abstract void loadContent(ContentManager contentManager);

    public abstract void update(GameTime gameTime);

    public abstract void render(GameTime gameTime);

    protected interface PlayerState {
      public void render(GameTime gameTime);
      public void update(GameTime gameTime);
    }
  }
}