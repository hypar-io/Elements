using Hypar.GeoJSON;

namespace Hypar
{
    [System.AttributeUsage(System.AttributeTargets.Class |  
                       System.AttributeTargets.Method,  
                       AllowMultiple = true)] 
    public abstract class HyparParameter : System.Attribute
    {
        public string ParameterName{get;}
        public string Description{get;}
        public HyparParameter(string parameterName, string description)
        {
            this.ParameterName = parameterName;
            this.Description = description;
        }
    }

    public class NumericParameterData : HyparParameter
    {
        public double Min{get;}
        public double Max{get;}
        public double Step{get;}
        public NumericParameterData(string parameterName, string description, double min, double max, double step) : base(parameterName, description)
        {
            this.Min = min;
            this.Max = max;
            this.Step = step;
        }
    }

    public class LocationParameterData : HyparParameter
    {
        public Feature[] Location{get;}
        public LocationParameterData(string parameterName, string description):base(parameterName, description){}
    }
}