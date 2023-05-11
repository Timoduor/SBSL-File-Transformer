using System.Collections.Generic;
using Newtonsoft.Json;

namespace SbslFileTransformer.Infrastructure.Jobs.Converters.Kenya.VisionFinacleMatcher
{
    public class VisionCommonHelpers
    {
        public static VisionRecordType GetVisionRecordType(string file)
        {
            VisionRecordType visionRecordType = VisionRecordType.Collections;

            if (
                (file.ToLower().Contains("credit_card") && file.ToLower().Contains("collections_cms")) ||
                (file.ToLower().Contains("credit_card") && file.ToLower().Contains("credit_bal")) ||
                (file.ToLower().Contains("credit_card") && file.ToLower().Contains("collections_gl"))
               )
            {
                visionRecordType = VisionRecordType.Collections;
            }

            if (
                (file.ToLower().Contains("credit_sett") && file.ToLower().Contains("vision")) ||
                (file.ToLower().Contains("credit_sett") && file.ToLower().Contains("finacle")) ||
                (file.ToLower().Contains("credit_sett") && file.ToLower().Contains("bal"))
               )
            {
                visionRecordType = VisionRecordType.CreditSettlement;
            }

            if (
                (file.ToLower().Contains("debtors") && file.ToLower().Contains("vision")) ||
                (file.ToLower().Contains("debtors") && file.ToLower().Contains("finacle")) ||
                (file.ToLower().Contains("debtors") && file.ToLower().Contains("bal"))
               )
            {
                visionRecordType = VisionRecordType.Debtors;
            }

            return visionRecordType;
        }

        public static List<TChild> ConvertParentToChild<TParent, TChild>(List<TParent> records)
        {
            var serializedParent = JsonConvert.SerializeObject(records);
            List<TChild> result = JsonConvert.DeserializeObject<List<TChild>>(serializedParent);

            return result;
        }
    }
}
