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

namespace CatLib.Tests.Fixture
{
    [Variant]
    public class VariantModel
    {
        public string Name { get; }

        public VariantModel(int id)
        {
            if (id == -1)
            {
                throw new TestException("VariantModel");
            }

            if (id == 1)
            {
                Name = "iron man";
            }
            else if (id == 2)
            {
                Name = "black window";
            }
            else
            {
                Name = "Undefiend";
            }
        }
    }
}
