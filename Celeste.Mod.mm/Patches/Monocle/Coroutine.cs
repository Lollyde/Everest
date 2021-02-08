﻿#pragma warning disable CS0414 // The field is assigned but its value is never used
#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value

using MonoMod;
using System.Collections;
using System.Collections.Generic;

namespace Monocle {
    class patch_Coroutine : Coroutine {

        // We're effectively in Coroutine, but still need to "expose" private fields to our mod.
        private float waitTimer;
        private Stack<IEnumerator> enumerators;

        /// <summary>
        /// Force this coroutine to mimic vanilla behavior:<br></br>
        /// When yield returning a new IEnumerator or when a previously IEnumerator finishes,
        /// the next IEnumerator runs delayed by one frame.<br></br>
        /// <br></br>
        /// To describe the behavior of this field, imagine the following code replacing all yield returns of IEnumerators:<br></br>
        /// <code>
        /// IEnumerator next = Nested(...);<br></br>
        /// if (ForceDelayedSwap) yield return null;<br></br>
        /// while (next.MoveNext()) yield return next.Current;<br></br>
        /// if (ForceDelayedSwap) yield return null;<br></br>
        /// // Control is returned to your code here.<br></br>
        /// </code>
        /// </summary>
        public bool ForceDelayedSwap;

        /// <summary>
        /// Forcibly set the timer to 0 to jump to the next "step."
        /// </summary>
        public void Jump() {
            waitTimer = 0;
        }

        public extern void orig_Update();
        public override void Update() {
            IEnumerator prev, next;
            do {
                prev = enumerators.Count > 0 ? enumerators.Peek() : null;
                orig_Update();
                next = enumerators.Count > 0 ? enumerators.Peek() : null;
            } while (prev != next && next != null && !(prev?.GetType()?.Assembly == typeof(Engine).Assembly || ForceDelayedSwap));
        }

    }
    public static class CoroutineExt {

        // Mods can't access patch_ classes directly.
        // We thus expose any new members through extensions.

        /// <inheritdoc cref="patch_Coroutine.Jump"/>
        public static void Jump(this Coroutine self)
            => ((patch_Coroutine) self).Jump();

    }
}
