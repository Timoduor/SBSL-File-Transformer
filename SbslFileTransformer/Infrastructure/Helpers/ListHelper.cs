using System.Collections.Generic;
using System.Linq;

namespace SbslFileTransformer.Infrastructure.Helpers
{
    public static class ListHelper
    {
        public static bool ContainsAllItems<T>(this List<T> listToCheck, List<T> listOfItemsThatMustExist)
        {
            return !listOfItemsThatMustExist.Except(listToCheck).Any();
        }
    }
}
