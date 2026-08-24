using Xunit;

// The grid reads its words from the application's resources, which is ambient state: a test that
// stands a translation in for it would otherwise be running beside one asserting the English the
// library falls back to. The whole suite is a third of a second, so serializing it costs nothing.
[assembly: CollectionBehavior(DisableTestParallelization = true)]
