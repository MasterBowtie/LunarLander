using CS5410;
using Microsoft.Xna.Framework;
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

    protected Texture2D _image;
    protected Rectangle _rectangle;
    protected Vector2 _renderVector;
    protected SpriteFont _fontLg;
    protected SpriteFont _fontSm;

    private PlayerState _currentState;
    private Dictionary<PlayerStates, PlayerState> _states;

    private bool _waitforKeyRelease = false;

    private float _radius;

    private float _rotation = (float) Math.PI/2;
    private float _rotationRate = .001f;
    private float _fuel = 5;

    //position has to be a vector, since rectangle works in int and we need a constantly updated vector.
    private Vector2 _position = new Vector2(50, 50);

    // accelerators
    private Vector2 _momentum = new Vector2(0,0);
    private Vector2 _accelerate = new Vector2(0, -.001f);
    private Vector2 _gravity = new Vector2(0,.003f);

    protected override void setupStates() {
      _states = new Dictionary<PlayerStates, PlayerState>();
      _states.Add(PlayerStates.start, new Start(this));
      _states.Add(PlayerStates.playing, new Playing(this));
      _states.Add(PlayerStates.paused, new Pause(this));
      _states.Add(PlayerStates.win, new Win(this));
      _states.Add(PlayerStates.explode, new Explode(this));

      _currentState = _states[PlayerStates.start];
    }

    public override void setupInput(KeyboardInput keyboard){
      _keyboard = keyboard;
    }

    public void bindCommand(IInputDevice.CommandDelegate callback, Keys key, bool pause) {
      if (pause) {
        _keyboard.registerCommand(key, true, callback);
      }
      else {
        _keyboard.registerCommand(key, _waitforKeyRelease, callback);
      }
    }

    public override void loadContent(ContentManager contentManager) {
      _image = contentManager.Load<Texture2D>("Images/player");
      _renderVector = new Vector2(_image.Width/2, _image.Height/2);
      float width =  _graphics.PreferredBackBufferHeight/10;
      float height = (width/_image.Width) * _image.Height;
      // System.Console.WriteLine("{0}, {1}, {2}, {3}", width, _image.Width, height, _image.Height);
      _rectangle = new Rectangle((int)(_position.X + width/2), (int)(_position.Y + height/2), (int) width, (int) height);
      _radius = _rectangle.Height/2;
      _fontLg = contentManager.Load<SpriteFont>("Fonts/CourierPrimeLg");
      _fontSm = contentManager.Load<SpriteFont>("Fonts/CourierPrimeSm");
    }

    public override void processInput(GameTime gameTime){
      _keyboard.Update(gameTime);
    }

    public override void update(GameTime gameTime) {
      _currentState.update(gameTime);
    }

    public override void render(GameTime gameTime) {
       _currentState.render(gameTime);
       String rotationStr = String.Format("{0:0.00} deg", _rotation * 360/Math.PI);
        String fuelStr =    String.Format("{0:0.00} g", _fuel);
        String speedStr =   String.Format("{0:0.00} m/s", _momentum.Y * 6.666);
        Vector2 stringSize = _fontSm.MeasureString(rotationStr);
        int startY = _graphics.PreferredBackBufferHeight - (int)stringSize.Y * 3 ;
        _spriteBatch.DrawString(
          _fontSm,  
          rotationStr, 
          new Vector2(0, startY), 
          (-5f <= _rotation * 360/Math.PI && _rotation * 360/Math.PI <= 5)? Color.Green : Color.White
        );
        _spriteBatch.DrawString(
          _fontSm, 
          fuelStr, 
          new Vector2(0, startY + stringSize.Y), 
          (_fuel > 0)? Color.Green : Color.White
        );

        _spriteBatch.DrawString(
          _fontSm, 
          speedStr, 
          new Vector2(0, startY + stringSize.Y*2), 
          (_momentum.Y < .3f)? Color.Green :  Color.White
        );
    }

    public void rotateLeft(GameTime gameTime, float value) {
      if (_currentState == _states[PlayerStates.playing]) {
        _rotation -= _rotationRate * gameTime.ElapsedGameTime.Milliseconds * value;
      }
    }
    
    public void rotateRight(GameTime gameTime, float value) {
      if (_currentState == _states[PlayerStates.playing]) {  
      _rotation += _rotationRate * gameTime.ElapsedGameTime.Milliseconds * value;
      }
    }

    // Attempted to use Framework.Vector2 Methods, but unable to find Rotate
    // Ended up copying the code from the Monogame Git Repo
    public void moveForward(GameTime gameTime, float value) {
      if (_currentState == _states[PlayerStates.playing]) {
        float X = _accelerate.X * gameTime.ElapsedGameTime.Milliseconds;
        float Y = _accelerate.Y * gameTime.ElapsedGameTime.Milliseconds;

        float cos = MathF.Cos(_rotation);
        float sin = MathF.Sin(_rotation);


        X = X * cos - Y * sin;
        Y = _accelerate.X * sin + Y * cos;
        _fuel -=.01f;
        if (_fuel > 0) {
          _momentum = Vector2.Add(_momentum, new Vector2(X, Y));
        }
      }
    }

    public void pause(GameTime gameTime, float value) {
      if (_currentState == _states[PlayerStates.paused]) {
        _currentState = _states[PlayerStates.playing];
      } else {      

        _currentState = _states[PlayerStates.paused];
      }
    }

    public bool win() {
      return _currentState == _states[PlayerStates.win];
    }

    public void reset() {
      _rotation = (float) Math.PI/2;
      _position = new Vector2(50, 50);
      _momentum = new Vector2(0,0);
      _rectangle.X = (int)_position.X + _rectangle.Width/2;
      _rectangle.Y = (int)_position.Y + _rectangle.Height/2;
      _fuel = 5;
      _currentState = _states[PlayerStates.start];
    }

    public float getRadius() {
      return _radius;
    }

    public Vector2 getCenter() {
      // System.Console.WriteLine("{0},{1}",_position, _rectangle);
      // System.Console.WriteLine(_graphics.PreferredBackBufferHeight);
      return new Vector2(_position.X + _radius, _position.Y + _radius);
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

    public void collision(VertexPositionColor[] floor) {
      int index = 2;

      Vector2 player = getCenter();
      float radius = getRadius();
      
      while (index < floor.Length) {
        Vector3 pt1 = floor[index - 2].Position;
        Vector3 pt2 = floor[index].Position;
        if (testPoints(pt1, pt2, player, radius)) {
          if (pt2.Y == pt1.Y && -5f <= _rotation * 360/Math.PI && _rotation * 360/Math.PI <= 5 && _momentum.Y < .3f) {
            _currentState = _states[PlayerStates.win];
            return;
          }
          else{
            _currentState = _states[PlayerStates.explode];
            return;
          }
        }
        index += 2;
      }
    }

    protected class Start: PlayerState {
      Lander parent;
      TimeSpan countDown = new TimeSpan(0,0,3);
      public Start(Lander parent) {
        this.parent = parent;
      }

      public void render(GameTime gameTime) {
        Vector2 stringSize = parent._fontLg.MeasureString(string.Format("{0}", (int)countDown.TotalSeconds));
        parent._spriteBatch.DrawString(
        parent._fontLg, string.Format("{0}", (int)countDown.TotalSeconds + 1), new Vector2(parent._graphics.PreferredBackBufferWidth/2 - stringSize.X/2, parent._graphics.PreferredBackBufferHeight/2 - stringSize.Y/2), Color.White);

        parent._spriteBatch.Draw(
          parent._image, 
          parent._rectangle, 
          null,
          Color.White,
          parent._rotation,
          parent._renderVector,
          SpriteEffects.None,
          0
        );
      }
      public void update(GameTime gameTime) {
        countDown -= gameTime.ElapsedGameTime;
        if (countDown.TotalMilliseconds < 0) {
          countDown = new TimeSpan(0,0,3);
          parent._currentState = parent._states[PlayerStates.playing];
        }
      }
    }

    protected class Playing: PlayerState {
      Lander parent;
      public Playing(Lander parent) {
        this.parent = parent;
      }

      public void render(GameTime gameTime) {
        parent._spriteBatch.Draw(
          parent._image, 
          parent._rectangle, 
          null,
          Color.White,
          parent._rotation,
          parent._renderVector,
          SpriteEffects.None,
          0
        );
      }

      public void update(GameTime gameTime) {
        parent._momentum = Vector2.Add(parent._momentum, parent._gravity);
        parent._position = Vector2.Add(parent._position, parent._momentum);
        parent._rectangle.X = (int)parent._position.X + parent._rectangle.Width/2;
        parent._rectangle.Y = (int)parent._position.Y + parent._rectangle.Height/2;
      }
    }
  
    protected class Pause: PlayerState {
      Lander parent;
      
      public Pause(Lander parent) {
        this.parent = parent;
      }

      public void render(GameTime gameTime) {
        parent._spriteBatch.Draw(
          parent._image, 
          parent._rectangle, 
          null,
          Color.White,
          parent._rotation,
          parent._renderVector,
          SpriteEffects.None,
          0
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

        parent._spriteBatch.Draw(
          parent._image, 
          parent._rectangle, 
          null,
          Color.White,
          parent._rotation,
          parent._renderVector,
          SpriteEffects.None,
          0
        );
        
        String message = "You Win!";
        Vector2 stringSize = parent._fontLg.MeasureString(message);
        parent._spriteBatch.DrawString(
        parent._fontLg, message, new Vector2(parent._graphics.PreferredBackBufferWidth/2 - stringSize.X/2, parent._graphics.PreferredBackBufferHeight/2 - stringSize.Y/2), Color.Green);
      }
      public void update(GameTime gameTime) {

      }
      public void processInput(GameTime gameTime) {
        parent._keyboard.Update(gameTime);
      }

    }
  
    protected class Explode: PlayerState {
      Lander parent;

      public Explode(Lander parent) {
        this.parent = parent;
      }
      
      public void render(GameTime gameTime) {
        String message = "You Lose!";
        Vector2 stringSize = parent._fontLg.MeasureString(message);
        parent._spriteBatch.DrawString(
        parent._fontLg, message, new Vector2(parent._graphics.PreferredBackBufferWidth/2 - stringSize.X/2, parent._graphics.PreferredBackBufferHeight/2 - stringSize.Y/2), Color.Red);
      }
      
      public void update(GameTime gameTime) {

      }
    }
  
  }
}