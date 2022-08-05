using System;
using System.Collections.Generic;
using System.Numerics;

namespace Elements
{
    /// <summary>
    /// An element's animation.
    /// </summary>
    public class Animation
    {
        List<byte> _scaleTimes = new List<byte>();
        List<byte> _scales = new List<byte>();
        List<byte> _translationTimes = new List<byte>();
        List<byte> _translations = new List<byte>();
        List<byte> _rotationTimes = new List<byte>();
        List<byte> _rotations = new List<byte>();

        float[] _scaleMin;
        float[] _scaleMax;
        float _scaleTimeMin;
        float _scaleTimeMax;

        float[] _translationMin;
        float[] _translationMax;
        float _translationTimeMin;
        float _translationTimeMax;

        float[] _rotationMin;
        float[] _rotationMax;
        float _rotationTimeMin;
        float _rotationTimeMax;

        public byte[] Scales => _scales.ToArray();
        public byte[] ScaleTimes => _scaleTimes.ToArray();
        public float ScaleTimeMin => _scaleTimeMin;
        public float ScaleTimeMax => _scaleTimeMax;
        public float[] ScaleMin => _scaleMin;
        public float[] ScaleMax => _scaleMax;

        public byte[] Translations => _translations.ToArray();
        public byte[] TranslationTimes => _translationTimes.ToArray();
        public float TranslationTimeMin => _translationTimeMin;
        public float TranslationTimeMax => _translationTimeMax;
        public float[] TranslationMin => _translationMin;
        public float[] TranslationMax => _translationMax;

        public byte[] Rotations => _rotations.ToArray();
        public byte[] RotationTimes => _rotationTimes.ToArray();
        public float RotationTimeMin => _rotationTimeMin;
        public float RotationTimeMax => _rotationTimeMax;
        public float[] RotationMin => _rotationMin;
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
        /// <param name="scale"></param>
        /// <param name="time"></param>
        public void AddScaleKeyframe(Geometry.Vector3 scale, double time)
        {
            _scales.AddRange(BitConverter.GetBytes((float)scale.X));
            _scales.AddRange(BitConverter.GetBytes((float)scale.Y));
            _scales.AddRange(BitConverter.GetBytes((float)scale.Z));

            _scaleTimes.AddRange(BitConverter.GetBytes((float)time));

            _scaleMin[0] = Math.Min(_scaleMin[0], (float)scale.X);
            _scaleMin[1] = Math.Min(_scaleMin[1], (float)scale.Y);
            _scaleMin[2] = Math.Min(_scaleMin[2], (float)scale.Z);

            _scaleMax[0] = Math.Max(_scaleMax[0], (float)scale.X);
            _scaleMax[1] = Math.Max(_scaleMax[1], (float)scale.Y);
            _scaleMax[2] = Math.Max(_scaleMax[2], (float)scale.Z);

            _scaleTimeMin = Math.Min(_scaleTimeMin, (float)time);
            _scaleTimeMax = Math.Max(_scaleTimeMax, (float)time);
        }

        /// <summary>
        /// Add a rotation keyframe.
        /// </summary>
        /// <param name="axis">The axis of rotation.</param>
        /// <param name="angle">The angle of rotation in radians.</param>
        /// <param name="time">The keyframe time.</param>
        public void AddRotationKeyframe(Elements.Geometry.Vector3 axis, double angle, double time)
        {
            var rotation = new Quaternion(new Vector3((float)axis.X, (float)axis.Y, (float)axis.Z), (float)Units.DegreesToRadians(angle));
            rotation = Quaternion.Normalize(rotation);

            _rotations.AddRange(BitConverter.GetBytes(rotation.X));
            _rotations.AddRange(BitConverter.GetBytes(rotation.Y));
            _rotations.AddRange(BitConverter.GetBytes(rotation.Z));
            _rotations.AddRange(BitConverter.GetBytes(rotation.W));

            _rotationTimes.AddRange(BitConverter.GetBytes((float)time));

            _rotationMin[0] = Math.Min(_rotationMin[0], rotation.X);
            _rotationMin[1] = Math.Min(_rotationMin[1], rotation.Y);
            _rotationMin[2] = Math.Min(_rotationMin[2], rotation.Z);
            _rotationMin[3] = Math.Min(_rotationMin[3], rotation.W);

            _rotationMax[0] = Math.Max(_rotationMax[0], rotation.X);
            _rotationMax[1] = Math.Max(_rotationMax[1], rotation.Y);
            _rotationMax[2] = Math.Max(_rotationMax[2], rotation.Z);
            _rotationMax[3] = Math.Max(_rotationMax[3], rotation.W);

            _rotationTimeMin = Math.Min(_rotationTimeMin, (float)time);
            _rotationTimeMax = Math.Max(_rotationTimeMax, (float)time);
        }

        /// <summary>
        /// Add a translation keyframe.
        /// </summary>
        /// <param name="translation"></param>
        /// <param name="time"></param>
        public void AddTranslationKeyframe(Geometry.Vector3 translation, double time)
        {
            _translations.AddRange(BitConverter.GetBytes((float)translation.X));
            _translations.AddRange(BitConverter.GetBytes((float)translation.Y));
            _translations.AddRange(BitConverter.GetBytes((float)translation.Z));

            _translationTimes.AddRange(BitConverter.GetBytes((float)time));

            _translationMin[0] = Math.Min(_translationMin[0], (float)translation.X);
            _translationMin[1] = Math.Min(_translationMin[1], (float)translation.Y);
            _translationMin[2] = Math.Min(_translationMin[2], (float)translation.Z);

            _translationMax[0] = Math.Max(_translationMax[0], (float)translation.X);
            _translationMax[1] = Math.Max(_translationMax[1], (float)translation.Y);
            _translationMax[2] = Math.Max(_translationMax[2], (float)translation.Z);

            _translationTimeMin = Math.Min(_translationTimeMin, (float)time);
            _translationTimeMax = Math.Max(_translationTimeMax, (float)time);
        }

        /// <summary>
        /// Is the scale of the element animated?
        /// </summary>
        /// <returns></returns>
        public bool HasAnimatedScale()
        {
            return _scales.Count > 0;
        }

        /// <summary>
        /// Is the translation of the element animated?
        /// </summary>
        /// <returns></returns>
        public bool HasAnimatedTranslation()
        {
            return _translations.Count > 0;
        }

        /// <summary>
        /// Is the rotation of the element animated?
        /// </summary>
        /// <returns></returns>
        public bool HasAnimatedRotation()
        {
            return _rotations.Count > 0;
        }
    }
}