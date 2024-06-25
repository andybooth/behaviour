﻿namespace Behaviour.Tests;

public class BehaviourRunnerTests
{
    [Fact]
    public async Task BehaviourRunner_SingleScenario_ExecuteSingle()
    {
        var context = new BehaviourContext(NullLogger.Instance);
        var feature = Substitute.For<BehaviourFeature>();
        var scenario = Substitute.For<BehaviourScenario>();

        feature.Given(context).Returns(true);
        feature.Scenarios.Returns([scenario]);

        scenario.Given(context).Returns(BehaviourPhase.On);
        scenario.When(context).Returns(true);
        scenario.ThenAsync(context).Returns(context.Continue());

        var result = await BehaviourRunner.ExecuteAsync(context, [feature]);

        Assert.NotNull(result);
        Assert.NotNull(scenario.Received(1).ThenAsync(context));
    }

    [Fact]
    public async Task BehaviourRunner_MultipleScenario_ExecuteAll()
    {
        var context = new BehaviourContext(NullLogger.Instance);
        var feature = Substitute.For<BehaviourFeature>();
        var scenario1 = Substitute.For<BehaviourScenario>();
        var scenario2 = Substitute.For<BehaviourScenario>();
        var scenario3 = Substitute.For<BehaviourScenario>();
        var scenario4 = Substitute.For<BehaviourScenario>();
        var scenario5 = Substitute.For<BehaviourScenario>();
        var scenario6 = Substitute.For<BehaviourScenario>();

        feature.Given(context).Returns(true);
        feature.Scenarios.Returns([scenario1, scenario2, scenario3, scenario4, scenario5, scenario6]);

        scenario1.Given(context).Returns(BehaviourPhase.None);
        scenario1.When(context).Returns(true);
        scenario1.ThenAsync(context).Returns(context.Continue());

        scenario2.Given(context).Returns(BehaviourPhase.Before);
        scenario2.When(context).Returns(true);
        scenario2.ThenAsync(context).Returns(context.Continue());

        scenario3.Given(context).Returns(BehaviourPhase.On);
        scenario3.When(context).Returns(true);
        scenario3.ThenAsync(context).Returns(context.Continue());

        scenario4.Given(context).Returns(BehaviourPhase.On);
        scenario4.When(context).Returns(true);
        scenario4.ThenAsync(context).Returns(context.Continue());

        scenario5.Given(context).Returns(BehaviourPhase.On);
        scenario5.When(context).Returns(false);
        scenario5.ThenAsync(context).Returns(context.Continue());

        scenario6.Given(context).Returns(BehaviourPhase.After);
        scenario6.When(context).Returns(true);
        scenario6.ThenAsync(context).Returns(context.Continue());

        var result = await BehaviourRunner.ExecuteAsync(context, [feature]);

        Assert.NotNull(result);
        Assert.NotNull(scenario1.DidNotReceive().ThenAsync(context));
        Assert.NotNull(scenario2.Received(1).ThenAsync(context));
        Assert.NotNull(scenario3.Received(1).ThenAsync(context));
        Assert.NotNull(scenario4.Received(1).ThenAsync(context));
        Assert.NotNull(scenario5.DidNotReceive().ThenAsync(context));
        Assert.NotNull(scenario6.Received(1).ThenAsync(context));
    }

    [Fact]
    public async Task BehaviourRunner_WhenFalse_SkipScenario()
    {
        var context = new BehaviourContext(NullLogger.Instance);
        var feature = Substitute.For<BehaviourFeature>();
        var scenario1 = Substitute.For<BehaviourScenario>();
        var scenario2 = Substitute.For<BehaviourScenario>();
        var scenario3 = Substitute.For<BehaviourScenario>();

        feature.Given(context).Returns(true);
        feature.Scenarios.Returns([scenario1, scenario2, scenario3]);

        scenario1.Given(context).Returns(BehaviourPhase.On);
        scenario1.When(context).Returns(true);
        scenario1.ThenAsync(context).Returns(context.Continue());

        scenario2.Given(context).Returns(BehaviourPhase.On);
        scenario2.When(context).Returns(false);
        scenario2.ThenAsync(context).Returns(context.Continue());

        scenario3.Given(context).Returns(BehaviourPhase.On);
        scenario3.When(context).Returns(true);
        scenario3.ThenAsync(context).Returns(context.Continue());

        var result = await BehaviourRunner.ExecuteAsync(context, [feature]);

        Assert.NotNull(result);
        Assert.NotNull(scenario1.Received(1).ThenAsync(context));
        Assert.NotNull(scenario2.DidNotReceive().ThenAsync(context));
        Assert.NotNull(scenario3.Received(1).ThenAsync(context));
    }

    [Fact]
    public async Task BehaviourRunner_ThrowException_Exit()
    {
        var context = new BehaviourContext(NullLogger.Instance);
        var feature = Substitute.For<BehaviourFeature>();
        var scenario1 = Substitute.For<BehaviourScenario>();
        var scenario2 = Substitute.For<BehaviourScenario>();
        var scenario3 = Substitute.For<BehaviourScenario>();

        feature.Given(context).Returns(true);
        feature.Scenarios.Returns([scenario1, scenario2, scenario3]);

        scenario1.Given(context).Returns(BehaviourPhase.On);
        scenario1.When(context).Returns(true);
        scenario1.ThenAsync(context).Returns(context.Continue());

        scenario2.Given(context).Returns(BehaviourPhase.On);
        scenario2.When(context).Returns(true);
        scenario2.ThenAsync(context).ThrowsAsync<InvalidOperationException>();

        scenario3.Given(context).Returns(BehaviourPhase.On);
        scenario3.When(context).Returns(true);
        scenario3.ThenAsync(context).Returns(context.Continue());

        var result = await BehaviourRunner.ExecuteAsync(context, [feature]);

        Assert.NotNull(result);
        Assert.NotNull(scenario1.Received(1).ThenAsync(context));
        Assert.NotNull(scenario2.Received(1).ThenAsync(context));
        Assert.NotNull(scenario3.DidNotReceive().ThenAsync(context));
    }

    [Fact]
    public async Task BehaviourRunner_NotContinue_SkipScenario()
    {
        var context = new BehaviourContext(NullLogger.Instance);
        var feature = Substitute.For<BehaviourFeature>();
        var scenario1 = Substitute.For<BehaviourScenario>();
        var scenario2 = Substitute.For<BehaviourScenario>();
        var scenario3 = Substitute.For<BehaviourScenario>();

        feature.Given(context).Returns(true);
        feature.Scenarios.Returns([scenario1, scenario2, scenario3]);

        scenario1.Given(context).Returns(BehaviourPhase.On);
        scenario1.When(context).Returns(true);
        scenario1.ThenAsync(context).Returns(context.Continue());

        scenario2.Given(context).Returns(BehaviourPhase.On);
        scenario2.When(context).Returns(true);
        scenario2.ThenAsync(context).Returns(context.NotContinue());

        scenario3.Given(context).Returns(BehaviourPhase.On);
        scenario3.When(context).Returns(true);
        scenario3.ThenAsync(context).Returns(context.Continue());

        var result = await BehaviourRunner.ExecuteAsync(context, [feature]);

        Assert.NotNull(result);
        Assert.NotNull(scenario1.Received(1).ThenAsync(context));
        Assert.NotNull(scenario2.Received(1).ThenAsync(context));
        Assert.NotNull(scenario3.DidNotReceive().ThenAsync(context));
    }

    [Fact]
    public async Task BehaviourRunner_MultipleFeature_ExecuteAll()
    {
        var context = new BehaviourContext(NullLogger.Instance);
        var feature1 = Substitute.For<BehaviourFeature>();
        var feature2 = Substitute.For<BehaviourFeature>();
        var feature3 = Substitute.For<BehaviourFeature>();
        var scenario1 = Substitute.For<BehaviourScenario>();
        var scenario2 = Substitute.For<BehaviourScenario>();
        var scenario3 = Substitute.For<BehaviourScenario>();

        feature1.Given(context).Returns(false);
        feature1.Scenarios.Returns([scenario1]);

        feature2.Given(context).Returns(true);
        feature2.Scenarios.Returns([scenario2]);

        feature3.Given(context).Returns(true);
        feature3.Scenarios.Returns([scenario3]);

        scenario1.Given(context).Returns(BehaviourPhase.On);
        scenario1.When(context).Returns(true);
        scenario1.ThenAsync(context).Returns(context.Continue());

        scenario2.Given(context).Returns(BehaviourPhase.On);
        scenario2.When(context).Returns(true);
        scenario2.ThenAsync(context).Returns(context.Continue());

        scenario3.Given(context).Returns(BehaviourPhase.On);
        scenario3.When(context).Returns(true);
        scenario3.ThenAsync(context).Returns(context.Continue());

        var result = await BehaviourRunner.ExecuteAsync(context, [feature1, feature2, feature3]);

        Assert.NotNull(result);
        Assert.NotNull(scenario1.DidNotReceive().ThenAsync(context));
        Assert.NotNull(scenario2.Received(1).ThenAsync(context));
        Assert.NotNull(scenario3.Received(1).ThenAsync(context));
    }

    [Fact]
    public async Task BehaviourRunner_FeatureFlag_SkipScenario()
    {
        var context = new BehaviourContext(NullLogger.Instance);
        var feature1 = Substitute.For<BehaviourFeature>();
        var feature2 = Substitute.For<BehaviourFeature>();
        var feature3 = Substitute.For<BehaviourFeature>();
        var scenario1 = Substitute.For<BehaviourScenario>();
        var scenario2 = Substitute.For<BehaviourScenario>();
        var scenario3 = Substitute.For<BehaviourScenario>();

        feature1.FeatureName.Returns("Feature1");
        feature1.Given(context).Returns(true);
        feature1.Scenarios.Returns([scenario1]);

        feature2.FeatureName.Returns("Feature2");
        feature2.Given(context).Returns(true);
        feature2.Scenarios.Returns([scenario2]);

        feature3.FeatureName.Returns("Feature3");
        feature3.Given(context).Returns(true);
        feature3.Scenarios.Returns([scenario3]);

        scenario1.Given(context).Returns(BehaviourPhase.On);
        scenario1.When(context).Returns(true);
        scenario1.ThenAsync(context).Returns(context.Continue());

        scenario2.Given(context).Returns(BehaviourPhase.On);
        scenario2.When(context).Returns(true);
        scenario2.ThenAsync(context).Returns(context.Continue());

        scenario3.Given(context).Returns(BehaviourPhase.On);
        scenario3.When(context).Returns(true);
        scenario3.ThenAsync(context).Returns(context.Continue());

        var result = await BehaviourRunner.ExecuteAsync(context, [feature1, feature2, feature3], featureName => featureName != "Feature1");

        Assert.NotNull(result);
        Assert.NotNull(scenario1.DidNotReceive().ThenAsync(context));
        Assert.NotNull(scenario2.Received(1).ThenAsync(context));
        Assert.NotNull(scenario3.Received(1).ThenAsync(context));
    }
}