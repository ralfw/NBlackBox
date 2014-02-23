NBlackBox
=========

With NBlackBox you can record domain events in a simple store and later replay them.

Usually this is called event sourcing with an event store as a persistence medium. But I like to compare it with recording data during a flight in a black box. This data represents "the truth" about the state changes of an air plane and can be played back to get insight into the movements of the air plane - especially in case of a crash.

Once recorded data cannot be changed. And depending on the use case the recorded data be played back selectively.

Usage samples:

```
using nblackbox;
...
var bb = new NBlackBox(@"c:\myblackbox");

bb.Record("eventname", "aggregateid", "data, data");
...

var events = bb.Player.Play(); // replays all events

// selective replay
events = bb.Player.ForEvent("eventname").Play();
events = bb.Player.ForEvent("e1", "e2", "e3", ...).Play();

bb.Player.WithContext("aggregateid").Play();
bb.Player.WithContext("a1", "a2", "a3", ...).Play();

bb.Player.FromIndex(42).Play(); // indexes are 0 based

bb.Player.FromIndex(42).WithContext("a1").ForEvent("e1", "e2").Play();

```

When recording an event the black box fires an event. It´s fired synchronously; subscribers should switch to another thread if their event processing is time consuming.

```
var bb = new NBlackBox(...);
bb.OnRecorded += recordedEvent => { ... };
...

```

Enjoy!

-Ralf Westphal, Hamburg, Germany