namespace XIVExtraComboVX.Attributes;

using System;

[AttributeUsage(AttributeTargets.Field)]
internal class DeprecatedAttribute: Attribute {
	public CustomComboPreset[] Recommended { get; }
	internal DeprecatedAttribute(params CustomComboPreset[] suggestions) {
		this.Recommended = suggestions;
	}
}
