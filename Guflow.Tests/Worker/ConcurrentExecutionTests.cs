﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amazon.SimpleWorkflow;
using Amazon.SimpleWorkflow.Model;
using Guflow.Worker;
using Moq;
using NUnit.Framework;

namespace Guflow.Tests.Worker
{
    [TestFixture]
    public class ConcurrentExecutionTests
    {
        private HostedActivities _hostedActivities;
        private Mock<IAmazonSimpleWorkflow> _amazonSimpleWorkflow;
        private const string _activityResult = "result";
        private const string _taskToken = "token";
        [SetUp]
        public void Setup()
        {
            _amazonSimpleWorkflow = new Mock<IAmazonSimpleWorkflow>();
            _hostedActivities = new HostedActivities(new Domain("name", _amazonSimpleWorkflow.Object), new[] { typeof(TestActivity) }, t=> new TestActivity(_activityResult));
            TestActivity.Reset();
        }
        [Test]
        public async Task Can_limit_the_activity_execution()
        {
            var concurrentExecution = ConcurrentExecution.LimitTo(2);
            concurrentExecution.Set(_hostedActivities);

            await concurrentExecution.Execute(NewWorkerTask());
            await concurrentExecution.Execute(NewWorkerTask());
            await concurrentExecution.Execute(NewWorkerTask());
            await concurrentExecution.Execute(NewWorkerTask());


            Assert.That(TestActivity.MaxConcurrentExecution , Is.EqualTo(2));
        }

        [Test]
        public async Task Can_execute_tasks_in_sequence()
        {
            var concurrentExecution = ConcurrentExecution.LimitTo(1);
            concurrentExecution.Set(_hostedActivities);

            await concurrentExecution.Execute(NewWorkerTask());
            await concurrentExecution.Execute(NewWorkerTask());
            await concurrentExecution.Execute(NewWorkerTask());
            await concurrentExecution.Execute(NewWorkerTask());


            Assert.That(TestActivity.MaxConcurrentExecution, Is.EqualTo(1));
        }

        [Test]
        public void Throws_exception_when_limit_is_zero()
        {
            Assert.Throws<ArgumentException>(() => ConcurrentExecution.LimitTo(0));
        }

        [Test]
        public async Task Send_activity_response_to_amazon_client()
        {
            var concurrentExecution = ConcurrentExecution.LimitTo(1);
            concurrentExecution.Set(_hostedActivities);

            await concurrentExecution.Execute(NewWorkerTask());

            Func<RespondActivityTaskCompletedRequest, bool> request = r =>
            {
                Assert.That(r.Result, Is.EqualTo(_activityResult));
                Assert.That(r.TaskToken, Is.EqualTo(_taskToken));
                return true;
            };
            _amazonSimpleWorkflow.Verify(w=>w.RespondActivityTaskCompleted(It.Is<RespondActivityTaskCompletedRequest>(r=>request(r))), Times.Once);
        }

        private static WorkerTask NewWorkerTask()
        {
            return WorkerTask.CreateFor(new ActivityTask()
            {
                ActivityType = new ActivityType() { Name = "TestActivity", Version = "1.0" },
                TaskToken = "token",
                WorkflowExecution = new WorkflowExecution(){ RunId = "rid", WorkflowId = "wid"},
                Input = "input"
            });
        }

        [ActivityDescription("1.0")]
        private class TestActivity : Activity
        {
            private static int _noOfConcurrentTasks;
            private static ConcurrentBag<int> _concurrentTaskRecords = new ConcurrentBag<int>();

            private readonly string _result;

            public TestActivity(string result)
            {
                _result = result;
            }

            [Execute]
            public async Task<ActivityResponse> Execute()
            {
                _concurrentTaskRecords.Add(Interlocked.Increment(ref _noOfConcurrentTasks));
                await Task.Delay(100);
                Interlocked.Decrement(ref _noOfConcurrentTasks);
                return Complete(_result);
            }

            public static int MaxConcurrentExecution
            {
                get { return _concurrentTaskRecords.Max(); }
            }

            public static void Reset()
            {
                _concurrentTaskRecords = new ConcurrentBag<int>();
            }
        }
    }
}