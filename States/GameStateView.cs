using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;

namespace CS5410 {
  public abstract class GameStateView : IGameState {
    protected GraphicsDeviceManager graphics;
    protected BasicEffect effect;
    protected SpriteBatch spriteBatch;
    protected KeyboardInput keyboard;

    public void initialize(GraphicsDevice graphicsDevice, GraphicsDeviceManager graphics) {
      this.graphics = graphics;
      this.spriteBatch = new SpriteBatch(graphicsDevice);
      this.keyboard = new KeyboardInput();
    }
    
    public abstract void setupInput();

    public abstract void loadContent(ContentManager contentManager);

    public abstract void loadMusic(Song music);

    public abstract GameStateEnum processInput(GameTime gameTime);

    public abstract void render(GameTime gameTime);

    public abstract void update(GameTime gameTime);
  }
}