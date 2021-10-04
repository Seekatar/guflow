// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using System;
using System.Threading.Tasks;
using Amazon;
using Amazon.Runtime;
using Amazon.SimpleWorkflow;
using Guflow.Decider;
using Guflow.Worker;

namespace Guflow.IntegrationTests
{
    public  class TestDomain
    {
        private readonly Domain _domain;
        public TestDomain()
        {
            var configuration = Configuration.Build();
            _domain = new Domain(configuration["AWSSWFDomain"], new AmazonSimpleWorkflowClient(new BasicAWSCredentials(configuration["AWSAccessKey"], configuration["AWSSecretKey"]), RegionEndpoint.GetBySystemName(configuration["AWSRegion"])));
        }

        public async Task<WorkflowHost> Host(params Workflow[] workflows)
        {
            await _domain.RegisterAsync(2);
            foreach (var workflow in workflows)
            {
                await _domain.RegisterWorkflowAsync(workflow.GetType());
            }
            
            return _domain.Host(workflows);
        }

        public async Task<ActivityHost> Host(Type[] activityTypes)
        {
            await _domain.RegisterAsync(2);
            foreach (var activityType in activityTypes)
            {
                await _domain.RegisterActivityAsync(activityType);
            }
            return _domain.Host(activityTypes);
        }

        public async Task<string> StartWorkflow<TWorkflow>(object input, string taskListName, string lambdaRole = null) where TWorkflow :Workflow
        {
            var workflowId = Guid.NewGuid().ToString();
            var startRequest = StartWorkflowRequest.For<TWorkflow>(workflowId);
            startRequest.TaskListName = taskListName;
            startRequest.Input = input;
            startRequest.LambdaRole = lambdaRole;
            await _domain.StartWorkflowAsync(startRequest);
            return workflowId;
        }

        public async Task SendSignal(string workflowId, string name, string input)
        {
            await _domain.SignalWorkflowAsync(new SignalWorkflowRequest(workflowId, name){ SignalInput = input });
        }
    }
}