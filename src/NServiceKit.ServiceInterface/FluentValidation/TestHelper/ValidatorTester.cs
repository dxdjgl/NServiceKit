#region License
// Copyright (c) Jeremy Skinner (http://www.jeremyskinner.co.uk)
// 
// Licensed under the Apache License, Version 2.0 (the "License"); 
// you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at 
// 
// http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, 
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
// See the License for the specific language governing permissions and 
// limitations under the License.
// 
// The latest version of this file can be found at http://www.codeplex.com/FluentValidation
#endregion

namespace NServiceKit.FluentValidation.TestHelper
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Internal;

    /// <summary>A validator tester.</summary>
    /// <typeparam name="T">     Generic type parameter.</typeparam>
    /// <typeparam name="TValue">Type of the value.</typeparam>
    public class ValidatorTester<T, TValue> where T : class {
        private readonly IValidator<T> validator;
        private readonly TValue value;
        private readonly MemberInfo member;

        /// <summary>Initializes a new instance of the NServiceKit.FluentValidation.TestHelper.ValidatorTester&lt;T, TValue&gt; class.</summary>
        ///
        /// <param name="expression">The expression.</param>
        /// <param name="validator"> The validator.</param>
        /// <param name="value">     The value.</param>
        public ValidatorTester(Expression<Func<T, TValue>> expression, IValidator<T> validator, TValue value) {
            this.validator = validator;
            this.value = value;
            member = expression.GetMember();
        }

        /// <summary>Validates the no error described by instanceToValidate.</summary>
        ///
        /// <exception cref="ValidationTestException">Thrown when a Validation Test error condition occurs.</exception>
        ///
        /// <param name="instanceToValidate">The instance to validate.</param>
        public void ValidateNoError(T instanceToValidate) {
            SetValue(instanceToValidate);

            var count = validator.Validate(instanceToValidate).Errors.Count(x => x.PropertyName == member.Name);

            if (count > 0) {
                throw new ValidationTestException(string.Format("Expected no validation errors for property {0}", member.Name));
            }
        }

        /// <summary>Validates the error described by instanceToValidate.</summary>
        ///
        /// <exception cref="ValidationTestException">Thrown when a Validation Test error condition occurs.</exception>
        ///
        /// <param name="instanceToValidate">The instance to validate.</param>
        public void ValidateError(T instanceToValidate) {
            SetValue(instanceToValidate);
            var count = validator.Validate(instanceToValidate).Errors.Count(x => x.PropertyName == member.Name);

            if (count == 0) {
                throw new ValidationTestException(string.Format("Expected a validation error for property {0}", member.Name));
            }
        }

        private void SetValue(object instance) {
            var property = member as PropertyInfo;
            if (property != null) {
                property.SetValue(instance, value, null);
                return;
            }

            var field = member as FieldInfo;
            if (field != null) {
                field.SetValue(instance, value);
            }
        }
    }
}