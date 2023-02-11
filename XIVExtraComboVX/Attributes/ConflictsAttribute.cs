namespace XIVExtraComboVX.Attributes;

using System;

[AttributeUsage(AttributeTargets.Field)]
internal class ConflictsAttribute: Attribute {
	public CustomComboPreset[] Conflicts { get; }
	internal ConflictsAttribute(params CustomComboPreset[] conflictingPresets) {
		this.Conflicts = conflictingPresets;
	}
}
