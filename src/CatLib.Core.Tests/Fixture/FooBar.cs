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

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CatLib.Tests.Fixture
{
    public class FooBar
    {
        public FooBar(Foo foo, Bar bar)
        {
            Assert.AreNotEqual(null, foo);
            Assert.AreNotEqual(null, bar);

            Foo = foo;
            Bar = bar;
        }

        public Foo Foo { get; private set; }
        public Bar Bar { get; private set; }

        public string GetName(Foo foo, Bar bar)
        {
            return foo.ToString() + bar.ToString();
        }

        public override string ToString()
        {
            return Foo.ToString() + Bar.ToString();
        }

        public static FooBar New()
        {
            return new FooBar(new Foo(), new Bar());
        }
    }
}
