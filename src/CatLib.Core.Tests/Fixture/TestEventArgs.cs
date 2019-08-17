/*
 * This file is part of the CatLib package.
 *
 * (c) CatLib <support@catlib.io>
 *
 * For the full copyright and license information, please view the LICENSE
 * file that was distributed with this source code.
 *
 * Document: https://catlib.io/
 */

using CatLib.EventDispatcher;
using System;

namespace CatLib.Tests.Fixture
{
    public class TestEventArgs : EventArgs, IStoppableEvent
    {
        public bool IsPropagationStopped { get; private set; }

        public void StopPropagation()
        {
            IsPropagationStopped = true;
        }
    }
}
