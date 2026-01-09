namespace QrCode_Reader.Helpers
{
    public static class QrHelper
    {
        /// <summary>
        /// Extracts the client ID from a QR code value.
        /// This value is expected to start with "UNID" followed by the numeric ID.
        /// </summary>
        /// <param name="qrValue"></param>
        /// <returns>Int ID</returns>
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
