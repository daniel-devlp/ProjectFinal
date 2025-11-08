namespace Project.Domain.ValueObjects
{
    public class Identification
    {
        public string Type { get; private set; }
        public string Number { get; private set; }

        private Identification() { } // Para EF

        public Identification(string type, string number)
        {
            if (string.IsNullOrWhiteSpace(type))
                throw new ArgumentException("Identification type is required", nameof(type));
            
            if (string.IsNullOrWhiteSpace(number))
                throw new ArgumentException("Identification number is required", nameof(number));

            if (type == "Cedula" && !IsValidEcuadorianCedula(number))
                throw new ArgumentException("Invalid Ecuadorian cedula", nameof(number));

            Type = type;
            Number = number;
        }

        private static bool IsValidEcuadorianCedula(string identificationNumber)
        {
            if (!int.TryParse(identificationNumber, out _) || identificationNumber.Length != 10)
                return false;

            int[] coefficients = { 2, 1, 2, 1, 2, 1, 2, 1, 2 };
            var province = Convert.ToInt32(identificationNumber.Substring(0, 2));
            var thirdDigit = Convert.ToInt32(identificationNumber[2].ToString());

            if (province < 1 || province > 24 || thirdDigit >= 6)
                return false;

            var total = 0;
            for (var k = 0; k < coefficients.Length; k++)
            {
                var value = coefficients[k] * Convert.ToInt32(identificationNumber[k].ToString());
                total += value >= 10 ? value - 9 : value;
            }

            var verifierDigit = total >= 10 ? (total % 10) != 0 ? 10 - (total % 10) : 0 : total;
            return verifierDigit == Convert.ToInt32(identificationNumber[9].ToString());
        }

        protected bool Equals(Identification other)
        {
            return Type == other.Type && Number == other.Number;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Identification)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Type, Number);
        }
    }
}