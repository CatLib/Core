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
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CatLib.Tests.Fixture
{
    public sealed class Baz
    {
        public Baz(Foo foo, int boo = 100)
        {
            Boo = boo;
            Assert.AreNotEqual(null, foo);
            Assert.AreNotEqual(null, boo);
        }

        [Inject(Required = false)]
        public string Name { get; set; } = "baz";

        [Inject]
        public Bar Bar { get; set; }

        [Inject]
        public int Qux { get; set; }

        public int Boo { get; private set; }
    }
}
