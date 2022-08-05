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

package activity

import (
	"context"
	"sync"
)

var (
	mu sync.RWMutex

	// contextID is incremented (protected by a mutex) every time
	// a new cadence context.Context is created
	contextID int64
)

type (

	// ContextsMap holds a thread-safe map[interface{}]interface{} of
	// ActivityContexts with their contextID's
	ContextsMap struct {
		sync.Mutex
		contexts map[int64]*Context
	}

	// Context holds a Cadence activity
	// context, the registered activity function.
	// This struct is used as an intermediate for storing worklfow information
	// and state while registering and executing cadence activitys
	Context struct {
		ctx          context.Context
		activityName *string
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
// ActivityContext instance methods

// NewActivityContext is the default constructor
// for a ActivityContext struct
//
// returns *ActivityContext -> pointer to a newly initialized
// activity ExecutionContext in memory
func NewActivityContext(ctx context.Context) *Context {
	actx := new(Context)
	actx.SetContext(ctx)
	return actx
}

// GetContext gets a ActivityContext's context.Context
//
// returns context.Context -> a cadence context context
func (actx *Context) GetContext() context.Context {
	return actx.ctx
}

// SetContext sets a ActivityContext's context.Context
//
// param value context.Context -> a cadence activity context to be
// set as a ActivityContext's cadence context.Context
func (actx *Context) SetContext(value context.Context) {
	actx.ctx = value
}

// GetActivityName gets a ActivityContext's activity function name
//
// returns *string -> a cadence activity function name
func (actx *Context) GetActivityName() *string {
	return actx.activityName
}

// SetActivityName sets a ActivityContext's activity function name
//
// param value *string -> a cadence activity function name
func (actx *Context) SetActivityName(value *string) {
	actx.activityName = value
}

//----------------------------------------------------------------------------
// ActivityContextsMap instance methods

// NewActivityContextsMap is the constructor for an ActivityContextsMap
func NewActivityContextsMap() *ContextsMap {
	o := new(ContextsMap)
	o.contexts = make(map[int64]*Context)
	return o
}

// Add adds a new cadence context and its corresponding ContextId into
// the ActivityContextsMap map.  This method is thread-safe.
//
// param contextID int64 -> the long contextID of activity.
// This will be the mapped key.
//
// param actx *ActivityContext -> pointer to the new ActivityContex used to
// execute activity functions. This will be the mapped value
//
// returns int64 -> long contextID of the new cadence ActivityContext added to the map
func (a *ContextsMap) Add(contextID int64, actx *Context) int64 {
	a.Lock()
	defer a.Unlock()
	a.contexts[contextID] = actx
	return contextID
}

// Remove removes key/value entry from the ActivityContextsMap map at the specified
// ContextId.  This is a thread-safe method.
//
// param contextID int64 -> the long contextID of activity.
// This will be the mapped key.
//
// returns int64 -> long contextID of the ActivityContext removed from the map
func (a *ContextsMap) Remove(contextID int64) int64 {
	a.Lock()
	defer a.Unlock()
	delete(a.contexts, contextID)
	return contextID
}

// Get gets a ActivityContext from the ActivityContextsMap at the specified
// ContextID.  This method is thread-safe.
//
// param contextID int64 -> the long contextID of activity.
// This will be the mapped key.
//
// returns *ActivityContext -> pointer to ActivityContext with the specified id
func (a *ContextsMap) Get(contextID int64) *Context {
	a.Lock()
	defer a.Unlock()
	return a.contexts[contextID]
}
