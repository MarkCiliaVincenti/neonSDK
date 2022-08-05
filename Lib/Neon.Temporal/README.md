﻿Neon.Temporal
============

---

**NOTE:** We are actively working on porting **Neon.Cadence** to [temporal.io](https://temporal.io/) here.  This is still very much a work in progress and is not yet for ready general use.

---

This is .NET client for the **Temporal Workflow Platform**.  Temporal is a fork of the Uber Cadence project brought to you by the original creators of Uber Cadence as well as the Amazon AWS Simple Workflow Foundation (SWF).  We expect to see most, if not all significant future advancements for this platform comming from Temporal.

You can get started with this here: [Neon.Temporal](https://doc.neonkube.com/Neon.Temporal-Overview.htm)

**Neon.Cadence** is our older project that supports Uber Cadence and is fairly complete and we've been using it production for over nearly a year now for relatively simple workflows.  The .NET Temporal API will be closly aligned with **Neon.Cadence** so using Cadence isn't a bad place to get started if you're itching to try this out on .NET.  There will be breaking changes; we're renaming "Cadence" to "Temporal" throughout the API, changing the term "domain" to "namespace" and "task list" to "task queue" like the other Temporal clients have, and there will be some changes to align with the Temporal ecosystem.  But, the major API concepts will remain the same.

You can get started with Cadence for .NET here: [Neon.Cadence](https://doc.neonkube.com/Neon.Cadence-Overview.htm) and we also have some [.NET Temporal Samples](https://github.com/nforgeio/temporal-samples).

Once **Neon.Temporal** is usable, we're planning on deprecating **Neon.Cadence** to focus our efforts on Temporal long term.
