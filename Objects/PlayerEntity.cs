using CS5410;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace Apedaile {
  public abstract class PlayerEntity : IEntity {
    protected GraphicsDeviceManager graphics;
    protected SpriteBatch spriteBatch;
    protected KeyboardInput keyboard;

    public void initialize(GraphicsDeviceManager graphics, SpriteBatch spriteBatch) {
      this.graphics = graphics;
      this.spriteBatch = spriteBatch;
      setupStates();
      keyboard = new KeyboardInput();
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