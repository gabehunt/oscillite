using System;


namespace Oscillite.CleanRoom.LSM
{
    // we discovered these from real world files, but are not sure the list is complete.
	// Ignition scope was only found in 2 files, indicating it is not commonly used.
    public enum ScopeType
	{
		
		ScopeType_LabScope,
		ScopeType_GraphingMeter,
		ScopeType_IgnitionScope,
		ScopeType_DigitalMeter
	}
}
