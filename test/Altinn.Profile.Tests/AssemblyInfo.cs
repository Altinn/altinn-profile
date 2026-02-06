using Xunit;

// Temporary mitigation: disable parallel test execution to avoid races caused by shared mutable WebApplicationFactory mocks.
[assembly: CollectionBehavior(DisableTestParallelization = true)]
