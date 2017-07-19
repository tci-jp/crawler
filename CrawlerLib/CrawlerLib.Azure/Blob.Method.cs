namespace AzureSearch.SDKHowTo
{
    using System;
    using System.Text;

    public partial class Blob
    {
        // This implementation of ToString() is only for the purposes of the sample console application.
        // You can override ToString() in your own model class if you want, but you don't need to in order
        // to use the Azure Search .NET SDK.
        public override string ToString()
        {
            var builder = new StringBuilder();

            if (!String.IsNullOrEmpty())
            {
                builder.AppendFormat("Url: {0}\t", BlobUrl);
            }

            if (!String.IsNullOrEmpty(Html))
            {
                builder.AppendFormat("Html: {0}\t", Html);
            }

            return builder.ToString();
        }
    }
}
