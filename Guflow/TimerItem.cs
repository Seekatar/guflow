﻿using System;
using System.Collections.Generic;

namespace Guflow
{
    internal sealed class TimerItem : WorkflowItem, IFluentTimerItem, ITimerItem
    {
        private TimeSpan _fireAfter= new TimeSpan();
        private Func<TimerFiredEvent, WorkflowAction> _onFiredAction;
        private Func<TimerCancellationFailedEvent, WorkflowAction> _onCanellationFailedAction;
        private Func<TimerStartFailedEvent, WorkflowAction> _onStartFailureAction;
        private Func<TimerCancelledEvent, WorkflowAction> _onTimerCancelledAction; 
        private Func<TimerItem, bool> _whenFunc;
        private readonly bool _isRescheduleTimer;
        internal TimerItem(Identity identity, IWorkflow workflow, bool isRescheduleTimer=false):base(identity,workflow)
        {
            _isRescheduleTimer = isRescheduleTimer;
            _onFiredAction = f=>WorkflowAction.ContinueWorkflow(this);
            _onCanellationFailedAction = c => WorkflowAction.FailWorkflow("TIMER_CANCELLATION_FAILED", c.Cause);
            _onStartFailureAction = c => WorkflowAction.FailWorkflow("TIMER_START_FAILED", c.Cause);
            _onTimerCancelledAction = c => WorkflowAction.CancelWorkflow("TIMER_CANCELLED");
            _whenFunc = t => true;
        }
        public override WorkflowItemEvent LastEvent
        {
            get { return WorkflowHistoryEvents.LastTimerEventFor(this); }
        }

        public override IEnumerable<WorkflowItemEvent> AllEvents
        {
            get { return WorkflowHistoryEvents.AllTimerEventsFor(this); }
        }
        public IFluentTimerItem FireAfter(TimeSpan fireAfter)
        {
            _fireAfter = fireAfter;
            return this;
        }
        public IFluentTimerItem When(Func<ITimerItem, bool> whenFunc)
        {
            _whenFunc = whenFunc;
            return this;
        }
        public IFluentTimerItem OnFired(Func<TimerFiredEvent, WorkflowAction> onFiredAction)
        {
            _onFiredAction = onFiredAction;
            return this;
        }
        public IFluentTimerItem After(string timerName)
        {
            AddParent(Identity.Timer(timerName));
            return this;
        }

        public IFluentTimerItem After(string activityName, string activityVersion, string positionalName = "")
        {
            AddParent(Identity.New(activityName, activityVersion, positionalName));
            return this;
        }

        public IFluentTimerItem OnCancelled(Func<TimerCancelledEvent, WorkflowAction> onCancelledAction)
        {
            _onTimerCancelledAction = onCancelledAction;
            return this;
        }
        public IFluentTimerItem OnFailedCancellation(Func<TimerCancellationFailedEvent, WorkflowAction> onCancellationFailedAction)
        {
            _onCanellationFailedAction = onCancellationFailedAction;
            return this;
        }

        public IFluentTimerItem OnStartFailure(Func<TimerStartFailedEvent, WorkflowAction> onStartFailureAction)
        {
            _onStartFailureAction = onStartFailureAction;
            return this;
        }

        internal override WorkflowDecision GetScheduleDecision()
        {
            if(!_whenFunc(this))
                return WorkflowDecision.Empty;

            return new ScheduleTimerDecision(Identity, _fireAfter,_isRescheduleTimer);
        }
        internal override WorkflowDecision GetCancelDecision()
        {
            return new CancelTimerDecision(Identity);
        }

        internal override WorkflowAction TimerFired(TimerFiredEvent timerFiredEvent)
        {
            if (timerFiredEvent.IsARescheduledTimer)
                return RescheduleTimerItem._onFiredAction(timerFiredEvent);

            return _onFiredAction(timerFiredEvent);
        }

        internal override WorkflowAction TimerCancelled(TimerCancelledEvent timerCancelledEvent)
        {
            return _onTimerCancelledAction(timerCancelledEvent);
        }

        internal override WorkflowAction TimerStartFailed(TimerStartFailedEvent timerStartFailedEvent)
        {
            return _onStartFailureAction(timerStartFailedEvent);
        }

        internal override WorkflowAction TimerCancellationFailed(TimerCancellationFailedEvent timerCancellationFailedEvent)
        {
            return _onCanellationFailedAction(timerCancellationFailedEvent);
        }
    }
}
