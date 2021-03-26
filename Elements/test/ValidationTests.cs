using Elements.Geometry;
using Elements.Validators;
using System;
using System.Collections.Generic;
using Xunit;

namespace Elements.Tests
{
    public class ValidationTests
    {
        [Fact]
        public void ValidationIsSkipped()
        {
            Action action = () =>
            {
                var polygon = new Polygon(new[] {
                    new Vector3(0,0,0),
                    new Vector3(10,0,0),
                    new Vector3(0, 10, 0),
                    new Vector3(10,10)
                });
            };
            Assert.Throws<System.ArgumentException>(action);
            Validator.DisableValidationOnConstruction = true;
            action();
            Validator.DisableValidationOnConstruction = false;
            Assert.Throws<System.ArgumentException>(action);
        }

    }
}