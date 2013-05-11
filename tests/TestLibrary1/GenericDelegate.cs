#pragma warning disable 1591,0067,0169,0649

namespace TestLibrary1
{
    /// <summary>
    /// A generic delegate.
    /// </summary>
    /// <typeparam name="TA">The type of the value.</typeparam>
    /// <param name="a">The value.</param>
    /// <returns>The result.</returns>
    public delegate TA GenericDelegate<TA>(TA a);
}
