using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Elements.Validators
{
    /// <summary>
    /// Implement this interface to act as a validator.
    /// </summary>
    public interface IValidator
    {
        /// <summary>
        /// The type to be validated.
        /// </summary>
        /// <value></value>
        Type ValidatesType {get;}

        /// <summary>
        /// Validate the type with the provided arguments.
        /// </summary>
        /// <param name="args"></param>
        void Validate(object[] args);
    }

    /// <summary>
    /// The supplier of validation logic for for element construction.
    /// </summary>
    public class Validator
    {
        private static Validator _validator;

        /// <summary>
        /// The validator singleton.
        /// </summary>
        public static Validator Instance
        {
            get
            {
                if(_validator == null)
                {
                    _validator = new Validator();
                }
                return _validator;
            }
        }

        private Dictionary<Type,IValidator> _validators;

        private Validator()
        {
            _validators = new Dictionary<Type, IValidator>();

            // Load all available validators
            var validatorTypes = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => typeof(IValidator).IsAssignableFrom(t) && typeof(IValidator) != t);

            foreach (var t in validatorTypes)
            {
                var v = (IValidator)Activator.CreateInstance(t);
                _validators.Add(v.ValidatesType, v);
            }
        }

        /// <summary>
        /// Gets the first validator for the supplied T.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>A validator for T, or null if no validator for T can be found.</returns>
        public IValidator GetFirstValidatorForType<T>()
        {
            if(_validators.ContainsKey(typeof(T)))
            {
                return _validators[typeof(T)];
            }
            return null;
        }
    }
}