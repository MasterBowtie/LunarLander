using CS5410;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;



namespace Apedaile {
  public class Lander : PlayerEntity {
    private enum PlayerStates {
      start,
      playing,
      paused,
      win,
      explode
    }

    protected Texture2D image;
    protected Rectangle rectangle;
    protected Texture2D menuBG;
    protected Rectangle menuRt;
    protected Vector2 renderVector;
    protected SpriteFont fontLg;
    protected SpriteFont fontSm;

    protected SoundEffect thrust;
    protected TimeSpan thrustDur;
    protected SoundEffect explosion;


    protected ParticleSystem particleSystem;
    protected Terrain terrain;

    private PlayerState currentState;
    private Dictionary<PlayerStates, PlayerState> states;

    private bool waitforKeyRelease = false;

    private float radius;

    private float rotation = (float) Math.PI/2;
    private float rotationRate = .001f;
    private float fuel = 5;

    //position has to be a vector, since rectangle works in int and we need a constantly updated vector.
    private Vector2 position = new Vector2(50, 50);

    // accelerators
    private Vector2 momentum = new Vector2(0,0);
    private Vector2 accelerate = new Vector2(0, -.001f);
    private Vector2 gravity = new Vector2(0,.003f);

    private uint score = 0;

    public void attachSystems(ParticleSystem particleSystem, Terrain terrain) {
      this.particleSystem = particleSystem;
      this.terrain = terrain;
    }

    protected override void setupStates() {
      states = new Dictionary<PlayerStates, PlayerState>();
      states.Add(PlayerStates.start, new Start(this));
      states.Add(PlayerStates.playing, new Playing(this));
      states.Add(PlayerStates.paused, new Pause(this));
      states.Add(PlayerStates.win, new Win(this));
      states.Add(PlayerStates.explode, new Explode(this));

      currentState = states[PlayerStates.start];
    }

    public override void setupInput(KeyboardInput keyboard){
      this.keyboard = keyboard;
    }

    public override void loadContent(ContentManager contentManager) {
      image = contentManager.Load<Texture2D>("Images/player");
      menuBG = contentManager.Load<Texture2D>("Images/menu");
      menuRt = new Rectangle(0,0,10,10);
      
      renderVector = new Vector2(image.Width/2, image.Height/2);
      float width =  graphics.PreferredBackBufferHeight/10;
      float height = width/image.Width * image.Height;
      // System.Console.WriteLine("{0}, {1}, {2}, {3}", width, image.Width, height, image.Height);
      rectangle = new Rectangle((int)(position.X + width/2), (int)(position.Y + height/2), (int) width, (int) height);
      radius = rectangle.Height/2;
      
      fontLg = contentManager.Load<SpriteFont>("Fonts/CourierPrimeLg");
      fontSm = contentManager.Load<SpriteFont>("Fonts/CourierPrimeSm");

      thrust = contentManager.Load<SoundEffect>("Sounds/rocketthrust");
      thrustDur = thrust.Duration;
      explosion = contentManager.Load<SoundEffect>("Sounds/hq-explosion");
    }

    public override void update(GameTime gameTime) {
      currentState.update(gameTime);
      thrustDur -= gameTime.ElapsedGameTime;
    }

    public override void render(GameTime gameTime) {
       currentState.render(gameTime);

       
       String rotationStr = String.Format("{0:0.00} deg", rotation * 180/Math.PI);
      String fuelStr = String.Format("{0:0.00} L", fuel);
      String speedStr = String.Format("{0:0.00} m/s", momentum.Length() * 6.666);
      Vector2 stringSize = fontSm.MeasureString(rotationStr);
      float buffer = graphics.PreferredBackBufferWidth * .02f;
      menuRt.Y = 0;
      menuRt.X = (int)(graphics.PreferredBackBufferWidth * .8f);
      menuRt.Height = (int)((int)(buffer * 2) + (int)stringSize.Y * 3);
      menuRt.Width = (int)(graphics.PreferredBackBufferWidth * .2f);
      spriteBatch.Draw(menuBG, menuRt, Color.White);


      int startY = (int)buffer;
        spriteBatch.DrawString(
          fontSm,  
          fuelStr, 
          new Vector2(graphics.PreferredBackBufferWidth * .8f + buffer, startY), 
          (-5f <= rotation * 180/Math.PI && rotation * 180/Math.PI <= 5)? Color.LawnGreen : Color.White
        );
        spriteBatch.DrawString(
          fontSm, 
          rotationStr, 
          new Vector2(graphics.PreferredBackBufferWidth * .8f + buffer, startY + stringSize.Y), 
          (fuel > 0)? Color.LawnGreen : Color.White
        );

        spriteBatch.DrawString(
          fontSm, 
          speedStr, 
          new Vector2(graphics.PreferredBackBufferWidth * .8f + buffer, startY + stringSize.Y*2), 
          (momentum.Length() < .3f)? Color.LawnGreen :  Color.White
        );
    }

    public void rotateLeft(GameTime gameTime, float value) {
      if (currentState == states[PlayerStates.playing]) {
        rotation -= rotationRate * gameTime.ElapsedGameTime.Milliseconds * value;
      }
      if (rotation < -Math.PI) {
        rotation += (float)(Math.PI * 2);
      }
    }
    
    public void rotateRight(GameTime gameTime, float value) {
      if (currentState == states[PlayerStates.playing]) {  
      rotation += rotationRate * gameTime.ElapsedGameTime.Milliseconds * value;
      }
      if (rotation > Math.PI) {
        rotation -= (float)(Math.PI * 2);
      }
    }

    // Attempted to use Framework.Vector2 Methods, but unable to find Rotate
    // Ended up copying the code from the Monogame Git Repo
    public void moveForward(GameTime gameTime, float value) {
      if (currentState == states[PlayerStates.playing]) {
        
        if (thrustDur.TotalMilliseconds <= 0) {
          thrust.Play();
          thrustDur = thrust.Duration;
        }
        float X = accelerate.X * gameTime.ElapsedGameTime.Milliseconds * value;
        float Y = accelerate.Y * gameTime.ElapsedGameTime.Milliseconds * value;

        float cos = MathF.Cos(rotation);
        float sin = MathF.Sin(rotation);


        X = X * cos - Y * sin;
        Y = accelerate.X * sin + Y * cos;
        if (fuel > 0) {
          fuel -=.01f;
          momentum = Vector2.Add(momentum, new Vector2(X, Y));
          float x = (float)(Math.Cos(rotation + Math.PI/2) * (rectangle.Height * .25)) + getCenter().X;
          float y = (float)(Math.Sin(rotation + Math.PI/2) * (rectangle.Height * .25)) + getCenter().Y;

          particleSystem.create(new Vector2(x, y), (float)(rotation + Math.PI/2));
        }
      }
    }

    private void explode() {
      explosion.Play();
      for (int i = 0; i < 360 ; i += 10) {
        particleSystem.create(getCenter(), (float)(i/(2 *Math.PI)));
      }
    }

    public void pause(GameTime gameTime, float value) {
      if (currentState == states[PlayerStates.paused]) {
        currentState = states[PlayerStates.playing];
      } else {      

        currentState = states[PlayerStates.paused];
      }
    }

    public bool win() {
      return currentState == states[PlayerStates.win];
    }

    public void reset(bool reset) {
      rotation = (float) Math.PI/2;
      position = new Vector2(50, 50);
      momentum = new Vector2(0,0);
      rectangle.X = (int)position.X + rectangle.Width/2;
      rectangle.Y = (int)position.Y + rectangle.Height/2;
      fuel = 5;
      if (reset) {
        score = 0;
      }
      Explode explode = (Explode)states[PlayerStates.explode];
      explode.countDown = new TimeSpan(0,0,3);
      currentState = states[PlayerStates.start];
    }

    public float getRadius() {
      return radius;
    }

    public Vector2 getCenter() {
      // System.Console.WriteLine("{0},{1}",position, rectangle);
      // System.Console.WriteLine(graphics.PreferredBackBufferHeight);
      return new Vector2(position.X + radius, position.Y + radius);
    }

    protected bool testPoints(Vector3 pt1, Vector3 pt2, Vector2 player, float radius) {
      Vector2 v1 = new Vector2( pt2.X - pt1.X, pt2.Y - pt1.Y );
      Vector2 v2 = new Vector2( pt1.X - player.X, pt1.Y - player.Y );
      double b = -2 * (v1.X * v2.X + v1.Y * v2.Y);
      double c =  2 * (v1.X * v1.X + v1.Y * v1.Y);
      double d = b * b - 2 * c * (v2.X * v2.X + v2.Y * v2.Y - radius * radius);
      if (d < 0) { // no intercept
          return false;
      }
      d = Math.Sqrt(d);
      // These represent the unit distance of point one and two on the line
      double u1 = (b - d) / c;  
      double u2 = (b + d) / c;
      if (u1 <= 1 && u1 >= 0) {  // If point on the line segment
          return true;
      }
      if (u2 <= 1 && u2 >= 0) {  // If point on the line segment
          return true;
      }
      return false;
    }

    public void collision() {
      int index = 2;

      Vector2 player = getCenter();
      float radius = getRadius();
      if (player.X + radius < 0 || player.X - radius > graphics.PreferredBackBufferWidth || player.Y + radius < 0) {
        explode();
        currentState = states[PlayerStates.explode];
      }
      
      while (index < terrain.getStrip().Length) {
        Vector3 pt1 = terrain.getStrip()[index - 2].Position;
        Vector3 pt2 = terrain.getStrip()[index].Position;
        if (testPoints(pt1, pt2, player, radius)) {
          if (pt2.Y == pt1.Y && -5f <= rotation * 180/Math.PI && rotation * 360/Math.PI <= 5 && momentum.Length() < .3f) {
            updateScore();
            currentState = states[PlayerStates.win];
            return;
          }
          else{
            explode();
            currentState = states[PlayerStates.explode];
            return;
          }
        }
        index += 2;
      }
    }

    protected void updateScore(){
        int fuelScore = (int) (50 * fuel);
        int rotationScore =  50 - (int)(Math.Abs(rotation) * 180/Math.PI / 5 * 50);
        int velocityScore = 100 - (int)(momentum.Length() * 6.666 * 50);

      score += (uint)(fuelScore + rotationScore + velocityScore);
    }

    public uint getScore(){
      return score;
    }

    protected class Start: PlayerState {
      Lander parent;
      TimeSpan countDown = new TimeSpan(0,0,3);
      public Start(Lander parent) {
        this.parent = parent;
      }

      public void render(GameTime gameTime) {
        Vector2 stringSize = parent.fontLg.MeasureString(string.Format("{0}", (int)countDown.TotalSeconds));
        float buffer = parent.graphics.PreferredBackBufferWidth * .02f;
        parent.menuRt.X = (int)(parent.graphics.PreferredBackBufferWidth/2 - stringSize.X/2 - buffer);
        parent.menuRt.Y = (int)(parent.graphics.PreferredBackBufferHeight/3 - stringSize.Y/2 - buffer);
        parent.menuRt.Height = (int)(stringSize.Y + (buffer));
        parent.menuRt.Width = (int)(stringSize.X + (buffer));
        parent.spriteBatch.Draw(parent.menuBG, parent.menuRt, Color.White);

        parent.spriteBatch.DrawString(
        parent.fontLg, string.Format("{0}", (int)countDown.TotalSeconds + 1), new Vector2(parent.graphics.PreferredBackBufferWidth/2 - stringSize.X/2, parent.graphics.PreferredBackBufferHeight/3 - stringSize.Y/2), Color.White);

        parent.spriteBatch.Draw(
          parent.image, 
          parent.rectangle, 
          null,
          Color.White,
          parent.rotation,
          parent.renderVector,
          SpriteEffects.None,
          0
        );
      }
      public void update(GameTime gameTime) {
        countDown -= gameTime.ElapsedGameTime;
        if (countDown.TotalMilliseconds < 0) {
          countDown = new TimeSpan(0,0,3);
          parent.currentState = parent.states[PlayerStates.playing];
        }
      }
    }

    protected class Playing: PlayerState {
      Lander parent;
      public Playing(Lander parent) {
        this.parent = parent;
      }

      public void render(GameTime gameTime) {
        parent.spriteBatch.Draw(
          parent.image, 
          parent.rectangle, 
          null,
          Color.White,
          parent.rotation,
          parent.renderVector,
          SpriteEffects.None,
          0
        );
      }

      public void update(GameTime gameTime) {
        parent.momentum = Vector2.Add(parent.momentum, parent.gravity);
        parent.position = Vector2.Add(parent.position, parent.momentum);
        parent.rectangle.X = (int)parent.position.X + parent.rectangle.Width/2;
        parent.rectangle.Y = (int)parent.position.Y + parent.rectangle.Height/2;
        parent.collision();
      }
    }
  
    protected class Pause: PlayerState {
      Lander parent;
      
      public Pause(Lander parent) {
        this.parent = parent;
      }

      public void render(GameTime gameTime) {
        parent.spriteBatch.Draw(
          parent.image, 
          parent.rectangle, 
          null,
          Color.White,
          parent.rotation,
          parent.renderVector,
          SpriteEffects.None,
          0
        );
        Vector2 stringSize = parent.fontLg.MeasureString("Paused");
        float buffer = parent.graphics.PreferredBackBufferWidth * .02f;
        parent.menuRt.X = (int)(parent.graphics.PreferredBackBufferWidth/2 - stringSize.X/2 - buffer);
        parent.menuRt.Y = (int)(parent.graphics.PreferredBackBufferHeight/2 - stringSize.Y/2 - buffer);
        parent.menuRt.Height = (int)(stringSize.Y + (buffer * 2));
        parent.menuRt.Width = (int)(stringSize.X + (buffer * 2));
        parent.spriteBatch.Draw(parent.menuBG, parent.menuRt, Color.White);
        parent.spriteBatch.DrawString(
          parent.fontLg, 
          "Paused", 
          new Vector2(parent.graphics.PreferredBackBufferWidth/2 - stringSize.X/2, parent.graphics.PreferredBackBufferHeight/2 - stringSize.Y/2), 
          Color.White
        );
      }

      public void update(GameTime gameTime) {
        // No updates here
      }
    }
  
    protected class Win: PlayerState {
      Lander parent;

      public Win(Lander parent) {
        this.parent = parent;
      }
      public void render(GameTime gameTime) {

        parent.spriteBatch.Draw(
          parent.image, 
          parent.rectangle, 
          null,
          Color.White,
          parent.rotation,
          parent.renderVector,
          SpriteEffects.None,
          0
        );
        
        
        String message = "You Win!";
        Vector2 stringSize = parent.fontLg.MeasureString(message);
        float bottom = parent.graphics.PreferredBackBufferHeight/4 + stringSize.Y/2;
        int fuelScore = (int) (50 * parent.fuel);
        int rotationScore =  50 - (int)(Math.Abs(parent.rotation) * 180/Math.PI / 5 * 50);
        int velocityScore = 100 - (int)(parent.momentum.Length() * 6.666 * 50);
        String message2 = String.Format("Fuel:{1}\nRotation:{2}\nVelocity:{3}\nTotal Score: {0}", (uint)(fuelScore + rotationScore + velocityScore), fuelScore, rotationScore, velocityScore);
        Vector2 stringSize2 = parent.fontSm.MeasureString(message);
        
        float buffer = parent.graphics.PreferredBackBufferWidth * .05f;
        parent.menuRt.X = (int)(parent.graphics.PreferredBackBufferWidth/2 - stringSize.X/2 - buffer);
        parent.menuRt.Y = (int)(parent.graphics.PreferredBackBufferHeight/4 - stringSize2.Y - buffer);
        parent.menuRt.Height = (int)(stringSize.Y + stringSize2.Y * 4 + buffer);
        parent.menuRt.Width = (int)(stringSize.X + buffer);
        parent.spriteBatch.Draw(parent.menuBG, parent.menuRt, Color.White);

        parent.spriteBatch.DrawString(
        parent.fontLg, message, new Vector2(parent.graphics.PreferredBackBufferWidth/2 - stringSize.X/2, parent.graphics.PreferredBackBufferHeight/4 - stringSize.Y/2), Color.White);
        parent.spriteBatch.DrawString(
        parent.fontSm, message2, new Vector2(parent.graphics.PreferredBackBufferWidth/2 - stringSize.X/2, bottom), Color.White);
      
      }
      public void update(GameTime gameTime) {

      }
      public void processInput(GameTime gameTime) {
        parent.keyboard.Update(gameTime, GameStateEnum.GamePlay);
      }

    }
  
    protected class Explode: PlayerState {
      Lander parent;
      public TimeSpan countDown = new TimeSpan(0,0,3); 

      public Explode(Lander parent) {
        this.parent = parent;
      }
      
      public void render(GameTime gameTime) {
        if (countDown.TotalMilliseconds < 0) { 
        String message = "You Lose!";
        Vector2 stringSize = parent.fontLg.MeasureString(message);
        float bottom = parent.graphics.PreferredBackBufferHeight/4 + stringSize.Y/2;
        String message2 = String.Format("Score: {0}", parent.score);
        Vector2 stringSize2 = parent.fontLg.MeasureString(message2);
        
        float buffer = parent.graphics.PreferredBackBufferWidth * .05f;
        parent.menuRt.X = (int)(parent.graphics.PreferredBackBufferWidth/2 - stringSize2.X/2 - buffer);
        parent.menuRt.Y = (int)(parent.graphics.PreferredBackBufferHeight/3 - stringSize.Y/2 - buffer);
        parent.menuRt.Height = (int)(stringSize.Y + stringSize2.Y + (buffer * 2));
        parent.menuRt.Width = (int)(stringSize2.X + (buffer * 2));
        parent.spriteBatch.Draw(parent.menuBG, parent.menuRt, Color.White);
        
        parent.spriteBatch.DrawString(
        parent.fontLg, message, new Vector2(parent.graphics.PreferredBackBufferWidth/2 - stringSize.X/2, parent.graphics.PreferredBackBufferHeight/3 - stringSize.Y/2), Color.Red);
        parent.spriteBatch.DrawString(
        parent.fontLg, message2, new Vector2(parent.graphics.PreferredBackBufferWidth/2 - stringSize2.X/2, bottom + buffer), Color.White);
      }
      }
      
      public void update(GameTime gameTime) {
        countDown -= gameTime.ElapsedGameTime;
      }
    }
  
  }
}