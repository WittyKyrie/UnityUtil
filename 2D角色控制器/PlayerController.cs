using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerController : MonoBehaviour, IPlayerController {
    // Public for external hooks
    public Vector3 Velocity { get; private set; }
    public FrameInput Input { get; private set; }
    public bool JumpingThisFrame { get; private set; }
    public bool LandingThisFrame { get; private set; }
    public Vector3 RawMovement { get; private set; }
    public bool Grounded { get; private set; }

    private Vector3 _lastPosition;
    private float _currentHorizontalSpeed, _currentVerticalSpeed;

    // This is horrible, but for some reason colliders are not fully established when update starts...
    private bool _active;
    private void Awake() => Invoke(nameof(Activate), 0.5f);
    private void Activate() =>  _active = true;
        
    private void Update() {
        if(!_active) return;
        // Calculate velocity
        var position = transform.position;
        Velocity = (position - _lastPosition) / Time.deltaTime;
        _lastPosition = position;

        GatherInput();
        RunCollisionChecks();

        CalculateWalk(); // Horizontal movement
        CalculateJumpApex(); // Affects fall speed, so calculate before gravity
        CalculateGravity(); // Vertical movement
        CalculateJump(); // Possibly overrides vertical

        MoveCharacter(); // Actually perform the axis movement
    }


    #region Gather Input

    private void GatherInput() {
        Input = new FrameInput {
            JumpDown = UnityEngine.Input.GetButtonDown("Jump"),
            JumpUp = UnityEngine.Input.GetButtonUp("Jump"),
            X = UnityEngine.Input.GetAxisRaw("Horizontal")
        };
        if (Input.JumpDown) {
            _lastJumpPressed = Time.time;
        }
    }

    #endregion

    #region Collisions

    [Header("COLLISION")] [SerializeField] private Bounds characterBounds;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private int detectorCount = 3;
    [SerializeField] private float detectionRayLength = 0.1f;
    [SerializeField] [Range(0.1f, 0.3f)] private float rayBuffer = 0.1f; // Prevents side detectors hitting the ground

    private RayRange _raysUp, _raysRight, _raysDown, _raysLeft;
    private bool _colUp, _colRight, _colLeft;

    private float _timeLeftGrounded;

    // We use these raycast checks for pre-collision information
    private void RunCollisionChecks() {
        // Generate ray ranges. 
        CalculateRayRanged();

        // Ground
        LandingThisFrame = false;
        var groundedCheck = RunDetection(_raysDown);
        if (Grounded && !groundedCheck) _timeLeftGrounded = Time.time; // Only trigger when first leaving
        else if (!Grounded && groundedCheck) {
            _coyoteUsable = true; // Only trigger when first touching
            LandingThisFrame = true;
        }

        Grounded = groundedCheck;

        // The rest
        _colUp = RunDetection(_raysUp);
        _colLeft = RunDetection(_raysLeft);
        _colRight = RunDetection(_raysRight);

        //判断是否有一个点是属于ground的
        bool RunDetection(RayRange range) {
            return EvaluateRayPositions(range).Any(point => Physics2D.Raycast(point, range.Dir, detectionRayLength, groundLayer));
        }
    }

    private void CalculateRayRanged() {
        // This is crying out for some kind of refactor. 
        var b = new Bounds(transform.position, characterBounds.size);

        _raysDown = new RayRange(b.min.x + rayBuffer, b.min.y, b.max.x - rayBuffer, b.min.y, Vector2.down);
        _raysUp = new RayRange(b.min.x + rayBuffer, b.max.y, b.max.x - rayBuffer, b.max.y, Vector2.up);
        _raysLeft = new RayRange(b.min.x, b.min.y + rayBuffer, b.min.x, b.max.y - rayBuffer, Vector2.left);
        _raysRight = new RayRange(b.max.x, b.min.y + rayBuffer, b.max.x, b.max.y - rayBuffer, Vector2.right);
    }


    private IEnumerable<Vector2> EvaluateRayPositions(RayRange range) {
        for (var i = 0; i < detectorCount; i++) {
            var t = (float)i / (detectorCount - 1);
            yield return Vector2.Lerp(range.Start, range.End, t);
        }
    }

    private void OnDrawGizmos() {
        // Bounds
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position + characterBounds.center, characterBounds.size);

        // Rays
        if (!Application.isPlaying) {
            CalculateRayRanged();
            Gizmos.color = Color.blue;
            foreach (var range in new List<RayRange> { _raysUp, _raysRight, _raysDown, _raysLeft }) {
                foreach (var point in EvaluateRayPositions(range)) {
                    Gizmos.DrawRay(point, range.Dir * detectionRayLength);
                }
            }
        }

        if (!Application.isPlaying) return;

        // Draw the future position. Handy for visualizing gravity
        Gizmos.color = Color.red;
        var move = new Vector3(_currentHorizontalSpeed, _currentVerticalSpeed) * Time.deltaTime;
        Gizmos.DrawWireCube(transform.position + move, characterBounds.size);
    }

    #endregion
    
    #region Walk

    [FormerlySerializedAs("_acceleration")] [Header("WALKING")] [SerializeField] private float acceleration = 90;
    [FormerlySerializedAs("_moveClamp")] [SerializeField] private float moveClamp = 13;
    [FormerlySerializedAs("_deAcceleration")] [SerializeField] private float deAcceleration = 60f;
    [FormerlySerializedAs("_apexBonus")] [SerializeField] private float apexBonus = 2;

    private void CalculateWalk() {
        if (Input.X != 0) {
            // Set horizontal move speed
            _currentHorizontalSpeed += Input.X * acceleration * Time.deltaTime;

            // clamped by max frame movement
            _currentHorizontalSpeed = Mathf.Clamp(_currentHorizontalSpeed, -moveClamp, moveClamp);

            // Apply bonus at the apex of a jump
            var apexPoint = Mathf.Sign(Input.X) * this.apexBonus * _apexPoint;
            _currentHorizontalSpeed += apexPoint * Time.deltaTime;
        }
        else {
            // No input. Let's slow the character down
            _currentHorizontalSpeed = Mathf.MoveTowards(_currentHorizontalSpeed, 0, deAcceleration * Time.deltaTime);
        }

        if (_currentHorizontalSpeed > 0 && _colRight || _currentHorizontalSpeed < 0 && _colLeft) {
            // Don't walk through walls
            _currentHorizontalSpeed = 0;
        }
    }

    #endregion

    #region Gravity

    [FormerlySerializedAs("_fallClamp")] [Header("GRAVITY")] [SerializeField] private float fallClamp = -40f;
    [FormerlySerializedAs("_minFallSpeed")] [SerializeField] private float minFallSpeed = 80f;
    [FormerlySerializedAs("_maxFallSpeed")] [SerializeField] private float maxFallSpeed = 120f;
    private float _fallSpeed;

    private void CalculateGravity() {
        if (Grounded) {
            // Move out of the ground
            if (_currentVerticalSpeed < 0) _currentVerticalSpeed = 0;
        }
        else {
            // Add downward force while ascending if we ended the jump early
            var fallSpeed = _endedJumpEarly && _currentVerticalSpeed > 0 ? _fallSpeed * jumpEndEarlyGravityModifier : _fallSpeed;

            // Fall
            _currentVerticalSpeed -= fallSpeed * Time.deltaTime;

            // Clamp
            if (_currentVerticalSpeed < fallClamp) _currentVerticalSpeed = fallClamp;
        }
    }

    #endregion

    #region Jump

    [FormerlySerializedAs("_jumpHeight")] [Header("JUMPING")] [SerializeField] private float jumpHeight = 30;
    [FormerlySerializedAs("_jumpApexThreshold")] [SerializeField] private float jumpApexThreshold = 10f;
    [FormerlySerializedAs("_coyoteTimeThreshold")] [SerializeField] private float coyoteTimeThreshold = 0.1f;
    [FormerlySerializedAs("_jumpBuffer")] [SerializeField] private float jumpBuffer = 0.1f;
    [FormerlySerializedAs("_jumpEndEarlyGravityModifier")] [SerializeField] private float jumpEndEarlyGravityModifier = 3;
    private bool _coyoteUsable;
    private bool _endedJumpEarly = true;
    private float _apexPoint; // Becomes 1 at the apex of a jump
    private float _lastJumpPressed;
    private bool CanUseCoyote => _coyoteUsable && !Grounded && _timeLeftGrounded + coyoteTimeThreshold > Time.time;
    private bool HasBufferedJump => Grounded && _lastJumpPressed + jumpBuffer > Time.time;

    private void CalculateJumpApex() {
        if (!Grounded) {
            // Gets stronger the closer to the top of the jump
            _apexPoint = Mathf.InverseLerp(jumpApexThreshold, 0, Mathf.Abs(Velocity.y));
            _fallSpeed = Mathf.Lerp(minFallSpeed, maxFallSpeed, _apexPoint);
        }
        else {
            _apexPoint = 0;
        }
    }

    private void CalculateJump() {
        // Jump if: grounded or within coyote threshold || sufficient jump buffer
        if (Input.JumpDown && CanUseCoyote || HasBufferedJump) {
            _currentVerticalSpeed = jumpHeight;
            _endedJumpEarly = false;
            _coyoteUsable = false;
            _timeLeftGrounded = float.MinValue;
            JumpingThisFrame = true;
        }
        else {
            JumpingThisFrame = false;
        }

        // End the jump early if button released
        if (!Grounded && Input.JumpUp && !_endedJumpEarly && Velocity.y > 0) {
            // _currentVerticalSpeed = 0;
            _endedJumpEarly = true;
        }

        if (_colUp) {
            if (_currentVerticalSpeed > 0) _currentVerticalSpeed = 0;
        }
    }

    #endregion

    #region Move

    [FormerlySerializedAs("_freeColliderIterations")] [Header("MOVE")] [SerializeField, Tooltip("Raising this value increases collision accuracy at the cost of performance.")]
    private int freeColliderIterations = 10;

    // We cast our bounds before moving to avoid future collisions
    private void MoveCharacter() {
        var pos = transform.position;
        RawMovement = new Vector3(_currentHorizontalSpeed, _currentVerticalSpeed); // Used externally
        var move = RawMovement * Time.deltaTime;
        var furthestPoint = pos + move;

        // check furthest movement. If nothing hit, move and don't do extra checks
        var hit = Physics2D.OverlapBox(furthestPoint, characterBounds.size, 0, groundLayer);
        if (!hit) {
            transform.position += move;
            return;
        }

        // otherwise increment away from current pos; see what closest position we can move to
        var positionToMoveTo = transform.position;
        for (int i = 1; i < freeColliderIterations; i++) {
            // increment to check all but furthestPoint - we did that already
            var t = (float)i / freeColliderIterations;
            var posToTry = Vector2.Lerp(pos, furthestPoint, t);

            if (Physics2D.OverlapBox(posToTry, characterBounds.size, 0, groundLayer)) {
                transform.position = positionToMoveTo;

                // We've landed on a corner or hit our head on a ledge. Nudge the player gently
                if (i == 1) {
                    if (_currentVerticalSpeed < 0) _currentVerticalSpeed = 0;
                    var transform1 = transform;
                    var position = transform1.position;
                    var dir = position - hit.transform.position;
                    position += dir.normalized * move.magnitude;
                    transform1.position = position;
                }

                return;
            }

            positionToMoveTo = posToTry;
        }
    }

    #endregion
}