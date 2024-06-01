using System.Runtime.CompilerServices;

// Expose private members to "friend" testing assemblies
[assembly: InternalsVisibleTo("ACMESharp.UnitTests, PublicKey=b0c98a8633767f34")]
[assembly: InternalsVisibleTo("ACMESharp.IntegrationTests, PublicKey=b0c98a8633767f34")]