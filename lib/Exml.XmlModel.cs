using System;
using System.Text;
using System.Collections.Generic;

using Exml.Logging;
using ApiModel = Exml.ApiModel;

namespace Exml
{

namespace XmlModel
{

// Object model for EXML interfaces
public class Widget
{

    private static ApiModel.API s_api;

    public string Name { get; set; }
    public Dictionary<string, string> Attributes { get; set; }
    public List<Widget> Children { get; set; }
    public Widget Parent { get; set; }

    private bool _is_container;
    private ApiModel.Class _class;

    public Widget()
    {
        Attributes = new Dictionary<string, string>();
        Children = new List<Widget>();
    }

    public List<ValidatorModel.ValidationIssue> AddInfo(string name, Widget parent)
    {
        // RULE -  is it a valid widget name?
        string internal_name = name;
        var issues = new List<ValidatorModel.ValidationIssue>();

        if (!internal_name.Contains("."))
        {
            internal_name = "Efl.Ui." + internal_name;
        }

        _class = s_api.Classes.Find(c => c.Name == internal_name);

        if (_class == null)
        {
            issues.Add(new ValidatorModel.ValidationIssue($"Unknown type {name}", "Type could not be found in the Efl.Ui namespace",
                                                          ValidatorModel.ValidationIssueSeverity.Error));
        }

        // TODO: Is it a container?
        if (_class != null)
        {
            _is_container = _class.Interfaces.Find(i => i.Name == "Efl.IPack") != null;
            Console.WriteLine($"Class {name} Is container? {_is_container}");
            /* Console.WriteLine($"Class {name} implements the following interfaces:"); */
            /* foreach (var iface in _class.Interfaces) */
            /* { */
            /*     Console.WriteLine($"----> {iface.Name}"); */
            /* } */
        }

        Name = name;
        Parent = parent;

        return issues;
    }

    public List<ValidatorModel.ValidationIssue> AddAttribute(string name, string value)
    {
        // TODO: Rule: Does the property exist?
        // TODO: Rule: Is the property writable?
        // TODO: Rule: Is the value acceptable for the property?
        var issues = new List<ValidatorModel.ValidationIssue>();

        if (Attributes.TryGetValue(name, out string currentValue))
        {
            issues.Add(new ValidatorModel.ValidationIssue($"Unknown type {name}", "Type could not be found in the Efl.Ui namespace",
                                                          ValidatorModel.ValidationIssueSeverity.Error));
        }

        var property = _class.Properties.GetByName(name);
        if (property != null)
        {
            if (!property.HasSet || property.GetVisibility() != Visibility.Public)
            {
                issues.Add(new ValidatorModel.ValidationIssue($"Unknown type {name}", "Type could not be found in the Efl.Ui namespace",
                                                              ValidatorModel.ValidationIssueSeverity.Error));
            }

            if (!IsValueCompatible(property, value))
            {
                issues.Add(new ValidatorModel.ValidationIssue($"Unknown type {name}", "Type could not be found in the Efl.Ui namespace",
                                                              ValidatorModel.ValidationIssueSeverity.Error));
            }
        }

        Attributes[name] = value;

        return issues;
    }

    public List<ValidatorModel.ValidationIssue> AddChild(Widget child)
    {
        // TODO: Can we add this child (Is this a container?)?
        var issues = new List<ValidatorModel.ValidationIssue>();
        if (!_is_container)
        {
            issues.Add(new ValidatorModel.ValidationIssue($"Type {Name} is not a container", "It can't have children",
                                                          ValidatorModel.ValidationIssueSeverity.Error));
        }

        // We still add so we can track the invalid information further down
        Children.Add(child);
        return issues;
    }

    public override String ToString()
    {
        return ToString(0);
    }

    public string ToString(int indent)
    {
        var spaces = new String(' ', 4 * indent);
        var sb = new StringBuilder();

        sb.AppendLine(spaces + $"Widget: {Name}");

        foreach (var entry in Attributes)
        {
            sb.AppendLine(spaces + $"    attrib: {entry.Key} => {entry.Value}");
        }

        if (Children.Count > 0)
        {
            foreach (var child in Children)
            {
                sb.AppendLine(spaces + child.ToString(indent + 1));
            }
        }

        return sb.ToString();
    }

    public static void SetApi(ApiModel.API api)
    {
        s_api = api;
    }

    private bool IsValueCompatible(ApiModel.TypeRef type, string value, ref List<ValidatorModel.ValidationIssue> issues)
    {
        try
        {
            switch (type.Name)
            {
                case "System.Int16": Int16.Parse(value); break;
                case "System.Int32": Int32.Parse(value); break;
                case "System.Int64": Int64.Parse(value); break;
                case "System.Byte": Byte.Parse(value); break;
                case "System.UInt16": UInt16.Parse(value); break;
                case "System.UInt32": UInt32.Parse(value); break;
                case "System.UInt64": UInt64.Parse(value); break;
                case "System.Single": Single.Parse(value); break;
                case "System.Double": Double.Parse(value); break;
                case "System.Decimal": Decimal.Parse(value); break;
                default: return true;
            }
        }
        catch (Exception e)
        {
            return false;
        }

        return true
    }
}

} // Model

} // Exml

