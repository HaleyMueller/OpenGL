using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenGL.App.Core
{
    public class Camera
    {
        //Default Camera Values
        const float YAW = -90.0f;
        const float PITCH = 0.0f;
        const float SPEED = 2.5f;
        const float SENSITIVITY = 0.1f;
        const float ZOOM = 45.0f;

        //Camera Attributes
        public Vector3 Position;
        public Vector3 Front = new Vector3(0, 0, -1f);
        public Vector3 Up = new Vector3(0, 1, 0);
        public Vector3 Right = new Vector3(1, 0, 0);
        public Vector3 WorldUp;

        //Euler Angles
        float Yaw;
        float Pitch;

        //Camera Options
        float MovementSpeed;
        float MouseSensitivity;
        float Zoom;

        public enum Camera_Movement
        {
            FORWARD,
            BACKWARD,
            LEFT,
            RIGHT
        };

        public Camera(Vector3 position, Vector3? worldUp, float yaw = YAW, float pitch = PITCH, float movementSpeed = SPEED, float mouseSensitivity = SENSITIVITY, float zoom = ZOOM)
        {
            Position = position;

            if (worldUp == null)
                WorldUp = new Vector3(0, 1, 0);
            else
                WorldUp = worldUp.Value;

            Yaw = yaw;
            Pitch = pitch;
            MovementSpeed = movementSpeed;
            MouseSensitivity = mouseSensitivity;
            Zoom = zoom;
        }

        public Matrix4 GetViewMatrix()
        {
            return Matrix4.LookAt(Position, Position + Front, Up);
        }

        public void ProcessKeyboard(Camera_Movement direction, float deltaTime)
        {
            float velocity = MovementSpeed * deltaTime;
            if (direction == Camera_Movement.FORWARD)
                Position += Front * velocity;
            if (direction == Camera_Movement.BACKWARD)
                Position -= Front * velocity;
            if (direction == Camera_Movement.LEFT)
                Position -= Right * velocity;
            if (direction == Camera_Movement.RIGHT)
                Position += Right * velocity;
        }

        // processes input received from a mouse input system. Expects the offset value in both the x and y direction.
        public void ProcessMouseMovement(float xoffset, float yoffset, bool constrainPitch = true)
        {
            xoffset *= MouseSensitivity;
            yoffset *= MouseSensitivity * -1;

            Yaw += xoffset;
            Pitch += yoffset;

            // make sure that when pitch is out of bounds, screen doesn't get flipped
            if (constrainPitch)
            {
                if (Pitch > 89.0f)
                    Pitch = 89.0f;
                if (Pitch < -89.0f)
                    Pitch = -89.0f;
            }

            // update Front, Right and Up Vectors using the updated Euler angles
            updateCameraVectors();
        }

        // processes input received from a mouse scroll-wheel event. Only requires input on the vertical wheel-axis
        public void ProcessMouseScroll(float yoffset)
        {
            Zoom -= (float)yoffset;
            if (Zoom < 1.0f)
                Zoom = 1.0f;
            if (Zoom > 45.0f)
                Zoom = 45.0f;
        }

        private void updateCameraVectors()
        {
            // calculate the new Front vector
            Vector3 front;
            front.X = (float)(MathHelper.Cos(MathHelper.DegreesToRadians(Yaw)) * MathHelper.Cos(MathHelper.DegreesToRadians(Pitch)));
            front.Y = (float)(MathHelper.Sin(MathHelper.DegreesToRadians(Pitch)));
            front.Z = (float)(MathHelper.Sin(MathHelper.DegreesToRadians(Yaw)) * MathHelper.Cos(MathHelper.DegreesToRadians(Pitch)));
            Front = Vector3.Normalize(front);
            // also re-calculate the Right and Up vector
            Right = Vector3.Normalize(Vector3.Cross(Front,WorldUp));  // normalize the vectors, because their length gets closer to 0 the more you look up or down which results in slower movement.
            Up = Vector3.Normalize(Vector3.Cross(Right, Front));
        }
    }
}
