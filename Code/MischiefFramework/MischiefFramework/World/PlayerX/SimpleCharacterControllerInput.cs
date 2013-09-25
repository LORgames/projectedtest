using BEPUphysics;
using Microsoft.Xna.Framework;
using MischiefFramework.Cache;
using MischiefFramework.Core;
using MischiefFramework.States;
using Microsoft.Xna.Framework.Media;

namespace MischiefFramework.World.PlayerX {
    /// <summary>
    /// Handles input and movement of a character in the game.
    /// Acts as a simple 'front end' for the bookkeeping and math of the character controller.
    /// </summary>
    internal class SimpleCharacterControllerInput : Asset {
        /// <summary>
        /// Current offset from the position of the character to the 'eyes.'
        /// </summary>
        public Vector3 CameraOffset = new Vector3(0, .7f, 0);

        /// <summary>
        /// Physics representation of the character.
        /// </summary>
        public SimpleCharacterController CharacterController;

        /// <summary>
        /// Whether or not to use the character controller's input.
        /// </summary>
        public bool IsActive = true;

        /// <summary>
        /// Owning space of the character.
        /// </summary>
        public Space Space;

        /// <summary>
        /// Constructs the character and internal physics character controller.
        /// </summary>
        /// <param name="owningSpace">Space to add the character to.</param>
        /// <param name="CameraToUse">Camera to attach to the character.</param>
        public SimpleCharacterControllerInput(Space owningSpace) {
            CharacterController = new SimpleCharacterController(Vector3.Up, 1.2f, 0.75f, 0.3f, 35.0f, true);

            Space = owningSpace;
            Space.Add(CharacterController);

            Deactivate();
            Activate();

            AssetManager.AddAsset(this);
        }

        /// <summary>
        /// Gives the character control over the Camera and movement input.
        /// </summary>
        public void Activate() {
            if (!IsActive) {
                IsActive = true;
                CharacterController.Activate();

                //CharacterController.Body.Position = Vector3.Right * TerrainController.HouseCellX * TerrainChunk.CELL_SIZE + Vector3.Backward * TerrainController.HouseCellZ * TerrainChunk.CELL_SIZE + Vector3.Up * 5;
                CharacterController.Body.Position = Vector3.Up;
                //CharacterController.Body.Position = (Renderer.CharacterCamera.Position);
            }
        }

        /// <summary>
        /// Returns input control to the Camera.
        /// </summary>
        public void Deactivate() {
            if (IsActive) {
                IsActive = false;
                CharacterController.Deactivate();
            }
        }


        /// <summary>
        /// Handles the input and movement of the character.
        /// </summary>
        /// <param name="dt">Time since last frame in simulation seconds.</param>
        /// <param name="previousKeyboardInput">The last frame's keyboard state.</param>
        /// <param name="keyboardInput">The current frame's keyboard state.</param>
        /// <param name="previousGamePadInput">The last frame's gamepad state.</param>
        /// <param name="gamePadInput">The current frame's keyboard state.</param>
        public override void Update(float dt) {
            if (IsActive) {
                //Note that the character controller's update method is not called here; this is because it is handled within its owning space.
                //This method's job is simply to tell the character to move around based on the Camera and input.

                //Puts the Camera at eye level.
                Renderer.CharacterCamera.Position = CharacterController.Body.BufferedStates.InterpolatedStates.Position + CameraOffset;
                Vector2 totalMovement = Vector2.Zero;

                /*Vector3 forward = CharacterController.Body.OrientationMatrix.Forward;
                forward.Y = 0;
                forward.Normalize();

                Vector3 right = CharacterController.Body.OrientationMatrix.Right;
                right.Y = 0;
                right.Normalize();*/

                Vector3 forward = Renderer.CharacterCamera.View.Left;
                forward.Y = 0;
                forward.Normalize();

                Vector3 right = Renderer.CharacterCamera.View.Forward;
                right.Y = 0;
                right.Normalize();

                totalMovement += Player.Input.GetY() * new Vector2(forward.X, forward.Z);
                totalMovement += Player.Input.GetX() * new Vector2(right.X, right.Z);

                if(totalMovement != Vector2.Zero) totalMovement.Normalize();

                CharacterController.MovementDirection = totalMovement;

                const float CAMERA_ZOOM = 50.0f;
                Renderer.CharacterCamera.LookAt = Player.playerCharacter.Body.Position;
                Renderer.CharacterCamera.Position.X = CAMERA_ZOOM * 0.612f + Renderer.CharacterCamera.LookAt.X;
                Renderer.CharacterCamera.Position.Y = CAMERA_ZOOM * 0.500f + Renderer.CharacterCamera.LookAt.Y;
                Renderer.CharacterCamera.Position.Z = CAMERA_ZOOM * 0.612f + Renderer.CharacterCamera.LookAt.Z;
                Renderer.CharacterCamera.GenerateMatrices();

                if (Player.Input.GetJump()) {
                    Player.playerCharacter.Jump();
                }
            }
        }
    }
}