namespace QrCode_Reader.Helpers
{
    public static class QrHelper
    {
        public static int? ExtractClientId(string qrValue)
        {
            if (string.IsNullOrWhiteSpace(qrValue)) return null;
            if (!qrValue.StartsWith("UNID")) return null;

            var num = qrValue.Substring(4);
            if (int.TryParse(num, out int id))
                return id;

            return null;
        }
    }
}
