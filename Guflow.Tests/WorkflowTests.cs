﻿using NUnit.Framework;

namespace Guflow.Tests
{
    [TestFixture]
    public class WorkflowTests
    {
        [Test]
        public void Throws_exception_when_adding_the_same_item_again()
        {
            Assert.Throws<DuplicateItemException>(()=>new WorkflowWithSameActivity());
            Assert.Throws<DuplicateItemException>(() => new WorkflowWithSameTimer());
        }

        [Test]
        public void Throws_exception_when_parent_item_is_not_found()
        {
            Assert.Throws<ParentItemMissingException>(() => new WorkflowWithNonExistentParentActivityItem());
            Assert.Throws<ParentItemMissingException>(()=>new WorkflowWithNonExistentParentTimerItem());
        }

        private class WorkflowWithSameActivity : Workflow
        {
            public WorkflowWithSameActivity()
            {
                AddActivity("Download", "1.0");
                AddActivity("Download", "1.0");
            }
        }
        private class WorkflowWithSameTimer : Workflow
        {
            public WorkflowWithSameTimer()
            {
                AddTimer("Download");
                AddTimer("Download");
            }
        }

        private class WorkflowWithNonExistentParentActivityItem : Workflow
        {
            public WorkflowWithNonExistentParentActivityItem()
            {
                AddActivity("Name", "version").DependsOn("ParentName", "parentVer");
            }
        }
        private class WorkflowWithNonExistentParentTimerItem : Workflow
        {
            public WorkflowWithNonExistentParentTimerItem()
            {
                AddActivity("Name", "version").DependsOn("ParentName");
            }
        }
    }
}