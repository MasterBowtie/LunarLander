using Apedaile;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;

namespace CS5410 {
  public interface IGameState {
    void initialize(GraphicsDevice graphicsDevice, GraphicsDeviceManager graphics);

    void loadContent(ContentManager contentManager);

    void setupInput(Storage storage, KeyboardInput keyboard);

    void loadMusic(Song music);

    GameStateEnum processInput(GameTime gameTime);

    void update(GameTime gameTime);

    void render(GameTime gameTime);
  }
}