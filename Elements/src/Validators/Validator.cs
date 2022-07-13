using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Elements.Validators
{
    /// <summary>
    /// Implement this interface to act as a validator.
    /// </summary>
    [Obsolete]
    public interface IValidator
    {
        /// <summary>
        /// The type to be validated.
        /// </summary>
        Type ValidatesType { get; }

        /// <summary>
        /// Validate the object with the provided arguments.
        /// </summary>
        /// <param name="args"></param>
        void PreConstruct(object[] args);

        /// <summary>
        /// Post construction logic.
        /// </summary>
        /// <param name="obj">The constructed object.</param>
        void PostConstruct(object obj);
    }

    /// <summary>
    /// The supplier of validation logic for for element construction.
    /// </summary>
    public class Validator
    {
        private static Validator _validator;

        /// <summary>
        /// Should geometry validation be disabled during construction? 
        /// Note: Disabling validation can have unforeseen consequences. Use with caution.
        /// </summary>
        public static bool DisableValidationOnConstruction { get; set; } = false;

        /// <summary>
        /// The validator singleton.
        /// </summary>
        [Obsolete]
        public static Validator Instance
        {
            get
            {
                if (_validator == null)
                {
                    _validator = new Validator();
                }
                return _validator;
            }
        }
#pragma warning disable CS0612
        private Dictionary<Type, IValidator> _validators;

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
#pragma warning restore CS0612
        /// <summary>
        /// Gets the first validator for the supplied T.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>A validator for T, or null if no validator for T can be found.</returns>
        [Obsolete("Please include validation logic in the constructor of your object. Use the DisableValidationOnConstruction property to disable validation logic.")]
        public IValidator GetFirstValidatorForType<T>()
        {
            if (Validator.DisableValidationOnConstruction)
            {
                return null;
            }

            if (_validators.ContainsKey(typeof(T)))
            {
                return _validators[typeof(T)];
            }

            return null;
        }
    }
}