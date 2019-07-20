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

#pragma warning disable CA1822

namespace CatLib.Tests.Fixture
{
    public class Foo : IFoo
    {
        public static string Echo(string input)
        {
            return input;
        }

        public int EchoInt(int input)
        {
            return input;
        }

        public float EchoFloat(float input)
        {
            return input;
        }

        public override string ToString()
        {
            return "foo";
        }
    }
}
