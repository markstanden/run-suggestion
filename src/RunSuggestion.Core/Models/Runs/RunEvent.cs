namespace RunSuggestion.Core.Models.Runs;

public class RunEvent : RunBase, IEquatable<RunEvent>
{
    /// <summary>
    /// Equals override performs a value comparison of two RunEvents.
    /// Structural equality is determined by comparing all properties individually.
    /// Nulls are handled by returning false fast.
    /// Referential equality is also checked early.
    /// 
    /// Note: Hash codes are not used for equality comparison as per Microsoft guidelines
    /// - equal hash codes do not guarantee object equality due to potential hash collisions.
    /// <see href="https://docs.microsoft.com/en-us/dotnet/api/system.object.gethashcode"/>
    /// </summary>
    /// <param name="other">A valid or null RunEvent</param>
    /// <returns>True if the two RunEvents are structurally equal.</returns>
    public bool Equals(RunEvent? other)
    {
        // Fail fast if the comparison object is null
        if (other is null) return false;
        
        // Check if the objects reference the same object (Referencial equality)
        // if so they are guaranteed to have the same values (Structural equality)
        if (ReferenceEquals(this, other)) return true;
        
        // Check if the objects have the same values (structural equality)
        // short-circuiting will provide performance benefits and fail early,
        // so test the most likely to be unique first.
        return RunEventId == other.RunEventId &&
               Date.Equals(other.Date) &&
               Distance == other.Distance &&
               Duration.Equals(other.Duration) &&
               Effort == other.Effort;
    }

    /// <summary>
    /// Determines whether an unknown object is equal to a RunEvent.
    /// C# first attempts to cast the object as RunEvent - this returns either a RunEvent or a null, and passes into the Equals override.
    /// </summary>
    /// <param name="obj"></param>
    /// <returns>True if the passed object is both a valid RunEvent, and structurally equal to the calling RunEvent object</returns>
    public override bool Equals(object? obj)
    {
        return Equals(obj as RunEvent);
    }

    /// <summary>
    /// Returns a hash code for the RunEvent, used in collection optimisations.
    /// Note: Hash codes are not used for equality comparison as per Microsoft guidelines
    /// - equal hash codes do not guarantee object equality due to potential hash collisions.
    /// <see href="https://docs.microsoft.com/en-us/dotnet/api/system.object.gethashcode"/>
    /// </summary>
    /// <returns>A hash code for the RunEvent</returns>   
    public override int GetHashCode()
    {
        return HashCode.Combine(RunEventId, Date, Distance, Duration, Effort);
    }
}
