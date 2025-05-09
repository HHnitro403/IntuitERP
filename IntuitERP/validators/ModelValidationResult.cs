using System.Collections.Generic;
using System.Linq;

namespace IntuitERP.Validators
{
    public class ModelValidationResult
    {
        private readonly List<string> _errors = new List<string>();

        public bool IsValid => !_errors.Any();
        public IReadOnlyList<string> Errors => _errors.AsReadOnly();
        public string ErrorsAsString => string.Join("; ", _errors);

        public void AddError(string error)
        {
            _errors.Add(error);
        }
    }
}
