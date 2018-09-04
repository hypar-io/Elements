using Newtonsoft.Json;

namespace Hypar.Elements
{
    /// <summary>
    /// ParameterValue represents both the value and the type of a parameter.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    public abstract class Parameter<TValue>
    {
        /// <summary>
        /// The value of the parameter.
        /// </summary>
        /// <value></value>
        [JsonProperty("value")]
        public TValue Value{get;}

        /// <summary>
        /// Construct a parameter value given a value and a type.
        /// </summary>
        /// <param name="value">The value of the parameter.</param>
        public Parameter(TValue value)
        {
            this.Value = value;
        }
    }

    public enum NumericParameterType
    {
        None, Distance, Area, Volume, Mass, Force
    }

    public class NumericParameter  : Parameter<double>
    {
        /// <summary>
        /// The type of the value.
        /// </summary>
        /// <value></value>
        [JsonProperty("type")]
        public NumericParameterType Type{get;}

        public NumericParameter(double value, NumericParameterType type) : base(value)
        {
            this.Type = type;
        }

    }

    public class StringParameter : Parameter<string>
    {
        public StringParameter(string value) : base(value){}
    }
}