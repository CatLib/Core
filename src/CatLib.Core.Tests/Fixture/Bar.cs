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

using CatLib.Container;
using System;

namespace CatLib.Tests.Fixture
{
    public sealed class Bar : IDisposable
    {
        public Bar(int? num = null)
        {
            Num = num;
        }

        [Inject(Required = false)]
        public int? Age { get; set; } = 18;

        public int? Num { get; private set; }

        public bool Disposed { get; private set; } = false;

        public override string ToString()
        {
            return "bar";
        }

        public void Dispose()
        {
            Disposed = true;
        }
    }
}
