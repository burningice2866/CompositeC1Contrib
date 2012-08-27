using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using CompositeC1Contrib.FormBuilder.Attributes;
using CompositeC1Contrib.FormBuilder.Validation;

namespace CompositeC1Contrib.FormBuilder
{
    public abstract class BaseForm
    {
        private readonly object _lock = new object();

        private NameValueCollection _form = null;
        private IDictionary<PropertyInfo, IList<FormValidationRule>> _ruleList = null;
        private List<FormValidationRule> _validationResult = null;

        public BaseForm(NameValueCollection form)
        {
            _form = form;

            var props = GetType().GetProperties();
            foreach (var prop in props)
            {
                var val = (_form[prop.Name] ?? String.Empty).Trim();
                var t = prop.PropertyType;

                if (t == typeof(int))
                {
                    var i = 0;
                    int.TryParse(val, out i);

                    prop.SetValue(this, i, null);
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
                else if (t == typeof(bool))
                {
                    prop.SetValue(this, val == "on", null);
                }
                else if (t == typeof(string))
                {
                    prop.SetValue(this, val, null);
                }
            }
        }

        public IEnumerable<FormValidationRule> Validate()
        {
            if (_form.AllKeys.Length > 0)
            {
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
            else
            {
                return Enumerable.Empty<FormValidationRule>();
            }
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

        public string GetFormValue(string name)
        {
            return _form[name];
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
