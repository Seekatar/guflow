﻿// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.

using System;
using System.Linq;
using Guflow.Decider;
using NUnit.Framework;

namespace Guflow.Tests.Decider
{
    [TestFixture]
    public class ActivityScheduleTests
    {
        private const string ActivityName = "Activity1";
        private const string ActivityVersion = "1.0";
        private const string TimerName = "Timer1";

        private const string LambdaName = "LambdaName";
        private const string ParentActivityName = "ParentActivity";
        private const string ParentActivityVersion = "2.0";

        private HistoryEventsBuilder _builder;

        [SetUp]
        public void Setup()
        {
            _builder = new HistoryEventsBuilder();
        }

        [Test]
        public void Activity_can_be_scheduled_after_lambda()
        {
            var eventGraph = LambdaCompletedEventGraph();
            var decision = new ActivityAfterLambdaWorkflow().Interpret(eventGraph);

            Assert.That(decision, Is.EqualTo(new[] { new ScheduleActivityDecision(Identity.New(ActivityName, ActivityVersion)) }));
        }

        [Test]
        public void Activity_can_be_scheduled_after_time()
        {
            var eventGraph = TimerCompletedEventGraph();
            var decision = new ActivityAfterTimerWorkflow().Interpret(eventGraph);

            Assert.That(decision, Is.EqualTo(new[] { new ScheduleActivityDecision(Identity.New(ActivityName, ActivityVersion)) }));
        }

        [Test]
        public void Activity_can_be_scheduled_after_activity()
        {
            var eventGraph = ActivityEventGraph();
            var decision = new ActivityAfterActivityWorkflow().Interpret(eventGraph);

            Assert.That(decision, Is.EqualTo(new[] { new ScheduleActivityDecision(Identity.New(ActivityName, ActivityVersion)) }));
        }

        private WorkflowHistoryEvents ActivityEventGraph()
        {
            var startedEvent = _builder.WorkflowStartedEvent();
            var completedEvent = _builder.ActivityCompletedGraph(Identity.New(ParentActivityName, ParentActivityVersion), "id",
                "res");
            return new WorkflowHistoryEvents(completedEvent.Concat(new[] { startedEvent }), completedEvent.Last().EventId, completedEvent.First().EventId);
        }

        private WorkflowHistoryEvents TimerCompletedEventGraph()
        {
            var startedEvent = _builder.WorkflowStartedEvent();
            var completedEvent = _builder.TimerFiredGraph(Identity.Timer(TimerName), TimeSpan.FromSeconds(2));
            return new WorkflowHistoryEvents(completedEvent.Concat(new[] { startedEvent }), completedEvent.Last().EventId, completedEvent.First().EventId);
        }

        private WorkflowHistoryEvents LambdaCompletedEventGraph()
        {
            var startedEvent = _builder.WorkflowStartedEvent();
            var completedEvent = _builder.LambdaCompletedEventGraph(Identity.Lambda(LambdaName), "input", "result", "cont");
            return new WorkflowHistoryEvents(completedEvent.Concat(new[] { startedEvent }), completedEvent.Last().EventId, completedEvent.First().EventId);
        }
        private class ActivityAfterLambdaWorkflow : Workflow
        {
            public ActivityAfterLambdaWorkflow()
            {
                ScheduleLambda(LambdaName);
                ScheduleActivity(ActivityName, ActivityVersion).AfterLambda(LambdaName);
            }
        }
        private class ActivityAfterTimerWorkflow : Workflow
        {
            public ActivityAfterTimerWorkflow()
            {
                ScheduleTimer(TimerName);
                ScheduleActivity(ActivityName, ActivityVersion).AfterTimer(TimerName);
            }
        }
        private class ActivityAfterActivityWorkflow : Workflow
        {
            public ActivityAfterActivityWorkflow()
            {
                ScheduleActivity(ParentActivityName, ParentActivityVersion);
                ScheduleActivity(ActivityName, ActivityVersion)
                    .AfterActivity(ParentActivityName, ParentActivityVersion);
            }
        }
    }
}