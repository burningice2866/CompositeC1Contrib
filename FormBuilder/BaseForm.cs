using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using CompositeC1Contrib.FormBuilder.Attributes;
using CompositeC1Contrib.FormBuilder.Dependencies;
using CompositeC1Contrib.FormBuilder.Validation;

namespace CompositeC1Contrib.FormBuilder
{
    public abstract class BaseForm
    {
        private readonly object _lock = new object();

        public NameValueCollection SubmittedValues { get; private set; }
        public IEnumerable<FormFile> SubmittedFiles { get; private set; }

        private IDictionary<PropertyInfo, IList<FormValidationRule>> _ruleList = null;
        private List<FormValidationRule> _validationResult = null;

        public BaseForm(NameValueCollection values, IEnumerable<FormFile> files)
        {
            SubmittedValues = values;
            SubmittedFiles = files;

            if (SubmittedValues != null)
            {
                var props = GetType().GetProperties().Where(p => p.DeclaringType != typeof(BaseForm));
                foreach (var prop in props)
                {
                    var val = (SubmittedValues[prop.Name] ?? String.Empty).Trim();
                    var t = prop.PropertyType;

                    MapValueToProperty(prop, val, t);
                }
            }
        }

        private void MapValueToProperty(PropertyInfo prop, string val, Type t)
        {
            if (t == typeof(int))
            {
                var i = 0;
                int.TryParse(val, out i);

                prop.SetValue(this, i, null);
            }
            else if (t == typeof(decimal))
            {
                var d = 0m;
                decimal.TryParse(val, out d);

                prop.SetValue(this, d, null);
            }
            else if (t == typeof(int?))
            {
                var i = 0;
                if (int.TryParse(val, out i))
                {
                    prop.SetValue(this, i, null);
                }
                else
                {
                    prop.SetValue(this, null, null);
                }
            }
            else if (t == typeof(decimal?))
            {
                var d = 0m;
                if (decimal.TryParse(val, out d))
                {
                    prop.SetValue(this, d, null);
                }
                else
                {
                    prop.SetValue(this, null, null);
                }
            }
            else if (t == typeof(bool))
            {
                var b = false;

                if (val == "on")
                {
                    b = true;
                }
                else
                {
                    bool.TryParse(val, out b);
                }

                prop.SetValue(this, b, null);

            }
            else if (t == typeof(string))
            {
                prop.SetValue(this, val, null);
            }
            else if (t == typeof(IEnumerable<string>))
            {
                prop.SetValue(this, val.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries), null);
            }

            if (SubmittedFiles != null && SubmittedFiles.Any())
            {
                var files = SubmittedFiles.Where(f => f.Key == prop.Name);

                if (t == typeof(FormFile))
                {
                    prop.SetValue(this, files.FirstOrDefault(), null);

                }
                else if (t == typeof(IEnumerable<FormFile>))
                {
                    prop.SetValue(this, files, null);
                }
            }
        }

        protected virtual void OnValidation(FormValidationEventArgs e) { }

        public IEnumerable<FormValidationRule> Validate()
        {
            if (SubmittedValues == null || SubmittedValues.AllKeys.Length == 0)
            {
                return Enumerable.Empty<FormValidationRule>();
            }

            var e = new FormValidationEventArgs();

            OnValidation(e);

            if (e.Cancel)
            {
                return Enumerable.Empty<FormValidationRule>();
            }

            ensureRulesList();

            if (_validationResult == null)
            {
                lock (_lock)
                {
                    if (_validationResult == null)
                    {
                        _validationResult = new List<FormValidationRule>();

                        foreach (var list in _ruleList.Values)
                        {
                            var validationResult = getFormValidationResult(list, false);

                            _validationResult.AddRange(validationResult);
                        }
                    }
                }
            }

            return _validationResult;
        }

        public bool IsValid(string[] fields)
        {
            ensureRulesList();

            foreach (var field in fields)
            {
                var prop = GetType().GetProperty(field);
                var result = getFormValidationResult(_ruleList[prop], true);

                if (result.Any(r => r.AffectedFormIds.Contains(field)))
                {
                    return false;
                }
            }

            return true;
        }

        public bool IsRequired(PropertyInfo prop)
        {
            var attrs = prop.GetCustomAttributes(true);

            return attrs.Any(attr => attr is RequiredFieldAttribute);
        }

        private bool IsDependencyMetRecursive(PropertyInfo prop)
        {
            var attributes = prop.GetCustomAttributes(true).OfType<FormDependencyAttribute>().ToList();

            if (!attributes.Any())
            {
                return true;
            }

            foreach (var dependencyAttribute in attributes)
            {
                if (dependencyAttribute.DependencyMet(this))
                {
                    var dependencyProperty = this.GetType().GetProperty(dependencyAttribute.ReadFromFieldName);
                    if (IsDependencyMetRecursive(dependencyProperty))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private void ensureRulesList()
        {
            if (_ruleList == null)
            {
                lock (_lock)
                {
                    if (_ruleList == null)
                    {
                        _ruleList = new Dictionary<PropertyInfo, IList<FormValidationRule>>();

                        var props = GetType().GetProperties();
                        foreach (var prop in props)
                        {
                            if (!_ruleList.ContainsKey(prop))
                            {
                                _ruleList.Add(prop, new List<FormValidationRule>());
                            }

                            var list = _ruleList[prop];

                            var attributes = prop.GetCustomAttributes(true);

                            bool validateField = IsDependencyMetRecursive(prop);

                            if (validateField)
                            {
                                foreach (var attr in attributes)
                                {
                                    var validationAttribute = attr as FormValidationAttribute;
                                    if (validationAttribute != null)
                                    {
                                        var rule = validationAttribute.CreateRule(prop, this);

                                        list.Add(rule);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private IList<FormValidationRule> getFormValidationResult(IList<FormValidationRule> rules, bool skipMultipleFieldsRules)
        {
            return rules.Where(r =>
                {
                    if (skipMultipleFieldsRules)
                    {
                        return r.AffectedFormIds.Count == 1;
                    }

                    return true;
                })
            .Where(r => !r.Rule())
            .ToList();
        }

        public void Submit()
        {
            var attrs = GetType().GetCustomAttributes(true);
            foreach (var attr in attrs)
            {
                var submitHandler = attr as SubmitHandlerAttribute;
                if (submitHandler != null)
                {
                    submitHandler.Submit(this);
                }
            }
        }

        public PropertyInfo GetProperty(string name)
        {
            return GetType().GetProperty(name);
        }

        public PropertyInfo GetProperty<T>(Expression<Func<T, object>> field) where T : BaseForm
        {
            MemberExpression memberExpression;

            switch (field.Body.NodeType)
            {
                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                    var unaryExpression = (UnaryExpression)field.Body;
                    memberExpression = (MemberExpression)unaryExpression.Operand;

                    break;

                default:
                    memberExpression = field.Body as MemberExpression;

                    break;
            }

            return (PropertyInfo)memberExpression.Member;
        }
    }
}
