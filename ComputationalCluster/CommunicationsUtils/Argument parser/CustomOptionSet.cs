using System;
using NDesk.Options;

namespace CommunicationsUtils.Argument_parser
{
    internal class CustomOptionSet : OptionSet
    {
        protected override void InsertItem(int index, Option item)
        {
            if (item.Prototype.ToLower() != item.Prototype)
                throw new ArgumentException("prototypes must be lower-case!");
            base.InsertItem(index, item);
        }

        protected override OptionContext CreateOptionContext()
        {
            return new OptionContext(this);
        }

        protected override bool Parse(string option, OptionContext c)
        {
            string f, n, s, v;
            var haveParts = GetOptionParts(option, out f, out n, out s, out v);
            Option nextOption = null;
            var newOption = option;

            if (haveParts)
            {
                var nl = n.ToLower();
                nextOption = Contains(nl) ? this[nl] : null;
                newOption = f + n.ToLower() + (v != null ? s + v : "");
            }

            if (c.Option != null)
            {
                if (c.Option != null && haveParts)
                {
                    if (nextOption == null)
                    {
                        throw new OptionException(
                            $"Found option `{option}' as value for option `{c.OptionName}'.", c.OptionName);
                    }
                }
                if (AppendValue(option, c))
                {
                    if (!option.EndsWith("\\") &&
                        c.Option.MaxValueCount == c.OptionValues.Count)
                    {
                        c.Option.Invoke(c);
                    }
                    return true;
                }
                else
                    base.Parse(newOption, c);
            }

            if (!haveParts || v == null)
            {
                return base.Parse(newOption, c);
            }

            if (nextOption != null && (nextOption.OptionValueType == OptionValueType.None || !v.EndsWith("\\")))
                return base.Parse(newOption, c);
            c.Option = nextOption;
            c.OptionValues.Add(v);
            c.OptionName = f + n;
            return true;
        }

        private static bool AppendValue(string value, OptionContext c)
        {
            var added = false;
            var seps = c.Option.GetValueSeparators();
            foreach (var o in seps.Length != 0
                ? value.Split(seps, StringSplitOptions.None)
                : new[] { value })
            {
                var idx = c.OptionValues.Count - 1;
                if (idx == -1 || !c.OptionValues[idx].EndsWith("\\"))
                {
                    c.OptionValues.Add(o);
                    added = true;
                }
                else {
                    c.OptionValues[idx] += value;
                    added = true;
                }
            }
            return added;
        }
    }
}