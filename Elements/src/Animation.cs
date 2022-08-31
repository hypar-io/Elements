using System;
using System.Collections.Generic;
using System.Numerics;

namespace Elements
{
    /// <summary>
    /// The animation of a transform expressed using glTF convention.
    /// </summary>
    public class Animation
    {
        readonly List<byte> _scaleTimes = new List<byte>();
        readonly List<byte> _scales = new List<byte>();
        readonly List<byte> _translationTimes = new List<byte>();
        readonly List<byte> _translations = new List<byte>();
        readonly List<byte> _rotationTimes = new List<byte>();
        readonly List<byte> _rotations = new List<byte>();
        readonly float[] _scaleMin;
        readonly float[] _scaleMax;
        float _scaleTimeMin;
        float _scaleTimeMax;
        readonly float[] _translationMin;
        readonly float[] _translationMax;
        float _translationTimeMin;
        float _translationTimeMax;
        readonly float[] _rotationMin;
        readonly float[] _rotationMax;
        float _rotationTimeMin;
        float _rotationTimeMax;

        /// <summary>
        /// An array of bytes representing the scale keys.
        /// </summary>
        public byte[] Scales => _scales.ToArray();

        /// <summary>
        /// An array of bytes representing the scale time keys.
        /// </summary>
        public byte[] ScaleTimes => _scaleTimes.ToArray();

        /// <summary>
        /// The minimum scale time.
        /// </summary>
        public float ScaleTimeMin => _scaleTimeMin;

        /// <summary>
        /// The maximum scale time.
        /// </summary>
        public float ScaleTimeMax => _scaleTimeMax;

        /// <summary>
        /// The minimum scale as [x,y,z].
        /// </summary>
        public float[] ScaleMin => _scaleMin;

        /// <summary>
        /// The maximum scale as [x,y,z];
        /// </summary>
        public float[] ScaleMax => _scaleMax;

        /// <summary>
        /// An array of bytes representing translation keys.
        /// </summary>
        public byte[] Translations => _translations.ToArray();

        /// <summary>
        /// An array of bytes representing translation time keys.
        /// </summary>
        public byte[] TranslationTimes => _translationTimes.ToArray();

        /// <summary>
        /// The minimum translation time.
        /// </summary>
        public float TranslationTimeMin => _translationTimeMin;

        /// <summary>
        /// The maximum translation time.
        /// </summary>
        public float TranslationTimeMax => _translationTimeMax;

        /// <summary>
        /// The minimum translation as [x,y,z].
        /// </summary>
        public float[] TranslationMin => _translationMin;

        /// <summary>
        /// The maximum translation as [x,y,z].
        /// </summary>
        public float[] TranslationMax => _translationMax;

        /// <summary>
        /// An array of bytes representing rotation keys.
        /// </summary>
        public byte[] Rotations => _rotations.ToArray();

        /// <summary>
        /// An array of bytes representing rotation time keys.
        /// </summary>
        public byte[] RotationTimes => _rotationTimes.ToArray();

        /// <summary>
        /// The minimum rotation time.
        /// </summary>
        public float RotationTimeMin => _rotationTimeMin;

        /// <summary>
        /// The maximum rotation time.
        /// </summary>
        public float RotationTimeMax => _rotationTimeMax;

        /// <summary>
        /// The minimum rotation as [x,y,z,w].
        /// </summary>
        public float[] RotationMin => _rotationMin;

        /// <summary>
        /// The maximum rotation as [x,y,z,w].
        /// </summary>
        public float[] RotationMax => _rotationMax;

        /// <summary>
        /// Create an animation for an element.
        /// </summary>
        public Animation()
        {
            _scaleMin = new float[3] { float.MaxValue, float.MaxValue, float.MaxValue };
            _scaleMax = new float[3] { float.MinValue, float.MinValue, float.MinValue };
            _scaleTimeMin = float.MaxValue;
            _scaleTimeMax = float.MinValue;

            _translationMin = new float[3] { float.MaxValue, float.MaxValue, float.MaxValue };
            _translationMax = new float[3] { float.MinValue, float.MinValue, float.MinValue };
            _translationTimeMin = float.MaxValue;
            _translationTimeMax = float.MinValue;

            _rotationMin = new float[4] { float.MaxValue, float.MaxValue, float.MaxValue, float.MaxValue };
            _rotationMax = new float[4] { float.MinValue, float.MinValue, float.MinValue, float.MinValue };
            _rotationTimeMin = float.MaxValue;
            _rotationTimeMax = float.MinValue;
        }

        /// <summary>
        /// Add a scale keyframe.
        /// </summary>
        /// <param name="scale">The scale value.</param>
        /// <param name="timeSeconds">The time at which to apply the scale value.</param>
        public void AddScaleKeyframe(Geometry.Vector3 scale, double timeSeconds)
        {
            _scales.AddRange(BitConverter.GetBytes((float)scale.X));
            _scales.AddRange(BitConverter.GetBytes((float)scale.Y));
            _scales.AddRange(BitConverter.GetBytes((float)scale.Z));

            _scaleTimes.AddRange(BitConverter.GetBytes((float)timeSeconds));

            _scaleMin[0] = Math.Min(_scaleMin[0], (float)scale.X);
            _scaleMin[1] = Math.Min(_scaleMin[1], (float)scale.Y);
            _scaleMin[2] = Math.Min(_scaleMin[2], (float)scale.Z);

            _scaleMax[0] = Math.Max(_scaleMax[0], (float)scale.X);
            _scaleMax[1] = Math.Max(_scaleMax[1], (float)scale.Y);
            _scaleMax[2] = Math.Max(_scaleMax[2], (float)scale.Z);

            _scaleTimeMin = Math.Min(_scaleTimeMin, (float)timeSeconds);
            _scaleTimeMax = Math.Max(_scaleTimeMax, (float)timeSeconds);
        }

        /// <summary>
        /// Add a rotation keyframe.
        /// </summary>
        /// <param name="axis">The axis of rotation.</param>
        /// <param name="angleDegrees">The angle of rotation in degrees.</param>
        /// <param name="timeSeconds">The keyframe time.</param>
        public void AddRotationKeyframe(Geometry.Vector3 axis, double angleDegrees, double timeSeconds)
        {
            var rotation = new Quaternion(new Vector3((float)axis.X, (float)axis.Y, (float)axis.Z), (float)Units.DegreesToRadians(angleDegrees));
            rotation = Quaternion.Normalize(rotation);

            _rotations.AddRange(BitConverter.GetBytes(rotation.X));
            _rotations.AddRange(BitConverter.GetBytes(rotation.Y));
            _rotations.AddRange(BitConverter.GetBytes(rotation.Z));
            _rotations.AddRange(BitConverter.GetBytes(rotation.W));

            _rotationTimes.AddRange(BitConverter.GetBytes((float)timeSeconds));

            _rotationMin[0] = Math.Min(_rotationMin[0], rotation.X);
            _rotationMin[1] = Math.Min(_rotationMin[1], rotation.Y);
            _rotationMin[2] = Math.Min(_rotationMin[2], rotation.Z);
            _rotationMin[3] = Math.Min(_rotationMin[3], rotation.W);

            _rotationMax[0] = Math.Max(_rotationMax[0], rotation.X);
            _rotationMax[1] = Math.Max(_rotationMax[1], rotation.Y);
            _rotationMax[2] = Math.Max(_rotationMax[2], rotation.Z);
            _rotationMax[3] = Math.Max(_rotationMax[3], rotation.W);

            _rotationTimeMin = Math.Min(_rotationTimeMin, (float)timeSeconds);
            _rotationTimeMax = Math.Max(_rotationTimeMax, (float)timeSeconds);
        }

        /// <summary>
        /// Add a translation keyframe.
        /// </summary>
        /// <param name="translation">The translation value.</param>
        /// <param name="timeSeconds">The time at which to apply the translation.</param>
        public void AddTranslationKeyframe(Geometry.Vector3 translation, double timeSeconds)
        {
            _translations.AddRange(BitConverter.GetBytes((float)translation.X));
            _translations.AddRange(BitConverter.GetBytes((float)translation.Y));
            _translations.AddRange(BitConverter.GetBytes((float)translation.Z));

            _translationTimes.AddRange(BitConverter.GetBytes((float)timeSeconds));

            _translationMin[0] = Math.Min(_translationMin[0], (float)translation.X);
            _translationMin[1] = Math.Min(_translationMin[1], (float)translation.Y);
            _translationMin[2] = Math.Min(_translationMin[2], (float)translation.Z);

            _translationMax[0] = Math.Max(_translationMax[0], (float)translation.X);
            _translationMax[1] = Math.Max(_translationMax[1], (float)translation.Y);
            _translationMax[2] = Math.Max(_translationMax[2], (float)translation.Z);

            _translationTimeMin = Math.Min(_translationTimeMin, (float)timeSeconds);
            _translationTimeMax = Math.Max(_translationTimeMax, (float)timeSeconds);
        }

        /// <summary>
        /// Is the scale of the element animated?
        /// </summary>
        public bool HasAnimatedScale()
        {
            return _scales.Count > 0;
        }

        /// <summary>
        /// Is the translation of the element animated?
        /// </summary>
        public bool HasAnimatedTranslation()
        {
            return _translations.Count > 0;
        }

        /// <summary>
        /// Is the rotation of the element animated?
        /// </summary>
        public bool HasAnimatedRotation()
        {
            return _rotations.Count > 0;
        }
    }
}