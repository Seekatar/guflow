﻿using Amazon.SimpleWorkflow;
using Amazon.SimpleWorkflow.Model;

namespace NetPlayground
{
    internal static class HistoryEventsExtension
    {
        public static bool IsActivityCompletedEvent(this HistoryEvent historyEvent)
        {
            return historyEvent.EventType == EventType.ActivityTaskCompleted;
        }

        public static bool IsActivityFailedEvent(this HistoryEvent historyEvent)
        {
            return historyEvent.EventType == EventType.ActivityTaskFailed;
        }

        public static bool IsActivityTimedoutEvent(this HistoryEvent historyEvent)
        {
            return historyEvent.EventType == EventType.ActivityTaskTimedOut;
        }

        public static bool IsActivityStartedEventFor(this HistoryEvent historyEvent, long startedEventId)
        {
            return historyEvent.EventType == EventType.ActivityTaskStarted && historyEvent.EventId == startedEventId;
        }

        public static bool IsActivityScheduledEventFor(this HistoryEvent historyEvent, long scheduledEventId)
        {
            return historyEvent.EventType == EventType.ActivityTaskScheduled && historyEvent.EventId == scheduledEventId;
        }
    }
}