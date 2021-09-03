namespace Elements.Validators
{
    /// <summary>
    /// The supplier of validation logic for for element construction.
    /// </summary>
    public static class Validator
    {
        /// <summary>
        /// Should geometry validation be disabled during construction? 
        /// Note: Disabling validation can have unforeseen consequences. Use with caution.
        /// </summary>
        public static bool DisableValidationOnConstruction { get; set; } = false;
    }
}