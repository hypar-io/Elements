using Newtonsoft.Json;
using System;

namespace Hypar.Functions
{
    /// <summary>
    /// A numeric parameter with a defined range.
    /// </summary>
    public class RangeParameter: InputOutputBase
    {
        /// <summary>
        /// The minimum value of the parameter.
        /// </summary>
        [JsonProperty("min")]
        public double Min{get;}

        /// <summary>
        /// The maximum value of the parameter.
        /// </summary>
        [JsonProperty("max")]
        public double Max{get;}

        /// <summary>
        /// The step of the parameter.
        /// </summary>
        [JsonProperty("step")]
        public double Step{get;}

        /// <summary>
        /// Construct a RangeParameter.
        /// </summary>
        /// <param name="description"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="step"></param>
        /// <exception cref="System.ArgumentException">Thrown when the minimum value is less than the maximum value.</exception>
        /// <exception cref="System.ArgumentException">Thrown when the step is equal to zero.</exception>
        public RangeParameter(string description, double min, double max, double step) : base(description, HyparParameterType.Number)
        {
            if(min > max)
            {
                throw new ArgumentException($"The number parameter could not be created. The min value, {min}, cannot be greater than the max value, {max}.");
            }

            if(step <= 0.0)
            {
                throw new ArgumentException($"The number parameter could not be created. The step value must greater than 0.0");
            }
            this.Min = min;
            this.Max = max;
            this.Step = step;
        }
    }
}