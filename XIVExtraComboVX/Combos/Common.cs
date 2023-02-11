namespace XIVExtraComboVX.Combos;

internal class Common {
	public const uint
		// tanks
		LowBlow = 7540,
		Interject = 7538,
		// mages
		Swiftcast = 7561,
		// healers
		LucidDreaming = 7562;

	internal class Buffs {
		public const ushort
		Swiftcast1 = 167,
		Swiftcast2 = 1325,
		Swiftcast3 = 1987,
		LostChainspell = 2560;
	}

	internal class Levels {
		public const uint
			LucidDreaming = 14,
			Swiftcast = 18;
	}
}

internal abstract class SwiftRaiseCombo: CustomCombo {
	protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level) {
		return level >= Common.Levels.Swiftcast && ShouldSwiftcast
			? Common.Swiftcast
			: actionID;
	}
}

internal abstract class StunInterruptCombo: CustomCombo {
	public override uint[] ActionIDs { get; } = new[] { Common.LowBlow, Common.Interject };

	protected override uint Invoke(uint actionID, uint lastComboActionId, float comboTime, byte level) {
		return CanInterrupt && IsOffCooldown(Common.Interject)
			? Common.Interject
			: Common.LowBlow;
	}
}
