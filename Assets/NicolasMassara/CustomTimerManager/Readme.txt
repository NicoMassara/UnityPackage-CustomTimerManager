CustomTimerManager for Unity - Quick Start Guide

---

Thank you for downloading my CustomTimerManager!

This TimerManager is designed to be easy to use and flexible.

### Creating a Timer

To create a timer, simply call:

```csharp
TimerManager.Add(timerData);
```

This requires a `TimerData` object, which holds all the information needed for the timer to run:

* **Time**: Duration of the timer (in seconds).

* **Frequency**: Update frequency. This is an enum (`UpdateFrequency`) that lets you choose how often the timer updates.
  * Note: If you use `UpdateFrequency.EverySecond`, use a whole number for the time.
  
* **OnStartAction**: Action to execute when the timer starts.

* **OnEndAction**: Action to execute when the timer ends.

### TimerGeneratedId

Every call to `TimerManager.Add()` returns a `TimerGeneratedId`. This ID is unique (ulong) and is required if you want to cancel the timer later.

* To cancel a timer:

```csharp
TimerManager.Remove(timerGeneratedId);
```

* It's recommended to store the `TimerGeneratedId` in a variable or property if you might need to cancel the timer.

### Customization

You can adapt TimerManager to your needs, such as integrating it with a custom update manager.

### Feedback

If you find TimerManager useful, please consider rating it and leaving a comment!

Feel free to use it in the TestScene!

------------------------------------
Created by Nicolás Federico Massara

Linkedin: in/nicolas-federico-massara-95818322a
Github: github.com/NicoMassara;
Itch.io: https://nicolasmassara.itch.io/
Portfolio: https://nicomassara.github.io/
Email: nicolasmassara@hotmail.com
