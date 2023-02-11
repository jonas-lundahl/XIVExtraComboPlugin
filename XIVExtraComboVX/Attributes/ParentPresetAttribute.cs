namespace XIVExtraComboVX.Attributes;

using System;

[AttributeUsage(AttributeTargets.Field)]
internal class ParentPresetAttribute: Attribute {
	public CustomComboPreset Parent { get; }
	internal ParentPresetAttribute(CustomComboPreset required) {
		this.Parent = required;
	}
}
