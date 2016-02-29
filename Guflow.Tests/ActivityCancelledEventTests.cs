﻿using System.Linq;
using Guflow.Tests.TestWorkflows;
using Moq;
using NUnit.Framework;

namespace Guflow.Tests
{
    [TestFixture]
    public class ActivityCancelledEventTests
    {
        private ActivityCancelledEvent _activityCancelledEvent;
        private const string _activityName = "Download";
        private const string _activityVersion = "1.0";
        private const string _positionalName = "First";
        private const string _identity = "machine name";
        private const string _detail = "detail";

        [SetUp]
        public void Setup()
        {
            var cancelledActivityEventGraph = HistoryEventFactory.CreateActivityCancelledEventGraph(_activityName, _activityVersion, _positionalName, _identity, _detail);
            _activityCancelledEvent = new ActivityCancelledEvent(cancelledActivityEventGraph.First(), cancelledActivityEventGraph);
        }

        [Test]
        public void Should_populate_properties_from_event_attributes()
        {
            Assert.That(_activityCancelledEvent.Details, Is.EqualTo(_detail));
            Assert.That(_activityCancelledEvent.Name, Is.EqualTo(_activityName));
            Assert.That(_activityCancelledEvent.Version, Is.EqualTo(_activityVersion));
            Assert.That(_activityCancelledEvent.PositionalName, Is.EqualTo(_positionalName));
            Assert.That(_activityCancelledEvent.WorkerIdentity, Is.EqualTo(_identity));
        }

        [Test]
        public void By_default_return_cancel_workflow_action()
        {
            var workflow = new SingleActivityWorkflow();

            var workflowAction = _activityCancelledEvent.Interpret(workflow);

            Assert.That(workflowAction, Is.EqualTo(WorkflowAction.CancelWorkflow(_detail)) );
        }

        [Test]
        public void Throws_exception_when_completed_activity_is_not_found_in_workflow()
        {
            var incompatibleWorkflow = new EmptyWorkflow();

            Assert.Throws<IncompatibleWorkflowException>(() => _activityCancelledEvent.Interpret(incompatibleWorkflow));
        }

        [Test]
        public void Can_return_custom_workflow_action()
        {
            var workflowAction = new Mock<WorkflowAction>();
            var workflow = new WorkflowWithCustomAction(workflowAction.Object);

            var actualAction = _activityCancelledEvent.Interpret(workflow);

            Assert.That(actualAction,Is.EqualTo(workflowAction.Object));
        }

        private class SingleActivityWorkflow : Workflow
        {
            public SingleActivityWorkflow()
            {
                AddActivity(_activityName, _activityVersion, _positionalName);
            }
        }

        private class WorkflowWithCustomAction : Workflow
        {
            public WorkflowWithCustomAction(WorkflowAction workflowAction)
            {
                AddActivity(_activityName, _activityVersion, _positionalName).OnCancelled(c => workflowAction);
            }
        }
    }
}