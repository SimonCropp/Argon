namespace Argon;

/// <summary>
/// Provides an interface for using pooled arrays.
/// </summary>
public interface IArrayPool<T>
{
    /// <summary>
    /// Rent an array from the pool. This array must be returned when it is no longer needed.
    /// </summary>
    T[] Rent(int minimumLength);

    /// <summary>
    /// Return an array to the pool.
    /// </summary>
    void Return(T[]? array);
}