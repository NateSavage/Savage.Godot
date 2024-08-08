using Godot;

namespace Savage.GD;

#nullable enable

public static class ErrorExtensions {
    
    /// <returns> True when an error was present. </returns>
    public static bool LogIfNotOk(this Error outcome, string? message = null) {
        if (outcome == Error.Ok)
            return false;

        Godot.GD.PrintErr($"{message}: {outcome}");
        return true;
    }

    /// <returns> True when an error was present. </returns>
    public static Error LogOutcome(this Error outcome, string? message = null) {
        Godot.GD.PrintErr($"{message}: {outcome}");
        return outcome;
    }

    /// <returns> True when an error was present. </returns>
    public static bool NotOk(this Error outcome, out Error error) {
        error = outcome;
        return outcome != Error.Ok;
    } 
}

#nullable restore