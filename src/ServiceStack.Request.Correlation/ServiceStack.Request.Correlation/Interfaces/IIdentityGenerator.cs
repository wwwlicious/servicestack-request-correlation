namespace ServiceStack.Request.Correlation.Interfaces
{
    /// <summary>
    /// Contains method used to generate request correlation id
    /// </summary>
    public interface IIdentityGenerator
    {
        /// <summary>
        /// Generate a string that will uniquely identify a request
        /// </summary>
        /// <returns></returns>
        string GenerateIdentity();
    }
}