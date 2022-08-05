//-----------------------------------------------------------------------------
// FILE:		contexts.go
// CONTRIBUTOR: John C Burns
// COPYRIGHT:	Copyright (c) 2016-2019 by neonFORGE, LLC.  All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

package workflow

import (
	"sync"

	"go.uber.org/cadence/workflow"
)

var (
	mu sync.RWMutex

	// contextID is incremented (protected by a mutex) every time
	// a new cadence workflow.Context is created
	contextID int64
)

type (

	// ContextsMap is a global map of int64 contextID to
	// running cadence workflow instances (as *WorkflowContext)
	ContextsMap struct {
		sync.Mutex
		contexts map[int64]*Context
	}

	// Context represents a running cadence
	// workflow instance
	Context struct {
		sync.Mutex                       // allows us to safely iterate ID iterator
		workflowName *string             // string name of the workflow
		ctx          workflow.Context    // the cadence workflow context
		cancelFunc   workflow.CancelFunc // cadence workflow context cancel function
		children     *ChildMap           // maps child workflow instances to childID
		activities   *ActivityMap        // maps activity futures launched by the workflow instance to activityID
		queues       *QueueMap           // map of workflow queues (queueID to workflow.Channel queue)
		childID      int64               // childID iterator
		queueID      int64               // queueID iterator
	}
)

//----------------------------------------------------------------------------
// contextID methods

// NextContextID increments the global variable
// contextID by 1 and is protected by a mutex lock
func NextContextID() int64 {
	mu.Lock()
	contextID = contextID + 1
	defer mu.Unlock()
	return contextID
}

// GetContextID gets the value of the global variable
// contextID and is protected by a mutex Read lock
func GetContextID() int64 {
	mu.RLock()
	defer mu.RUnlock()
	return contextID
}

//----------------------------------------------------------------------------
// WorkflowContext instance methods

// NewWorkflowContext is the default constructor
// for a WorkflowContext struct
//
// returns *WorkflowContext -> pointer to a newly initialized
// workflow ExecutionContext in memory
func NewWorkflowContext(ctx workflow.Context) *Context {
	wectx := new(Context)
	wectx.children = NewChildMap()
	wectx.activities = NewActivityMap()
	wectx.queues = NewQueueMap()
	wectx.SetContext(ctx)
	return wectx
}

// GetContext gets a WorkflowContext's workflow.Context
//
// returns workflow.Context -> a cadence workflow context
func (wectx *Context) GetContext() workflow.Context {
	return wectx.ctx
}

// SetContext sets a WorkflowContext's workflow.Context
//
// param value workflow.Context -> a cadence workflow context to be
// set as a WorkflowContext's cadence workflow.Context
func (wectx *Context) SetContext(value workflow.Context) {
	wectx.ctx = value
}

// GetWorkflowName gets a WorkflowContext's workflow function name
//
// returns *string -> a cadence workflow function name
func (wectx *Context) GetWorkflowName() *string {
	return wectx.workflowName
}

// SetWorkflowName sets a WorkflowContext's workflow function name
//
// param value *string -> a cadence workflow function name
func (wectx *Context) SetWorkflowName(value *string) {
	wectx.workflowName = value
}

// GetCancelFunction gets a WorkflowContext's context cancel function
//
// returns workflow.CancelFunc -> a cadence workflow context cancel function
func (wectx *Context) GetCancelFunction() workflow.CancelFunc {
	return wectx.cancelFunc
}

// SetCancelFunction sets a WorkflowContext's cancel function
//
// param value workflow.CancelFunc -> a cadence workflow context cancel function
func (wectx *Context) SetCancelFunction(value workflow.CancelFunc) {
	wectx.cancelFunc = value
}

// GetChildren gets a WorkflowContext's child contexts map
//
// returns *ChildMap -> a cadence workflow child contexts map
func (wectx *Context) GetChildren() *ChildMap {
	return wectx.children
}

// SetChildren sets a WorkflowContext's child contexts map
//
// param value *ChildMap -> a cadence workflow child contexts map
func (wectx *Context) SetChildren(value *ChildMap) {
	wectx.children = value
}

// AddChild adds a new cadence context and its corresponding ContextId into
// the WorkflowContext's children map.  This method is thread-safe.
//
// param id int64 -> the long childId. This will be the mapped key
//
// param cctx *Child -> pointer to the new WorkflowContex used to
// execute workflow functions. This will be the mapped value
//
// returns int64 -> long id of the new Child added to the map
func (wectx *Context) AddChild(id int64, cctx *Child) int64 {
	return wectx.children.Add(id, cctx)
}

// RemoveChild removes key/value entry from the WorkflowContext's
// children map at the specified
// ContextId.  This is a thread-safe method.
//
// param id int64 -> the long childId.
//
// returns int64 -> long id of the Child removed from the map
func (wectx *Context) RemoveChild(id int64) int64 {
	return wectx.children.Remove(id)
}

// GetChild gets a childContext from the WorkflowContext's
// ChildMap at the specified ContextID.
// This method is thread-safe.
//
// param id int64 -> the long childId. This will be the mapped key
//
// returns *WorkflowContext -> pointer to Child with the specified id
func (wectx *Context) GetChild(id int64) *Child {
	return wectx.children.Get(id)
}

// GetActivities gets a WorkflowContext's activity futures map
//
// returns *proxyactivity.FuturesMap -> map of an executing workflow's activities.
func (wectx *Context) GetActivities() *ActivityMap {
	return wectx.activities
}

// SetActivities sets a WorkflowContext's activity futures.
//
// param value *proxyactivity.FuturesMap -> a cadence workflow activity futures map
func (wectx *Context) SetActivities(value *ActivityMap) {
	wectx.activities = value
}

// AddActivity adds a new Activity to the ActivityMap
//
// param id int64 -> the long activity id.
//
// param activity Activity -> executing activity.
//
// returns int64 -> activity id.
func (wectx *Context) AddActivity(id int64, activity Activity) int64 {
	return wectx.activities.Add(id, activity)
}

// RemoveActivity removes key/value entry from the WorkflowContext's
// activities map at the specified id.  This is a thread-safe method.
//
// param id int64 -> the long activity id.
//
// returns int64 -> activity id of removed activity.
func (wectx *Context) RemoveActivity(id int64) int64 {
	return wectx.activities.Remove(id)
}

// GetActivity gets the activity of an executing workflow activity at
// the specified activity id.
//
// param id int64 -> the long activity id.
//
// returns Activity -> the activity at the specified Id.
func (wectx *Context) GetActivity(id int64) Activity {
	return wectx.activities.Get(id)
}

// GetQueues gets a WorkflowContext's QueueMap
//
// returns *QueueMap -> a map of workflow queues (queueID to a chan []byte queue).
func (wectx *Context) GetQueues() *QueueMap {
	return wectx.queues
}

// SetQueues sets a WorkflowContext's QueueMap
//
// param value *QueueMap -> a map of workflow queues (queueID to a chan []byte queue).
func (wectx *Context) SetQueues(value *QueueMap) {
	wectx.queues = value
}

// AddQueue adds a new queue to the WorkflowContext. This method is thread-safe.
//
// param id int64 -> the long queueID. This will be the mapped key.
//
// param b workflow.Channel -> the workflow.Channel workflow queue. This will be the mapped value.
//
// returns int64 -> long queueID of the newly added queue.
func (wectx *Context) AddQueue(id int64, b workflow.Channel) int64 {
	return wectx.queues.Add(id, b)
}

// RemoveQueue removes key/value entry from the WorkflowContext's
// queues map at the specified queueID.  This is a thread-safe method.
//
// param id int64 -> the long queueID.
//
// returns int64 -> the long queueID of the Queue to be removed from the map.
func (wectx *Context) RemoveQueue(id int64) int64 {
	return wectx.queues.Remove(id)
}

// GetQueue gets a workflow.Channel workflow queue from the WorkflowContext's
// QueueMap at the specified queueID. This method is thread-safe.
//
// param id int64 -> the long queueID.
//
// returns workflow.Channel -> the workflow.Channel workflow queue at the specified
// queueID.
func (wectx *Context) GetQueue(id int64) workflow.Channel {
	return wectx.queues.Get(id)
}

// NextChildID increments the variable
// childID by 1 and is protected by a mutex lock
func (wectx *Context) NextChildID() int64 {
	wectx.Lock()
	wectx.childID = wectx.childID + 1
	defer wectx.Unlock()
	return wectx.childID
}

// GetChildID gets the value of the variable
// childID and is protected by a mutex lock
func (wectx *Context) GetChildID() int64 {
	wectx.Lock()
	defer wectx.Unlock()
	return wectx.childID
}

// NextQueueID increments the variable
// childID by 1 and is protected by a mutex lock
func (wectx *Context) NextQueueID() int64 {
	wectx.Lock()
	wectx.queueID = wectx.queueID + 1
	defer wectx.Unlock()
	return wectx.queueID
}

// GetQueueID gets the value of the variable
// queueID and is protected by a mutex lock
func (wectx *Context) GetQueueID() int64 {
	wectx.Lock()
	defer wectx.Unlock()
	return wectx.queueID
}

//----------------------------------------------------------------------------
// WorkflowContextsMap instance methods

// NewWorkflowContextsMap is the constructor for an WorkflowContextsMap
func NewWorkflowContextsMap() *ContextsMap {
	o := new(ContextsMap)
	o.contexts = make(map[int64]*Context)
	return o
}

// Add adds a new cadence context and its corresponding ContextId into
// the WorkflowContextsMap map.  This method is thread-safe.
//
// param contextID int64 -> the long id contextID of a executing
// cadence workflow.
//
// param wectx *WorkflowContext -> pointer to the new WorkflowContex used to
// execute workflow functions. This will be the mapped value
//
// returns int64 -> long id of the new cadence WorkflowContext added to the map
func (w *ContextsMap) Add(contextID int64, wectx *Context) int64 {
	w.Lock()
	defer w.Unlock()
	w.contexts[contextID] = wectx
	return contextID
}

// Remove removes key/value entry from the WorkflowContextsMap map at the specified
// ContextId.  This is a thread-safe method.
//
// param contextID int64 -> the long id contextID of a executing
// cadence workflow.
//
// returns int64 -> long id of the WorkflowContext removed from the map
func (w *ContextsMap) Remove(contextID int64) int64 {
	w.Lock()
	defer w.Unlock()
	delete(w.contexts, contextID)
	return contextID
}

// Get gets a WorkflowContext from the WorkflowContextsMap at the specified
// ContextID.  This method is thread-safe.
//
// param contextID int64 -> the long id contextID of a executing
// cadence workflow.
//
// returns *WorkflowContext -> pointer to WorkflowContext with the specified id
func (w *ContextsMap) Get(contextID int64) *Context {
	w.Lock()
	defer w.Unlock()
	return w.contexts[contextID]
}
